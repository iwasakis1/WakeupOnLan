Imports System.ComponentModel

Public Class Form1



    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
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

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles Me.Load
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
    End Sub
End Class

'XMLファイルに保存するオブジェクトのためのクラス
Public Class SampleClass
    Public IP As String
    Public MAC As String
End Class

