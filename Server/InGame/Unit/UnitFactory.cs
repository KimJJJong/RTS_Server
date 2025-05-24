public static class UnitFactory
{
    public static Unit CreateUnit(string unitID, int level, int oid)
    {
        UnitStat stat = UnitStatDatabase.GetStat(unitID, level);
        CardMeta meta = CardMetaDatabase.GetMeta(unitID, level);

        Unit unit;
        if (stat.IsProjectile)
            unit = new ProjectileUnit();
        else if (/*meta?.IsRanged == true && */unitID.StartsWith("TWR"))
            unit = new TowerUnit(unitID);
        else if(unitID.StartsWith("CASTLE"))
            unit = new WallMariaUnit();
        else
            unit = new DefaultUnit();

        unit.InitializeID(unitID, level, oid);
        unit.SetStats(stat);


        return unit;
    }
}