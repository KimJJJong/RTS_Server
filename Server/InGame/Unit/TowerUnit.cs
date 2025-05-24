using System;

public class TowerUnit : Unit, ITickable
{
    private float TowerDecayPerSecond = 100f;
    private float _accumulatedDecay = 0f;
    private int _lastProcessedTick = -1;

    public TowerUnit(string uid)
    {
        switch (uid)
        {
            case "TWR-ATK-001":
                TowerDecayPerSecond = 10f;
                break;
            case "TWR-DEF-001":
                TowerDecayPerSecond = 14f;
                break;

        }          
    }
    public override void Summon(float x, float y, float playerId, int spawnTick)
    {
        base.Summon(x, y, playerId, spawnTick);
        _lastProcessedTick = -1;
        _accumulatedDecay = 0f;
    }
    public override void TickUpdate(int tick)
    {
        if (!IsActive) return;

        if (_lastProcessedTick == -1)
            _lastProcessedTick = SpawnTick;

        int elapsedTick = tick - _lastProcessedTick;
        if (elapsedTick <= 0) return;

        // 1초 = 1000ms / 서버 TickRate (예: 30)일 경우 1초당 30tick
        float decay = TowerDecayPerSecond * (elapsedTick / 30f);
        _accumulatedDecay += decay;
        _lastProcessedTick = tick;

        int decayAmount = (int)_accumulatedDecay;
        if (decayAmount > 0)
        {
            CurrentHP -= decayAmount;
            _accumulatedDecay -= decayAmount;

            if (CurrentHP <= 0)
            {
                CurrentHP = 0;
                Deactivate(tick);
            }
        }
    }
    public override UnitType UnitTypeIs()
    {
        return UnitType.Tower;
    }
}