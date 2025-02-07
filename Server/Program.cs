﻿using System;
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
		public static GameRoom Room = new GameRoom();

       /* static void FlushRoom()
        {
            Room.Push(() => Room.Flush());
            JobTimer.Instance.Push(FlushRoom, 1000);
            
        }*/
        static void FlushLobby()
        {
            Lobby.Push(() => Lobby.Flush());
            JobTimer.Instance.Push(FlushLobby, 250);
        }


        static void Main(string[] args)
        {
            // DNS (Domain Name System)
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(/*IPAddress.Parse("172.30.1.5")*/ipAddr, 13221);

            _listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
            Console.WriteLine("Listening...");

            //Flush();
            //JobTimer.Instance.Push(FlushRoom);
            JobTimer.Instance.Push(FlushLobby);
            //Room.StartSyncTimer();

            while (true)
            {
                JobTimer.Instance.Flush();
            }
        }
    }
}
