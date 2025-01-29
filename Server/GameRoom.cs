using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server;

namespace Server
{
    class GameRoom
    {
        List<ClientSession> _sessions = new List<ClientSession>();
        object _lock = new object();

        public void BroadCast(ClientSession session)
        {
            foreach()
        }

        public void Enter(ClientSession session)
        {
            lock (_lock)
            {
            _sessions.Add(session);
             session.Room = this;

            }
            
        }

        public void Leave(ClientSession session)
        {
            lock(_lock)
            {
            _sessions.Remove(session);

            }
        }

    }
}
