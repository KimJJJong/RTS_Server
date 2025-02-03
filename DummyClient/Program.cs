﻿using ServerCore;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DummyClient
{
	

	class Program
	{
		static void Main(string[] args)
		{
			// DNS (Domain Name System)
			string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[0];
			IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("172.30.1.100")/*ipAddr*/, 13221);

			Connector connector = new Connector();

			connector.Connect(endPoint,
				() => { return SessionManager.Instance.Generate(); } ,
				 1 );

			while (true)
			{
				try
				{
					//SessionManager.Instance.SendForEach();
				}
				catch (Exception e)
				{
					Console.WriteLine(e.ToString());
				}

				Thread.Sleep(250);
			}
		}
	}
}
