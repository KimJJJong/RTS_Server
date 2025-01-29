using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server;
using ServerCore;

namespace Server
{
    class GameRoom :IJobQueue
    {
        List<ClientSession> _sessions = new List<ClientSession>();
        JobQueue _jobQueue = new JobQueue();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();

        public void Push(Action action)
        {
            _jobQueue.Push(action);
        }

        public void Flush()
        {
            foreach(ClientSession session in _sessions) 
                session.Send(_pendingList);

            Console.WriteLine($"Flushed {_pendingList.Count} items");
            _pendingList.Clear();   
        }

        public void BroadCast(ClientSession session)
        {
        }

        public void Enter(ClientSession session)
        {
            _sessions.Add(session);
             session.Room = this;
            
        }

        public void Leave(ClientSession session)
        {

            _sessions.Remove(session);

        }

    }
}
