public class RoomMapping
{
    public static RoomMapping Instance { get; } = new RoomMapping();



    private readonly Dictionary<string, RoomInfo> _playerToRoom = new();
    private readonly object _lock = new();

    public void AddRoomInfo(S_M_CreateRoom packet)
    {
        lock (_lock)
        {
            _playerToRoom[packet.player1] = new RoomInfo { RoomId = packet.roomId, Player1 = packet.player1, Player2 = packet.player2 };
            _playerToRoom[packet.player2] = new RoomInfo { RoomId = packet.roomId };

        }
    }

    public RoomInfo GetRoomInfo(string playerId)
    {
        lock (_lock)
        {
            return _playerToRoom.TryGetValue(playerId, out var room) ? room : null;
        }
    }

    public void Remove(string roomId)
    {
        lock (_lock)
        {
            var toRemove = _playerToRoom
                .Where(x => x.Value.RoomId == roomId)
                .Select(x => x.Key)
                .ToList();

            foreach (var userId in toRemove)
                _playerToRoom.Remove(userId);
        }
    }
}

public class RoomInfo
{
    public string RoomId { get; set; }
    public string Player1 { get; set; }
    public string Player2 { get; set; }
}
