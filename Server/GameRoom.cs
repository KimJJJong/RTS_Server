using System;
using System.Collections.Generic;

namespace Server
{
    enum RoomState
    {
        Waiting = 0,   // 대기 중 (플레이어 모집)
        InGame = 1,    // 게임 진행 중
        Finished = 2   // 게임 종료됨
    }

    // TODO : Room For Test ( Need to reMake )
    class GameRoom : IJobQueue
    {
        public string RoomId { get; }
        List<ClientSession> _sessions = new List<ClientSession>();
        JobQueue _jobQueue = new JobQueue();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
        GameLogicManager _gameLogic;
        RoomState _roomState;

        public GameRoom()
        {
            RoomId = Guid.NewGuid().ToString();
            _roomState = RoomState.Waiting;
        }
        public void Push(Action action)
        {
            _jobQueue.Push(action);
        }
        
        public void Flush()
        {
            foreach (ClientSession session in _sessions)
                session.Send(_pendingList);

            Console.WriteLine($"Flushed {_pendingList.Count} items");
            _pendingList.Clear();
        }

        public void BroadCast(ArraySegment<byte> segment)
        {
            _pendingList.Add(segment);
        }

        public void Enter(ClientSession session)
        {
            if (_roomState != RoomState.Waiting)
            {
                Console.WriteLine($"방 {RoomId}은 이미 게임 중이므로 입장 불가");
                return;
            }
            _sessions.Add(session);
            session.Room = this;
            Console.WriteLine($"클라이언트 {session.SessionID}가 방 {RoomId}에 입장");

            if (_sessions.Count == 2)  // 두 명이 입장하면 게임 시작
            {
                StartGame();
            }

        }
        private void StartGame()
        {
            Console.WriteLine($"게임 시작! Room ID: {RoomId}");
            
            _gameLogic = new GameLogicManager(this);
            foreach (ClientSession session in _sessions)
                _gameLogic.AddPlayer(session);

            //BroadcastToAll("게임이 시작되었습니다!");
            // 게임 로직 돌리기 : _gameLogic.Update();
        }

        public void Leave(ClientSession session)
        {

            _sessions.Remove(session);
            Console.WriteLine($"클라이언트 {session.SessionID}가 방 {RoomId}에서 퇴장");

            if (_sessions.Count == 0)
            {
                EndGame();
            }

        }
        public void EndGame()
        {
            //BroadcastToAll("게임 종료되었습니다!");
        }

    }
}
