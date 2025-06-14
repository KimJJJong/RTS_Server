using System;

class TickManager
{
    private readonly int _tickIntervalMs = 33;
    private long _startTimeMs;

    public TickManager()
    {
        _startTimeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        Console.WriteLine($"[TickManager] Server Start Time: {_startTimeMs} ms");
    }

    public int GetCurrentTick()
    {
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        return (int)((nowMs - _startTimeMs) / _tickIntervalMs);
    }

    public float GetTickIntervalSec() => _tickIntervalMs / 1000f;

    public long GetStartTimeMs() => _startTimeMs;
    public long GetNowTimeMs() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}
