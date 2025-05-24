/*using System.Collections.Generic;

public static class MatchQueue
{
    private static readonly Queue<List<int>> _queue = new();

    public static void Enqueue(List<int> players)
    {
        lock (_queue)
        {
            _queue.Enqueue(players);
        }
    }

    public static List<int> Dequeue()
    {
        lock (_queue)
        {
            return _queue.Count > 0 ? _queue.Dequeue() : null;
        }
    }

    public static int Count
    {
        get { lock (_queue) { return _queue.Count; } }
    }
}
*/