public class DefaultUnit : Unit
{
    public override void TickUpdate(int tick) { /* 대부분 빈 구현 */ }
    public override UnitType UnitTypeIs()
    {
        return UnitType.Deaful;
    }
}
