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
		/// <summary>
		/// Init ListenerSocket on the Server
		/// </summary>
		/// <param name="endPoint">IP Addresss, Port</param>
		/// <param name="sessionFactory">Function to create a new session for incoming client connections</param>
		/// <param name="register">ListenerNum</param>
		/// <param name="backlog">backLog</param>
		public void Init(IPEndPoint endPoint, Func<Session> sessionFactory, int register = 10, int backlog = 100)
		{
            // Create the Listener socket
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			// Sotre the session creation Fun
			_sessionFactory += sessionFactory;
			
			_listenSocket.Bind(endPoint);

			// Start Listening 
			_listenSocket.Listen(backlog);


			for(int i = 0; i < register; i++)
			{
			SocketAsyncEventArgs args = new SocketAsyncEventArgs();
			args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
			// First Register( Casting )
			RegisterAccept(args);

			}
		}

		void RegisterAccept(SocketAsyncEventArgs args)
		{
			args.AcceptSocket = null;

			bool pending = _listenSocket.AcceptAsync(args);
			// pending == false : no delay excution
			if (pending == false)
				OnAcceptCompleted(null, args);
		}

		// Multi Threading Start : BeCareful RaceCondition 
		void OnAcceptCompleted(object sender, SocketAsyncEventArgs args /* CallBack Parameter : Clinet delegate Socket */ )
		{
			try
			{
                if (args.SocketError == SocketError.Success)
                {
                    Session session = _sessionFactory.Invoke();
                    session.Start(args.AcceptSocket);
                    session.OnConnected(args.AcceptSocket.RemoteEndPoint);
                }
                else
                    Console.WriteLine(args.SocketError.ToString());
            }
			catch (Exception e)
			{
				Console.WriteLine($"Err Connect Fail : { e.Message }");
			}
			finally
			{
			// Re Casting
			RegisterAccept(args);
			}
		}
	}
}
