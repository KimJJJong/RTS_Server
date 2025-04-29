public class TowerUnit : Unit, ITickable
{
    private const float TowerDecayPerSecond = 25f;

    public override void TickUpdate(int tick)
    {
        if (!IsActive) return;

        CurrentHP -= TowerDecayPerSecond;
        if (CurrentHP <= 0)
        {
            CurrentHP = 0;
            SetDeadTick(tick);
            Dead();
        }
    }
}