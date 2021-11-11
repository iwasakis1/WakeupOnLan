Public Class Form1
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim strIP As String = "192.168.1.1" 'IPアドレス
        Dim strMAC As String = "00-00-00-00-00-00" 'マックアドレス

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
        BCIP = System.Net.IPAddress.Parse("255.255.255.255")
        EP = New System.Net.IPEndPoint(BCIP, RemotePort)
        '送信先を指定してデータを送信する
        UDP.Send(sendBytes, sendBytes.Length, EP)

        'ピンポイントのIPアドレス指定して送信するとき
        BCIP = System.Net.IPAddress.Parse(strIP)
        EP = New System.Net.IPEndPoint(BCIP, RemotePort)
        '送信先を指定してデータを送信する
        UDP.Send(sendBytes, sendBytes.Length, EP)

        'UDP接続を終了
        UDP.Close()
    End Sub
End Class
