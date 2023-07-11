Imports System.ComponentModel
Imports System
Imports System.Threading
Imports System.Reflection

Public Class Form1

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles Me.Load

        Dim assembly As Assembly = Assembly.GetExecutingAssembly()
        Dim version As Version = assembly.GetName().Version
        Label5.Text = $"version{version.ToString}"

        Dim fileName As String = $"{Application.StartupPath}\config.xml"

        'XmlSerializerオブジェクトを作成
        Dim serializer As New System.Xml.Serialization.XmlSerializer(
            GetType(SampleClass))
        '読み込むファイルを開く
        Try
            Dim sr As New System.IO.StreamReader(
            fileName, New System.Text.UTF8Encoding(False))
            'XMLファイルから読み込み、逆シリアル化する
            Dim obj As SampleClass =
            DirectCast(serializer.Deserialize(sr), SampleClass)

            TextBox1.Text = obj.MAC
            TextBox2.Text = obj.IP

            'ファイルを閉じる
            sr.Close()
        Catch ex As Exception
            TextBox1.Text = "XX:XX:XX:XX:XX:XX"
            TextBox2.Text = "192.168.XXX.XX"
        End Try

        Dim p As New System.Net.NetworkInformation.Ping()
        Try
            Dim reply As System.Net.NetworkInformation.PingReply = p.Send($"{TextBox2.Text }", 100)
            If reply.Status = System.Net.NetworkInformation.IPStatus.Success Then
                Label3.ForeColor = Color.Green
                Label4.Text = "起動済み。"
            Else
                Label3.ForeColor = Color.Black
                Label4.Text = "起動してません。"
            End If
        Catch ex As Exception
            Label3.ForeColor = Color.Gray
            Label4.Text = "状態不明"
        End Try

        Timer1.Enabled = False
    End Sub


    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Timer1.Enabled = False

        Dim strMAC As String = TextBox1.Text
        Dim strIP As String = TextBox2.Text

        '送信データを作成
        Dim intCounter As Integer = 0
        Dim sendBytes(0 To 101) As Byte

        '最初に&hFFを6個付ける
        For I = 1 To 6
            sendBytes(intCounter) = &HFF
            intCounter += 1
        Next
        'MACアドレスを16回繰り返す
        For I = 1 To 16
            'MACアドレス読込み
            For J = 0 To 5
                '16進数を変換して読込み
                sendBytes(intCounter) = Byte.Parse(strMAC.Substring(J * 3, 2), Globalization.NumberStyles.HexNumber)
                intCounter += 1
            Next
        Next

        'データを送信するポート番号
        Dim RemotePort As Integer = 2304 '何でも良い

        '送信先IP指定（ブロードキャストとピンポイントの両方に送信してみる）
        Dim BCIP As System.Net.IPAddress
        Dim EP As System.Net.IPEndPoint
        Dim UDP As New System.Net.Sockets.UdpClient 'UDP接続

        'ブロードキャストアドレス指定で送信するとき
        ''BCIP = System.Net.IPAddress.Parse("255.255.255.255")
        ''EP = New System.Net.IPEndPoint(BCIP, RemotePort)
        '''送信先を指定してデータを送信する
        ''UDP.Send(sendBytes, sendBytes.Length, EP)

        'ピンポイントのIPアドレス指定して送信するとき
        BCIP = System.Net.IPAddress.Parse(strIP)
        EP = New System.Net.IPEndPoint(BCIP, RemotePort)
        '送信先を指定してデータを送信する
        UDP.Send(sendBytes, sendBytes.Length, EP)

        'UDP接続を終了
        UDP.Close()

        Dim tp = New ThreadedPing
        tp.SetL(Label3, Label4, strIP)

    End Sub

    Private Sub Form1_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        Dim fileName As String = $"{Application.StartupPath}\config.xml"

        '保存するクラス(SampleClass)のインスタンスを作成
        Dim obj As New SampleClass()
        obj.MAC = TextBox1.Text
        obj.IP = TextBox2.Text

        'XmlSerializerオブジェクトを作成
        'オブジェクトの型を指定する
        Dim serializer As New System.Xml.Serialization.XmlSerializer(
            GetType(SampleClass))
        '書き込むファイルを開く（UTF-8 BOM無し）
        Dim sw As New System.IO.StreamWriter(
            fileName, False, New System.Text.UTF8Encoding(False))
        'シリアル化し、XMLファイルに保存する
        serializer.Serialize(sw, obj)
        'ファイルを閉じる
        sw.Close()
    End Sub



    Public Class ThreadedPing
        Private L3, L4 As Label
        Private strIP = ""
        Private once = False

        Private Delegate Sub D_L4UpdText(ByVal txt As String)
        Private Delegate Sub D_L3UpdCol(ByVal c As Color)

        Public Sub L4UpdText(ByVal txt As String)
            L4.Text = txt
            L4.Update()
        End Sub
        Public Sub L3UpdCol(ByVal c As Color)
            L3.ForeColor = c
            L3.Update()
        End Sub

        Public Sub SetL(l1 As Label, l2 As Label, s As String, Optional ByVal f As Boolean = False)
            L3 = l1
            L4 = l2
            strIP = s
            once = f

            Dim thread As New Thread(New ThreadStart(AddressOf ThreadMethod))
            thread.IsBackground = True
            thread.Start()
        End Sub

        ' 別スレッドで動作させるメソッド
        Private Sub ThreadMethod() ' （6）

            Dim p As New System.Net.NetworkInformation.Ping()
            L3.Invoke(New D_L3UpdCol(AddressOf L3UpdCol), Color.Gray)
            For i = 0 To If(once, 0, 30)
                L4.Invoke(New D_L4UpdText(AddressOf L4UpdText), $"{i}/30")

                Dim reply As System.Net.NetworkInformation.PingReply = p.Send($"{strIP}", 100)
                If reply.Status = System.Net.NetworkInformation.IPStatus.Success Then
                    'L3.ForeColor = Color.Green
                    L3.Invoke(New D_L3UpdCol(AddressOf L3UpdCol), Color.Green)
                    L4.Invoke(New D_L4UpdText(AddressOf L4UpdText), $"{i}:起動済み。")
                    Exit For
                Else
                    'L3.ForeColor = Color.Gray
                    L3.Invoke(New D_L3UpdCol(AddressOf L3UpdCol), Color.Gray)
                End If
                If Not once Then Thread.Sleep(1000)
            Next
        End Sub
    End Class

    Private ttp As ThreadedPing
    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        If ttp Is Nothing Then
            ttp = New ThreadedPing
        End If
        ttp.SetL(Label3, Label4, TextBox2.Text)
    End Sub
End Class



'XMLファイルに保存するオブジェクトのためのクラス
Public Class SampleClass
    Public IP As String
    Public MAC As String
End Class


