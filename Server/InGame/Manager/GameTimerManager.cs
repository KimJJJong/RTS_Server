using System;
using System.Collections.Generic;

class GameTimerManager
{
    private TickManager _tickManager;
    private long _startTick;
    private const int _durationTick = 9000; // 예: 300초 * 30Tick = 9000Tick

    
    public GameTimerManager(TickManager tickManager)
    {
        _tickManager = tickManager;
    }

    public void Init()
    {
        _startTick = _tickManager.GetStartTimeMs();
        Console.WriteLine($"[GameTimerManager] Game Start Tick: {_startTick}, Duration Tick: {_durationTick}");
    }

    private int ElapsedTick => _tickManager.GetCurrentTick() - _startTick;
    private int RemainingTick => Math.Max(0, _durationTick - ElapsedTick);

    public float ElapsedSeconds => ElapsedTick * _tickManager.GetTickIntervalSec();
    public float RemainingSeconds => RemainingTick * _tickManager.GetTickIntervalSec();
    
    public bool IsTimeUp() => ElapsedTick >= _durationTick;

    public S_InitGame MakeInitPacket()
    {
        return new S_InitGame
        {
            gameStartTime = _tickManager.GetStartTimeMs(),
            duration = _durationTick
        };
    }
}
