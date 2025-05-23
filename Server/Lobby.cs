using System;
using System.Collections.Generic;
using Shared;

    public class Lobby : IJobQueue
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
            //Console.WriteLine("LobbyFlush");
            foreach (ClientSession session in _sessions)
                session.Send(_pendingList);

            _pendingList.Clear();
        }

        public void Enter(ClientSession session)
        {
            //Console.WriteLine($"Player {session.SessionID} join Lobby");
            LogManager.Instance.LogInfo("Lobby", $"Player {session.SessionID} joined lobby");

            _sessions.Add(session);
          //  session.Lobby = this;
        }
        public void Leave(ClientSession session)
        {
            Console.WriteLine($"클라이언트 {session.SessionID}가 로비 에서 퇴장 ");
            _sessions.Remove(session);
        }

        #region Room
        //public string CreateRoom()
        //{
        //    GameRoom room = new GameRoom();
        //       _rooms.Add(room.RoomId, room);
        //    //_rooms.Add("testRoom", room);       // TODO : Del this code when Test Over
        //    return room.RoomId;
        //}
        //public GameRoom FindRoom(string roomId)
        //{
        //    if (_rooms.TryGetValue(roomId, out GameRoom room))
        //        return room;
        //    return null;
        //}

        //public void RemoveRoom(string roomId)
        //{
        //    if (_rooms.TryGetValue(roomId, out GameRoom room))
        //    {
        //       // room.Clear(); // 내부 데이터 정리 (선택 사항)

        //        // Room을 참조하는 세션도 Room 참조를 해제해야 함
        //        foreach (var session in room.Sessions)
        //            session.Value.Room = null;

        //        _rooms.Remove(roomId); // Dictionary에서 제거
        //    }
        //}
        //public List<string> GetRoomList()
        //{
        //    return new List<string>(_rooms.Keys);
        //}

        #endregion

    }

