using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace JankenClient1
{
    class C
    {
        const int PORT = 11000;

        public static void Main()
        {
            Console.WriteLine("===== JankenClient1 =====");

            SocketClient();

            Console.ReadKey();
        }

        public static void SocketClient()
        {
            // 名前入力
            Console.Write("名前を入力してください：");
            string playerName = Console.ReadLine();

            // JankenRoomのIP
            string hostName = Dns.GetHostName();
            IPHostEntry ipHostInfo = Dns.GetHostEntry(hostName);

            IPAddress ipAddress = ipHostInfo.AddressList[1];

            IPEndPoint remoteEP =
                new IPEndPoint(ipAddress, PORT);

            // ソケット作成
            Socket socket = new Socket(
                ipAddress.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            // 接続
            try
            {
                socket.Connect(remoteEP);
            }
            catch (Exception e)
            {
                Console.WriteLine("JankenRoomへの接続に失敗しました。");
                Console.WriteLine(e.Message);
                return;
            }

            Console.WriteLine("JankenRoomに接続しました。");

            // 名前を送信
            SendString(socket, playerName);

            // ゲーム開始メッセージ受信
            string message = ReceiveString(socket);

            Console.WriteLine();
            Console.WriteLine(message);

            // 手を入力
            Console.WriteLine();
            Console.Write("あなたの手：");

            string hand = Console.ReadLine();

            // 手を送信
            SendString(socket, hand);

            // 結果を受信
            string result = ReceiveString(socket);

            Console.WriteLine();
            Console.WriteLine(result);

            // 終了
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        static void SendString(Socket socket, string data)
        {
            byte[] msg =
                Encoding.UTF8.GetBytes(data + "<EOF>");

            socket.Send(msg);
        }

        static string ReceiveString(Socket socket)
        {
            byte[] bytes = new byte[1024];

            int bytesRec = socket.Receive(bytes);

            return Encoding.UTF8
                .GetString(bytes, 0, bytesRec)
                .Replace("<EOF>", "")
                .Trim();
        }
    }
}