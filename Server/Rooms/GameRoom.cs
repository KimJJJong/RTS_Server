using System;
using System.Collections.Generic;


public class GameRoom : IGameRoom
{
    public string RoomId { get; }
    private List<int> _playerIds;
    private Dictionary<int, ClientSession> _sessions = new Dictionary<int, ClientSession>();
    private RoomState _roomState;
    private GameLogicManager _logicManager;
    public GameLogicManager GameLogic => _logicManager;
    object _lock = new object();
    public GameRoom(string roomId, List<int> playerIds)
    {
        RoomId = roomId;
        _playerIds = playerIds;
        _roomState = RoomState.Waiting;
    }

    // 클라이언트 추가
    public bool AddClient(int clientId, ClientSession client)
    {
        if (_roomState == RoomState.Finished)
            return false;

        if (_playerIds.Contains(clientId) && !_sessions.ContainsKey(clientId))
        {
            _sessions[clientId] = client;
            client.Room = this;
            Console.WriteLine($"[GameRoom] Client {clientId} joined Room {RoomId}");

            lock (_lock)
            {
                if (_sessions.Count == _playerIds.Count)
                    StartGame();
            }

            return true;
        }
        return false;
    }

    // 클라이언트 제거
    public void RemoveClient(int clientId)
    {
        if (_sessions.ContainsKey(clientId))
        {
            _sessions.Remove(clientId);
            Console.WriteLine($"[GameRoom] Client {clientId} left Room {RoomId}");

            if (_sessions.Count == 0)
                EndGame();
        }
    }

    // 전체 클라이언트에게 패킷 전송
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
                Console.WriteLine($"[GameRoom] BroadCast Error: {ex.Message}");
            }
        }
    }

    // 특정 클라이언트에게 패킷 전송
    public void SendToPlayer(int sessionId, ArraySegment<byte> segment)
    {
        if (_sessions.ContainsKey(sessionId))
        {
            var client = _sessions[sessionId];
            client.Send(segment);
        }
    }

    // 게임 시작
    public void StartGame()
    {
        if (_roomState == RoomState.InGame)
            return;

        _roomState = RoomState.InGame;
        Console.WriteLine($"[GameRoom] Starting Game in Room {RoomId}");

        _logicManager = new GameLogicManager(this);
    }

    // 게임 종료
    public void EndGame()
    {
        if (_roomState == RoomState.Finished)
            return;

        _roomState = RoomState.Finished;
        Console.WriteLine($"[GameRoom] Ending Game in Room {RoomId}");
        _logicManager = null;
        _sessions.Clear();
    }

    public List<int> Players => _playerIds;
    public IEnumerable<ClientSession> ClientSessions => _sessions.Values;
}

// Room 상태 관리
enum RoomState
{
    Waiting,   // 대기 중 (플레이어 모집)
    InGame,    // 게임 진행 중
    Finished   // 게임 종료됨
}
