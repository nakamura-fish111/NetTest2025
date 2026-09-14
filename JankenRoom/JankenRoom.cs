using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace JankenRoom
{
    class S
    {
        const int PORT = 11000;

        public static void Main()
        {
            Console.WriteLine("===== JankenRoom =====");
            SocketServer();
            Console.ReadKey();
        }

        public static void SocketServer()
        {
            // IPアドレス設定
            string hostName = Dns.GetHostName();
            IPHostEntry ipHostInfo = Dns.GetHostEntry(hostName);

            IPAddress ipAddress = ipHostInfo.AddressList[1];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, PORT);

            // サーバーソケット作成
            Socket listener = new Socket(
                ipAddress.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            listener.Bind(localEndPoint);
            listener.Listen(2);

            Console.WriteLine($"IP : {ipAddress}");
            Console.WriteLine($"Port : {PORT}");
            Console.WriteLine();
            Console.WriteLine("プレイヤー1の接続を待っています...");

            // ホスト接続
            Socket host = listener.Accept();
            Console.WriteLine("ホストが接続しました。");

            // クライアント接続
            Console.WriteLine("クライアントの接続を待っています...");
            Socket client = listener.Accept();
            Console.WriteLine("クライアントが接続しました。");

            // 名前を受信
            string hostNameData = ReceiveString(host);
            string clientNameData = ReceiveString(client);

            Console.WriteLine();
            Console.WriteLine($"ホスト名    : {hostNameData}");
            Console.WriteLine($"クライアント名 : {clientNameData}");

            // じゃんけん開始通知
            SendString(host, "じゃんけんゲーム開始！\r\n0:ぐー　1:ちょき　2:ぱー");
            SendString(client, "じゃんけんゲーム開始！\r\n0:ぐー　1:ちょき　2:ぱー");

            // 手を受信
            string hostHandData = ReceiveString(host);
            string clientHandData = ReceiveString(client);

            Console.WriteLine();
            Console.WriteLine($"ホストの手    : {hostHandData}");
            Console.WriteLine($"クライアントの手 : {clientHandData}");

            // 手を数字に変換
            bool hostValid = int.TryParse(hostHandData, out int hostHand);
            bool clientValid = int.TryParse(clientHandData, out int clientHand);

            string resultHost;
            string resultClient;

            // 勝敗判定
            if (!hostValid || !clientValid ||
                hostHand < 0 || hostHand > 2 ||
                clientHand < 0 || clientHand > 2)
            {
                resultHost = "無効な手が入力されました。";
                resultClient = "無効な手が入力されました。";
            }
            else if (hostHand == clientHand)
            {
                resultHost = "あいこ";
                resultClient = "あいこ";
            }
            else if ((hostHand + 1) % 3 == clientHand)
            {
                resultHost = hostNameData + "人生の勝者！";
                resultClient = clientNameData + "負け犬";
            }
            else
            {
                resultHost = hostNameData + "人生の勝者！";
                resultClient = clientNameData + "負け犬";
            }

            // 手の名前
            string hostHandName = GetHandName(hostHand);
            string clientHandName = GetHandName(clientHand);

            // ホストに結果を送信
            string hostResult =
                "===== 結果 =====\r\n" +
                $"{hostNameData} : {hostHandName}\r\n" +
                $"{clientNameData} : {clientHandName}\r\n" +
                "\r\n" +
                resultHost;

            // クライアントに結果を送信
            string clientResult =
                "===== 結果 =====\r\n" +
                $"{hostNameData} : {hostHandName}\r\n" +
                $"{clientNameData} : {clientHandName}\r\n" +
                "\r\n" +
                resultClient;

            SendString(host, hostResult);
            SendString(client, clientResult);

            Console.WriteLine();
            Console.WriteLine("結果を両プレイヤーに送信しました。");

            // 終了
            host.Shutdown(SocketShutdown.Both);
            host.Close();

            client.Shutdown(SocketShutdown.Both);
            client.Close();

            listener.Close();

            Console.WriteLine("JankenRoomを終了しました。");
        }

        // データ受信
        static string ReceiveString(Socket socket)
        {
            byte[] bytes = new byte[1024];

            int bytesRec = socket.Receive(bytes);

            return Encoding.UTF8
                .GetString(bytes, 0, bytesRec)
                .Replace("<EOF>", "")
                .Trim();
        }

        // データ送信
        static void SendString(Socket socket, string data)
        {
            byte[] msg = Encoding.UTF8.GetBytes(data + "<EOF>");
            socket.Send(msg);
        }

        // じゃんけんの手を文字に変換
        static string GetHandName(int hand)
        {
            return hand switch
            {
                0 => "ぐー",
                1 => "ちょき",
                2 => "ぱー",
                _ => "不明"
            };
        }
    }
}