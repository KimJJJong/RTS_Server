using System.Collections.Generic;
using System;
using System.Linq;

public class GameRoom : IGameRoom
{
    public string RoomId { get; }
    private List<string> _playerIds;
    private Dictionary<string, int> _inGamePlayerNumber = new();
    private Dictionary<int, ClientSession> _sessions = new();
    private RoomState _roomState;
    private GameLogicManager _logicManager;
    public GameLogicManager GameLogic => _logicManager;

    private readonly List<int> _internalPlayerIds;
    private readonly object _lock = new();

    public GameRoom(string roomId, List<string> playerIds)
    {
        RoomId = roomId;
        _playerIds = playerIds;
        _internalPlayerIds = new List<int>();

        for (int i = 0; i < playerIds.Count; i++)
        {
            _inGamePlayerNumber[playerIds[i]] = i;
            _internalPlayerIds.Add(i);
        }

        _roomState = RoomState.Waiting;
    }

    public bool AddClient(string playerId, ClientSession client)
    {
        if (_roomState == RoomState.Finished)
            return false;

        if (_playerIds.Contains(playerId) && !_sessions.ContainsKey(_inGamePlayerNumber[playerId]))
        {
            int internalId = _inGamePlayerNumber[playerId];
            _sessions[internalId] = client;
            client.Room = this;
            client.PlayingID = internalId;

            Console.WriteLine($"[GameRoom] Client {playerId} joined Room {RoomId}");

            bool allPlayersJoined = false;
            lock (_lock)
            {
                if (_sessions.Count == _playerIds.Count)
                    allPlayersJoined = true;
            }

            if (allPlayersJoined)
                StartGame();

            return true;
        }
        return false;
    }

    public void RemoveClient(int internalId)
    {
        if (_sessions.ContainsKey(internalId))
        {
            _sessions.Remove(internalId);
            Console.WriteLine($"[GameRoom] Client {internalId} left Room {RoomId}");

            if (_sessions.Count == 0)
                EndGame();
        }
    }

    public void BroadCast(ArraySegment<byte> segment)
    {
        foreach (var session in _sessions.Values)
        {
            try { session.Send(segment); }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameRoom] BroadCast Error: {ex.Message}");
            }
        }
    }

    public void SendToPlayer(int internalId, ArraySegment<byte> segment)
    {
        if (_sessions.TryGetValue(internalId, out var client))
        {
            client.Send(segment);
        }
    }

    public void StartGame()
    {
        if (_roomState == RoomState.InGame)
            return;

        _roomState = RoomState.InGame;
        Console.WriteLine($"[GameRoom] Starting Game in Room {RoomId}");

        _logicManager = new GameLogicManager(this);
    }

    public void EndGame()
    {
        if (_roomState == RoomState.Finished)
            return;

        _roomState = RoomState.Finished;
        Console.WriteLine($"[GameRoom] Ending Game in Room {RoomId}");

        _logicManager = null;
        _sessions.Clear();
    }

    public List<int> Players => _internalPlayerIds;
    public IEnumerable<ClientSession> ClientSessions => _sessions.Values;
}

enum RoomState
{
    Waiting,
    InGame,
    Finished
}
