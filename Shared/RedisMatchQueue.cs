using StackExchange.Redis;
using System.Collections.Generic;

namespace Shared
{
    public static class RedisMatchQueue
    {
        private static readonly ConnectionMultiplexer _redis = ConnectionMultiplexer.Connect("localhost");
        private static readonly IDatabase _db = _redis.GetDatabase();
        private const string QueueKey = "match_queue";

        public static void Enqueue(List<int> players)
        {
            string json = System.Text.Json.JsonSerializer.Serialize(players);
            _db.ListLeftPush(QueueKey, json);
        }

        public static List<int> Dequeue()
        {
            RedisValue value = _db.ListRightPop(QueueKey);
            if (value.IsNullOrEmpty) return null;

            return System.Text.Json.JsonSerializer.Deserialize<List<int>>(value);
        }

        public static long Count => _db.ListLength(QueueKey);
    }
}
