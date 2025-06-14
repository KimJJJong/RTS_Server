using System.Collections.Generic;
using System;
using System.Linq;

public class GameRoom : IGameRoom
{
    public string RoomId { get; }
    private Dictionary<int, PlayerSlot> _players = new();
    private RoomState _roomState;
    private GameLogicManager _logicManager;
    public GameLogicManager GameLogic => _logicManager;

    private List<Card> _deckCombination;

    

    private readonly object _lock = new();

    public GameRoom(string roomId, List<string> playerIds, List<Card> deckCombination)
    {
        RoomId = roomId;
        _deckCombination = deckCombination;

        for (int i = 0; i < playerIds.Count; i++)
        {
            _players[i] = new PlayerSlot( i, playerIds[i]);

        }

        _roomState = RoomState.Waiting;
    }

    public bool AddClient(string userId, ClientSession client)
    {
        if (_roomState == RoomState.Finished)
            return false;

        var slot = _players.Values.FirstOrDefault(p => p.UserId == userId);
        if (slot == null || slot.Session != null)
            return false;

        slot.Session = client;
        client.Room = this;
        client.PlayingID = slot.InternalId;

        Console.WriteLine($"[GameRoom] Client {userId} joined Room {RoomId}");


        bool allPlayersJoined;
        lock (_lock)
        {
            allPlayersJoined = _players.Values.All(p => p.Session != null);
        }

        if (allPlayersJoined)
            StartGame();

        return true;
    }

    public void RemoveClient(int internalId)
    {
        if (_players.TryGetValue(internalId, out var slot) && slot.Session != null)
        {
            slot.Session = null;
            Console.WriteLine($"[GameRoom] Client {internalId} left Room {RoomId}");

            if (_players.Values.All(p => p.Session == null))
                EndGame();
        }
    }

    public void BroadCast(ArraySegment<byte> segment)
    {
        foreach (var session in _players.Values.Select(p => p.Session).Where(s => s != null))
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
/*        if (_sessions.TryGetValue(internalId, out var client))
        {
        }*/
            _players[internalId].Session.Send(segment);
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
        _players.Clear();
    }

    public string GetExternalId(int internalId) => _players[internalId].UserId;

    public List<int> Players => _players.Keys.ToList();
    public List<Card> PlayeCardDeckCombination => _deckCombination;
    public IEnumerable<ClientSession> ClientSessions => _players.Values.Select(p => p.Session).Where(s => s != null);
    //public IReadOnlyDictionary<int, ClientSession> Sessions => _sessions;
    public int ConnectedCount => _players.Values.Count(p => p.Session != null);


}

enum RoomState
{
    Waiting,
    InGame,
    Finished
}


public class PlayerSlot
{
    public int InternalId { get; }
    public string UserId { get; }
    public ClientSession Session { get; set; }

    public PlayerSlot(int internalId, string userId)
    {
        InternalId = internalId;
        UserId = userId;
    }
}