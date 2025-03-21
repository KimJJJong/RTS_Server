using ServerCore;
using System;
using System.Collections.Generic;

namespace Server
{
    /*class SessionManager
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

                ClientSession session = ClientSessionPool.Instance.GetSession();
                session.SessionID = sessionId;
                _sessions.Add(sessionId, session);
                Console.WriteLine($"Connected : {sessionId}");

                return session;

            }
        }
*//*        public void Add(ClientSession clientSession)
        {
          lock ( _lock)
            {
                int sessionId = ++_sessionId;

                clientSession.SessionID = sessionId;

                _sessions.Add(sessionId, clientSession);
                Console.WriteLine($"Connected : {sessionId}");
            }
        }*//*
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
    }*/
    class SessionManager
    {
        static SessionManager _session = new SessionManager();
        public static SessionManager Instance { get { return _session; } }

        int _sessionId = 0;
        Dictionary<int, ClientSession> _sessions = new Dictionary<int, ClientSession>();
        Queue<ClientSession> _sessionPool = new Queue<ClientSession>(); // 객체 풀

        object _lock = new object();

        public ClientSession Generate()
        {
            lock (_lock)
            {
                ClientSession session = null;

                if (_sessionPool.Count > 0) // 기존 객체 재사용
                {
                    session = _sessionPool.Dequeue();
                }
                else
                {
                    session = new ClientSession(); // 없으면 새로 생성
                }

                int sessionId = ++_sessionId;
                session.SessionID = sessionId;
                _sessions.Add(sessionId, session);
                

                Console.WriteLine($"Connected : {sessionId}");
                return session;
            }
        }

        public void Remove(ClientSession session)
        {
            lock (_lock)
            {
                _sessions.Remove(session.SessionID);

                session.Reset(); // 세션 데이터 초기화

                if (_sessionPool.Count < 5) // 풀 크기 제한
                {
                    _sessionPool.Enqueue(session);
                }
                else
                {
                    // 풀에 저장할 공간이 없으면, 세션을 완전히 삭제해 메모리 해제
                    session = null;
                }
            }
        }
    }


}
