using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DummyClient
{
     class SessionManager
    {
        static SessionManager _session= new SessionManager();
        public static SessionManager Instance {  get { return _session; } }
        List<ServerSession> _sessions = new List<ServerSession> ();
        object _lock = new object ();

        public void SendForEach()
        {
            lock (_lock)
            {
                foreach (ServerSession session in _sessions)
                {
                    C_ReqSummon sumPacket = new C_ReqSummon ();
                    sumPacket.x= 1;
                    sumPacket.y= 1;
                    sumPacket.uid = 12;
                    ArraySegment<byte> segment = sumPacket.Write();

                    session.Send(segment);
                }
            }
        }

        public ServerSession Generate()
        {
            lock (_lock)
            {
            ServerSession session = new ServerSession();
            _sessions.Add(session);
            return session;
            }
        }
    }
}
