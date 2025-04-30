using System;

public class TowerUnit : Unit, ITickable
{
    private const float TowerDecayPerSecond = 25f;

    public override void TickUpdate(int tick)
    {
        if (!IsActive) return;

        CurrentHP -= TowerDecayPerSecond;
        if (CurrentHP <= 0)
        {
            Console.WriteLine("DEadTickUpdate <= 0");
            CurrentHP = 0;
            //SetDeadTick(tick);
            Dead(tick);
        }
    }
    public override UnitType UnitTypeIs()
    {
        return UnitType.Tower;
    }
}