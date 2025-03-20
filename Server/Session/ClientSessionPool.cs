//using System;
//using System.Collections.Generic;
//using Server;

//class ClientSessionPool
//{
//    private Stack<ClientSession> _pool = new Stack<ClientSession>();
//    object _lock = new object();
//    private ClientSessionPool() { }
//    public static ClientSessionPool Instance { get; } = new ClientSessionPool();
//    public ClientSession GetSession()
//    {
//        lock (_lock)
//        {
//            if (_pool.Count > 0)
//            {
//                ClientSession session = _pool.Pop();
//                //session.CleanUp(); // ReSet Before ReUse;
//                return session;
//            }
//        }
//        Console.WriteLine($"PoolSize : {_pool.Count}");
//        return new ClientSession();
//    }

//    public void ReturnSession(ClientSession session)
//    {
//        session.CleanUp();

//        lock (_lock)
//        {
//            _pool.Push(session);
//        }
//    }

//}