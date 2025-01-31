using System;
using System.Collections.Generic;

namespace Server
{
    class Lobby : IJobQueue
    {
        Dictionary<string, GameRoom> _rooms = new Dictionary<string, GameRoom>();
        List<ClientSession> _sessions = new List<ClientSession>();
        List<ArraySegment<byte>> _pendingList =new List<ArraySegment<byte>>();
        JobQueue _jobQueue = new JobQueue();

        public void Push(Action action)
        {
            _jobQueue.Push(action);
        }
        public void Flush()
        {
            foreach (ClientSession session in _sessions)
                session.Send(_pendingList);

            _pendingList.Clear();
        }
        public void BroadCast()
        {
            // 근데 Lobby에서 BroadCast해야 할게 있을까?
        }
        public void Enter(ClientSession session)
        {
            _sessions.Add(session);
            session.Lobby = this;
        }
        public void Leave(ClientSession session)
        {
            _sessions.Remove(session);
        }

        #region Room
        public string GenerateRoom()
        {
            GameRoom room = new GameRoom();
            _rooms.Add(room.RoomId, room);
            return room.RoomId;
        }

        public GameRoom FindRoom(string roomId)
        {
            if (_rooms.TryGetValue(roomId, out GameRoom room))
                return room;
            return null;
        }

        public bool RemoveRoom(string roomId)
        {
            if (_rooms.ContainsKey(roomId))
            {
                _rooms.Remove(roomId);
                return true;
            }
            return false;
        }
        public List<string> GetRoomList()
        {
            return new List<string>(_rooms.Keys);
        }


        #endregion

    }
}
