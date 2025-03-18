using System;
using System.Collections.Generic;

namespace Server
{
    enum RoomState
    {
        Waiting,   // 대기 중 (플레이어 모집)
        InGame,    // 게임 진행 중
        Finished   // 게임 종료됨
    }

    class GameRoom : IJobQueue
    {
        public string RoomId { get; }
        private Dictionary<int, ClientSession> _sessions = new Dictionary<int, ClientSession>();
        private JobQueue _jobQueue = new JobQueue();
        private RoomState _roomState;
        public GameLogicManager GameLogic { get; private set; }
        public IReadOnlyDictionary<int, ClientSession> Sessions => _sessions;

        public GameRoom()
        {
            RoomId = Guid.NewGuid().ToString().Substring(0, 5);
            _roomState = RoomState.Waiting;
        }
        public GameRoom(string roomId)
        {
            RoomId = roomId;
            _roomState = RoomState.Waiting;
        }

        public void Push(Action action) => _jobQueue.Push(action);

        public void BroadCast(ArraySegment<byte> segment)
        {
            foreach (var session in _sessions.Values)
            {
                session.Send(segment);
            }
        }

        public void Enter(ClientSession session)
        {
            if (_roomState != RoomState.Waiting)
            {
                Console.WriteLine($"방 {RoomId}은 이미 게임 중이므로 입장 불가");
                return;
            }

            if (!_sessions.ContainsKey(session.SessionID))
            {
                _sessions.Add(session.SessionID, session);
                session.Room = this;
            }

            S_JoinRoom joinPacket = new S_JoinRoom
            {
                roomId = RoomId,
                sessionID = session.SessionID
            };
            session.Send(joinPacket.Write());

            Console.WriteLine($"클라이언트 {session.SessionID}가 방 {RoomId}에 입장");

        }

        public void BothReady()
        {
            S_Ready s_Ready = new S_Ready();

            BroadCast(s_Ready.Write());
        }

        public void ReadyStartGame()
        {
            _roomState = RoomState.InGame;
            Console.WriteLine($"게임 시작! Room ID: {RoomId}");

            GameLogic = new GameLogicManager(this);
            foreach (var session in _sessions.Values)
            {
                GameLogic.AddPlayer(session);
            }
            GameLogic.Init();

   /*         S_StartGame startPacket = new S_StartGame();
            startPacket.gameId = RoomId;

            BroadCast(startPacket.Write());*/
        }

        public void Leave(ClientSession session)
        {
            if (_sessions.ContainsKey(session.SessionID))
            {
                Console.WriteLine($"클라이언트 {session.SessionID}가 방 {RoomId}에서 퇴장");
                _sessions.Remove(session.SessionID);
                session.Room = null;
            }

            if (_sessions.Count == 0)
            {
                EndGame();
            }
        }

        public void EndGame()
        {
            Console.WriteLine($"방 {RoomId} 게임 종료");
            _roomState = RoomState.Finished;
            // 필요한 후처리 추가 가능
        }
    }
}
