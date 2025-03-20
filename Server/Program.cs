using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;

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
            // DNS (Domain Name System)
            
            Lobby.CreateRoom();         // TODO : Del this code when Test Over
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(/*IPAddress.Parse("192.168.52.119")*/ipAddr, 13221);

            _listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
            Console.WriteLine("Listening...");

            JobTimer.Instance.Push(FlushLobby);

            while (true)
            {
                JobTimer.Instance.Flush();
            }
        }
    }
}
