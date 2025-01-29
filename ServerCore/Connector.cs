﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
	public class Connector
	{
		Func<Session> _sessionFactory;

		public void Connect(IPEndPoint endPoint, Func<Session> sessionFactory)
		{
			// 휴대폰 설정
			Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			_sessionFactory = sessionFactory;

			SocketAsyncEventArgs args = new SocketAsyncEventArgs();
			args.Completed += OnConnectCompleted;
			args.RemoteEndPoint = endPoint;
			args.UserToken = socket;

			RegisterConnect(args);
		}

		void RegisterConnect(SocketAsyncEventArgs args)
		{
			Socket socket = args.UserToken as Socket;	
			if (socket == null)
				return;

			bool pending = socket.ConnectAsync(args);
			if (pending == false)
				OnConnectCompleted(null, args);
		}

		void OnConnectCompleted(object sender, SocketAsyncEventArgs args)
		{
			if (args.SocketError == SocketError.Success)
			{
				Session session = _sessionFactory.Invoke();
				session.Start(args.ConnectSocket ); // args.UserToken as Socket : 두가지 방법
                session.OnConnected(args.RemoteEndPoint);
			}
			else
			{
				Console.WriteLine($"OnConnectCompleted Fail: {args.SocketError}");
			}
		}
	}
}
