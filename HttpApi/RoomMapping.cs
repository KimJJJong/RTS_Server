public class RoomMapping
{
    private readonly Dictionary<string, RoomInfo> _roomMap = new();
    private readonly object _lock = new();

    public void Add(string playerId, RoomInfo room)
    {
        lock (_lock)
        {
            _roomMap[playerId] = room;
        }
    }

    public RoomInfo GetRoom(string playerId)
    {
        lock (_lock)
        {
            return _roomMap.TryGetValue(playerId, out var room) ? room : null;
        }
    }
}

public class RoomInfo
{
    public string RoomId { get; set; }
    public string GameServerIp { get; set; }
    public int GameServerPort { get; set; }
}
