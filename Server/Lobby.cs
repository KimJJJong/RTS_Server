using System;
using System.Collections.Generic;
using Shared;

namespace Server
{
    class Lobby : IJobQueue
    {
        Dictionary<string, GameRoom> _rooms = new Dictionary<string, GameRoom>();
        Dictionary<string, GameRoom> _matchingRoom = new Dictionary<string, GameRoom>();
        Queue<ClientSession> _waitingQueue = new Queue<ClientSession>(); // 매칭 대기열

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
        public void BroadCast()
        {
            // 근데 Lobby에서 BroadCast해야 할게 있을까?
        }
        public void Enter(ClientSession session)
        {
            Console.WriteLine($"Player {session.SessionID} join Lobby");
            LogManager.Instance.LogInfo("Lobby", $"Player {session.SessionID} joined lobby");

            _sessions.Add(session);
            session.Lobby = this;
        }
        public void Leave(ClientSession session)
        {
            Console.WriteLine($"클라이언트 {session.SessionID}가 로비 에서 퇴장 ");
            LeaveMatchQueue(session);
            _sessions.Remove(session);
        }

        #region Room
        public string CreateRoom()
        {
            GameRoom room = new GameRoom();
               _rooms.Add(room.RoomId, room);
            //_rooms.Add("testRoom", room);       // TODO : Del this code when Test Over
            return room.RoomId;
        }
        public GameRoom FindRoom(string roomId)
        {
            if (_rooms.TryGetValue(roomId, out GameRoom room))
                return room;
            return null;
        }

        public void RemoveRoom(string roomId)
        {
            if (_rooms.TryGetValue(roomId, out GameRoom room))
            {
               // room.Clear(); // 내부 데이터 정리 (선택 사항)

                // Room을 참조하는 세션도 Room 참조를 해제해야 함
                foreach (var session in room.Sessions)
                    session.Value.Room = null;

                _rooms.Remove(roomId); // Dictionary에서 제거
            }
        }
        public List<string> GetRoomList()
        {
            return new List<string>(_rooms.Keys);
        }


        #endregion

        #region MatchingRoom
        public void EnterMatchQueue(ClientSession session)
        {

                _waitingQueue.Enqueue(session);
                TryMatch();
        }
        private void TryMatch()
        {
            while (_waitingQueue.Count >= 2) // 1vs1 기준
            {
                ClientSession player1 = _waitingQueue.Dequeue();
                ClientSession player2 = _waitingQueue.Dequeue();

                string roomId = Guid.NewGuid().ToString().Substring(0, 6); ; // 랜덤 ID 생성
                GameRoom room = new GameRoom(roomId);
                _matchingRoom[roomId] = room;

                //room.Push(() => 
                room.Enter(player1);
                //room.Push(() => 
                room.Enter(player2);

                Console.WriteLine($"Matched {player1.SessionID} vs {player2.SessionID} in Room {roomId}");
                LogManager.Instance.LogInfo("Lobby", $"Matched {player1.SessionID} vs {player2.SessionID} in {roomId}");

                room.BothReady();
                //room.StartGame();
            }
        }

        public void LeaveMatchQueue(ClientSession session)
        {
     
                if (_waitingQueue.Contains(session))
                {
                    List<ClientSession> tempList = new List<ClientSession>(_waitingQueue);
                    tempList.Remove(session);
                    _waitingQueue = new Queue<ClientSession>(tempList);
                Console.WriteLine($"Cleint [{ session.SessionID }] Leave MatchQueue");
                }
            else
            {
                Console.WriteLine($"Err : _waitingQueue not enought");
            }
        }

        #endregion

    }
}
