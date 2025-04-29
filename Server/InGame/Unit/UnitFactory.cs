public static class UnitFactory
{
    public static Unit CreateUnit(string unitID, int level)
    {
        UnitStat stat = UnitStatDatabase.GetStat(unitID, level);
        CardMeta meta = CardMetaDatabase.GetMeta(unitID, level);

        Unit unit;
        if (stat.IsProjectile)
            unit = new ProjectileUnit();
        else if (meta?.IsRanged == true && unitID.StartsWith("TWR"))
            unit = new TowerUnit();
        else
            unit = new DefaultUnit();

        unit.InitializeID(unitID, level);
        unit.SetStats(stat);


        return unit;
    }
}