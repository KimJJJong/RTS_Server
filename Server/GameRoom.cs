using System;
using System.Collections.Generic;
using Shared;

namespace Server
{
    enum RoomState
    {
        Waiting,   // 대기 중 (플레이어 모집)
        InGame,    // 게임 진행 중
        Finished   // 게임 종료됨
    }

    class GameRoom 
    {
        public string RoomId { get; }
        private Dictionary<int, ClientSession> _sessions = new Dictionary<int, ClientSession>();
        private RoomState _roomState;
        public GameLogicManager GameLogic { get;  set; }
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


        public void BroadCast(ArraySegment<byte> segment)
        {
            foreach (var session in _sessions.Values)
            {
                try
                {
                    session.Send(segment);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"BroadCast 오류: {ex.Message}");
                }
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
            //Console.WriteLine($"클라이언트 {session.SessionID}가 방 {RoomId}에 입장");
            LogManager.Instance.LogInfo("GameRoom", $"[{RoomId}] Player {session.SessionID} entered room");

        }

        public void BothReady()
        {
            S_Ready s_Ready = new S_Ready();

            BroadCast(s_Ready.Write());
        }

        public void ReadyStartGame()
        {
            _roomState = RoomState.InGame;
            //Console.WriteLine($"게임 시작! Room ID: {RoomId}");
            LogManager.Instance.LogInfo("GameRoom", $"[{RoomId}] Game started");

            GameLogic = new GameLogicManager(this);
            foreach (var session in _sessions.Values)
            {
                GameLogic.AddPlayer(session);
            }
            GameLogic.Init();

            S_StartGame startPacket = new S_StartGame();
            startPacket.gameId = RoomId;

            BroadCast(startPacket.Write());
        }

        public void Leave(ClientSession session)
        {
            if (!_sessions.ContainsKey(session.SessionID))
                return;

                     //Console.WriteLine($"클라이언트 {session.SessionID}가 방 {RoomId}에서 퇴장");
            LogManager.Instance.LogInfo("GameRoom", $"클라이언트 {session.SessionID}가 방 {RoomId}에서 퇴장");
            _sessions.Remove(session.SessionID);
            session.isLoad = false;
            session.isReady = false;
            session.Room = null;


            if (_sessions.Count == 0)
            {
                EndGame();
            }
        }

        private void EndGame()
        {
            //_sessions.Clear();
            LogManager.Instance.LogInfo("GameRoom", $"[{RoomId}] Distroy");
            _sessions = null;
           // Console.WriteLine($"방 {RoomId} 게임 종료");
            GameLogic?.EndGame();
            GameLogic = null;
            _roomState = RoomState.Finished;
            // 필요한 후처리 추가 가능
        }
    }
}
