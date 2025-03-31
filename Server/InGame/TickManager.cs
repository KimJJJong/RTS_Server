using System;

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
