using System.Collections.Generic;
using System;
using System.Linq;
using Shared;

public class GameRoom : IGameRoom
{
    public string RoomId { get; }
    private Dictionary<int, PlayerSlot> _players = new();
    private List<int> _playerSessionID = new();
    private RoomState _roomState;
    private GameLogicManager _logicManager;
    public GameLogicManager GameLogic => _logicManager;

    private List<Card> _deckCombination = new List<Card>();

    public RoomState RoomState => _roomState;

    

    private readonly object _lock = new();

    public GameRoom(string roomId)
    {
        RoomId = roomId;

    
        _roomState = RoomState.Waiting;
    }

    public bool AddClient(ClientSession client,List<Card> cardList)
    {
        if (_roomState != RoomState.Waiting)
            return false;
        
        if(_players.Keys.Count == 0)
        {
            _players[0] = new PlayerSlot(0, client.SessionID);
            _players[0].Session = client;
            client.Room = this;
            client.PlayingID = _players[0].InternalId;
        }
        else if(_players.Keys.Count == 1)
        {
            _players[1] = new PlayerSlot(1, client.SessionID);
            _players[1].Session = client;
            client.Room = this;
            client.PlayingID = _players[1].InternalId;
        }
        else
        {
            LogManager.Instance.LogWarning(("[GameRoom]"), $"_players.Keys.Count outof Index {_players.Keys.Count}");
            return false;
        }

        _deckCombination.AddRange(cardList) ;
        Console.WriteLine($"[GameRoom] Client {client.SessionID} joined Room {RoomId}");


        bool allPlayersJoined;
        lock (_lock)
        {
            if(_players.Values.Count >= 2)
            {
            allPlayersJoined = true;
            }
            else
            {
                allPlayersJoined = false;
            }
            Console.WriteLine($"allPlayerJoin : {_players.Values.Count} || {allPlayersJoined}");

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
            _players[internalId].Session.Send(segment);
    }

    public void StartGame()
    {
        if (_roomState == RoomState.InGame)
            return;

        _roomState = RoomState.InGame;
        LogManager.Instance.LogInfo("[GameRoom]", $"Starting Game in Room {RoomId}|| Players {_players[0].Session} and {_players[1].Session}");

        _logicManager = new GameLogicManager(this);
    }

    public void EndGame()
    {
        if (_roomState == RoomState.Finished)
            return;

        _roomState = RoomState.Finished;
        Console.WriteLine($"[GameRoom] Ending Game in Room {RoomId} :Need To Check Room State");
        _logicManager.EndGame(-1);
        _logicManager = null;
        _players.Clear();
    }

    public string GetExternalId(int internalId) => _players[internalId].UserId.ToString();

    public List<int> Players => _players.Keys.ToList();
    public List<Card> PlayeCardDeckCombination => _deckCombination;
    public IEnumerable<ClientSession> ClientSessions => _players.Values.Select(p => p.Session).Where(s => s != null);
    public int ConnectedCount => _players.Values.Count(p => p.Session != null);


}

public enum RoomState
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

    public PlayerSlot(int internalId, int userId )
    {
        InternalId = internalId;
        UserId = userId.ToString();
    }
}