using System;
using System.Collections.Generic;

class TimerManager
{
    private TickManager _tickManager;
    private int _startTick;
    private const int DurationTick = 9000; // 예: 300초 * 30Tick = 9000Tick
    public int Duratino => DurationTick;
    
    public TimerManager(TickManager tickManager)
    {
        _tickManager = tickManager;
    }

    public void Init()
    {
        _startTick = _tickManager.GetCurrentTick();
        Console.WriteLine($"[GameTimerManager] Game Start Tick: {_startTick}, Duration Tick: {DurationTick}");
    }

    public int ElapsedTick => _tickManager.GetCurrentTick() - _startTick;
    public int RemainingTick => Math.Max(0, DurationTick - ElapsedTick);

    public float ElapsedSeconds => ElapsedTick * _tickManager.GetTickIntervalSec();
    public float RemainingSeconds => RemainingTick * _tickManager.GetTickIntervalSec();
    
    public bool IsTimeUp() => ElapsedTick >= DurationTick;

    public S_InitGame MakeInitPacket()
    {
        return new S_InitGame
        {
            gameStartTime = _tickManager.GetStartTimeMs(),
            duration = DurationTick
        };
    }
}
