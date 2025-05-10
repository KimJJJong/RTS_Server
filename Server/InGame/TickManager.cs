/*using System;

class TickManager
{
    private readonly int _tickIntervalMs = 33; // 30 Tick/sec
    
    private long _startTimeMs;

    public TickManager()
    {
        _startTimeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        Console.WriteLine($"ServerStartTime[{_startTimeMs}]");
    }

    /// <summary>
    /// 서버의 현재 Tick (시간 기준 계산)
    /// </summary>
    public int GetCurrentTick()
    {
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long elapsedMs = nowMs - _startTimeMs;
        return (int)(elapsedMs / _tickIntervalMs);
    }

    /// <summary>
    /// 서버 시작 시간 (UTC 기준 밀리초)
    /// </summary>
    public long GetStartTimeMs()
    {
        return _startTimeMs;
    }

    /// <summary>
    /// 현재 시간 (UTC 밀리초)
    /// </summary>
    public long GetNowTimeMs()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}
*/
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
