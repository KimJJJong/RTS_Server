using System;
using System.Net;
using System.Threading.Tasks;
using ServerCore;
using Shared;

namespace Server
{
    class Program
    {
        static Listener _listener = new Listener();
        //static HttpServer _httpServer;

        //static void FlushLobby()
        //{
        //    JobTimer.Instance.Push(FlushLobby, 250);
        //}
        static void FlushMatchQueue()
        {
            while (true)
            {
                var match = RedisMatchQueue.Dequeue();
                if (match == null)
                    break;

                string roomId = GameRoomManager.Instance.CreateRoom(match);
                Console.WriteLine($"[GameServer] Created room {roomId} for players: {string.Join(", ", match)}");
            }
            JobTimer.Instance.Push(FlushMatchQueue, 500);
        }

        static void Main(string[] args)
        {
            // 프로그램 종료 시 로그 매니저 종료
            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                LogManager.Instance.Shutdown();
            };

            // TCP Listener (클라이언트 연결 처리)
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 13221);

            _listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
            LogManager.Instance.LogInfo("Program", "[Game Server Start]");
            Console.WriteLine("Listening...");

            //JobTimer.Instance.Push(FlushLobby);
            JobTimer.Instance.Push(FlushMatchQueue);

            // HTTP API 서버 실행 (로비 서버로부터 매칭 수신)
            //_httpServer = new HttpServer();
            //await _httpServer.Start(13222);

            while (true)
            {
                JobTimer.Instance.Flush();
            }
        }
    }
}
