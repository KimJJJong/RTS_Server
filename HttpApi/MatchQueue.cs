public class MatchQueue
{
    private Queue<string> _queue = new();
    private HashSet<string> _enqueuedUsers = new();
    private readonly object _lock = new();

    public bool Enqueue(string userId)
    {
        lock (_lock)
        {
            if (_enqueuedUsers.Contains(userId))
            {
                // 이미 대기중
                return false;
            }

            _queue.Enqueue(userId);
            _enqueuedUsers.Add(userId);
            return true;
        }
    }

    public bool TryDequeue(out string userId)
    {
        lock (_lock)
        {
            if (_queue.Count > 0)
            {
                userId = _queue.Dequeue();
                _enqueuedUsers.Remove(userId);
                return true;
            }

            userId = null;
            return false;
        }
    }

    public bool Cancel(string userId)
    {
        lock (_lock)
        {
            if (!_enqueuedUsers.Contains(userId))
                return false;

            _queue = new Queue<string>(_queue.Where(x => x != userId));
            _enqueuedUsers.Remove(userId);
            return true;
        }
    }

    public int Count => _queue.Count;
}
