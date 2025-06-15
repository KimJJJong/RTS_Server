using System.Dynamic;

public class MatchQueue
{
    private Queue<MatchPlayerInfo> _queue = new();
    private HashSet<string> _enqueuedUsers = new();
    private readonly object _lock = new();

    public bool Enqueue(MatchPlayerInfo playerInfo)
    {
        lock (_lock)
        {
            if (_enqueuedUsers.Contains(playerInfo.UserId))
            {
                // 이미 대기중
                return false;
            }

            _queue.Enqueue(playerInfo);
            _enqueuedUsers.Add(playerInfo.UserId);
            return true;
        }
    }

    public bool TryDequeue(out MatchPlayerInfo playerInfo)
    {
        lock (_lock)
        {
            if (_queue.Count > 0)
            {
                playerInfo = _queue.Dequeue();
                _enqueuedUsers.Remove(playerInfo.UserId);
                return true;
            }

            playerInfo = null;
            return false;
        }
    }

    public bool Cancel(string userId)
    {
        lock (_lock)
        {
            if (!_enqueuedUsers.Contains(userId))
                return false;

            Queue<MatchPlayerInfo> newQueue = new();

            while (_queue.Count > 0)
            {
                var info = _queue.Dequeue();
                if (info.UserId != userId)
                    newQueue.Enqueue(info);
            }

            _queue = newQueue;
            _enqueuedUsers.Remove(userId);
            return true;
        }
    }

    public int Count => _queue.Count;
}

public class MatchPlayerInfo
{
    public string UserId { get; set; }
    public List<CardInfo> Deck { get; set; }
}
public class MatchPlayerCancel
{
    public string UserId { get; set; }
}
public class CardInfo
{
    public int Lv { get; set; }
    public string Uid { get; set; }
}