﻿using ServerCore;
using Shared;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace Server
{
    class SessionManager
    {
        static SessionManager _session = new SessionManager();
        public static SessionManager Instance { get { return _session; } }

        int _sessionId = 0;
        Dictionary<int, ClientSession> _sessions = new Dictionary<int, ClientSession>();
        object _lock = new object();

        public ClientSession Generate()
        {
            lock (_lock)
            {
                int sessionId = ++_sessionId;

                ClientSession session = new ClientSession();
                session.SessionID = sessionId;

                _sessions.Add(sessionId, session);
                Console.WriteLine($"Connected : {sessionId}");

                return session;

            }
        }
        public void Add(ClientSession clientSession)
        {
            lock (_lock)
            {
                int sessionId = ++_sessionId;

                clientSession.SessionID = sessionId;

                _sessions.Add(sessionId, clientSession);
                Console.WriteLine($"Connected : {sessionId}");
            }
        }
        public ClientSession Find(int id)
        {
            lock (_lock)
            {
                ClientSession session = null;
                _sessions.TryGetValue(id, out session);
                return session;
            }
        }

        public void Remove(ClientSession session)
        {
            lock (_lock)
            {
                _sessions.Remove(session.SessionID);
            }
        }
    }
}
  