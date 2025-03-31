using ServerCore;
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
    /*  class SessionManager
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

                  LogManager.Instance.LogInfo("Session", $"Connected from {session}");

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
      }*/

    // class SessionManager
    //{
    //    public static SessionManager Instance { get; } = new SessionManager();

    //    private int _sessionId = 0;
    //    private const int MaxPoolSize = 10;

    //    private readonly object _lock = new object();
    //    private readonly Dictionary<int, ClientSession> _sessions = new Dictionary<int, ClientSession>();
    //    private readonly ConcurrentBag<ClientSession> _sessionPool = new ConcurrentBag<ClientSession>();

    //    public ClientSession Generate()
    //    {
    //        ClientSession session;

    //        if (!_sessionPool.TryTake(out session))
    //        {
    //            session = new ClientSession();
    //        }
    //        else
    //        {
    //            session.Reset(); // 풀에서 꺼낸 세션은 초기화
    //        }

    //        session.SessionID = Interlocked.Increment(ref _sessionId);

    //        lock (_lock)
    //        {
    //            _sessions[session.SessionID] = session;
    //        }

    //        return session;
    //    }

    //    public void Remove(ClientSession session)
    //    {
    //        session.CleanUp();
    //        lock (_lock)
    //        {
    //            _sessions.Remove(session.SessionID);
    //        }

    //        if (_sessionPool.Count < MaxPoolSize)
    //        {
    //            _sessionPool.Add(session);
    //        }
    //        else
    //        {
    //            LogManager.Instance.LogDebug("SessionManager", $"MaxPoolSize 초과로 세션 폐기: {session.SessionID}");
    //            // 필요 시 세션 자원 정리
    //        }
    //    }

    //    public ClientSession Get(int sessionId)
    //    {
    //        lock (_lock)
    //        {
    //            _sessions.TryGetValue(sessionId, out var session);
    //            return session;
    //        }
    //    }

    //    public IReadOnlyDictionary<int, ClientSession> Sessions
    //    {
    //        get
    //        {
    //            lock (_lock)
    //            {
    //                return new Dictionary<int, ClientSession>(_sessions);
    //            }
    //        }
    //    }

    //    public void Clear()
    //    {
    //        lock (_lock)
    //        {
    //            _sessions.Clear();
    //        }

    //        while (_sessionPool.TryTake(out var _)) { }
    //    }
    //}



