using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
	public class Listener
	{
		Socket _listenSocket;
		Func<Session> _sessionFactory;

		public void Init(IPEndPoint endPoint, Func<Session> sessionFactory)
		{
			_listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			_sessionFactory += sessionFactory;

			// 문지기 교육
			_listenSocket.Bind(endPoint);

			// 영업 시작
			// backlog : 최대 대기수
			_listenSocket.Listen(10);

			SocketAsyncEventArgs args = new SocketAsyncEventArgs();
			args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
			// 최초 등록 ( Casting )
			RegisterAccept(args);
		}

		void RegisterAccept(SocketAsyncEventArgs args)
		{
			args.AcceptSocket = null;

			bool pending = _listenSocket.AcceptAsync(args);
			// pending이 false이면 기다림 없이 바로 실행이 되었다는 뜻
			if (pending == false)
				OnAcceptCompleted(null, args);
		}

		// Multi Thread의 시작 RaceCondition 주의
		void OnAcceptCompleted(object sender, SocketAsyncEventArgs args /* CallBack함수 인자 : Clinet 대리자Socket */ )
		{
			if (args.SocketError == SocketError.Success)
			{
				Session session = _sessionFactory.Invoke();
				session.Start(args.AcceptSocket);
				session.OnConnected(args.AcceptSocket.RemoteEndPoint);
			}
			else
				Console.WriteLine(args.SocketError.ToString());

			// 다시 Casting
			RegisterAccept(args);
		}
	}
}
