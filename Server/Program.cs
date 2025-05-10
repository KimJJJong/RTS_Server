using System;
using System.Net;
using ServerCore;
using Shared;

namespace Server
{
	

	class Program
	{
		static Listener _listener = new Listener();
        public static Lobby Lobby = new Lobby();

        static void FlushLobby()
        {
            Lobby.Push(() => Lobby.Flush());
            JobTimer.Instance.Push(FlushLobby, 250);
        }

        public static double currentTime;
        static void Main(string[] args)
        {

            // 프로그램 종료 시 로그 매니저도 종료되도록 이벤트에 등록
            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                LogManager.Instance.Shutdown();
            };

            // Ctrl+C, X버튼 등으로 직접 종료할 때의 이벤트 등록
            Console.CancelKeyPress += (sender, e) =>
            {
                LogManager.Instance.Shutdown();
                e.Cancel = true;
            };

            //currentTime = DateTime.UtcNow.Ticks * 1e-7;
            // DNS (Domain Name System)
            
            UnitStatDatabase.Load();
            CardMetaDatabase.Load();


            Lobby.CreateRoom();         // TODO : Del this code when Test Over
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("192.168.0.2")/*ipAddr*/, 13221);

            _listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
            LogManager.Instance.LogInfo("Program", "[Server Start]");
            Console.WriteLine("Listening...");

            JobTimer.Instance.Push(FlushLobby);

            while (true)
            {
                JobTimer.Instance.Flush();
            }
        }
    }
}
