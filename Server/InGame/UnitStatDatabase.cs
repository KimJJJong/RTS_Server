using System.Collections.Generic;
using System;



public class UnitStat
{
    public string UnitID;
    public int Level;
    public float MaxHP;
    public float Speed;
    public float AttackPower;
    public float AttackRange;
}

public static class UnitStatDatabase
{
    private static Dictionary<(string, int), UnitStat> _stats = new Dictionary<(string, int), UnitStat>();

    public static void Load()
    {
        // Joseon
        _stats[("U-JOS-001", 1)] = new UnitStat { UnitID = "U-JOS-001", Level = 1, MaxHP = 300, Speed = 0.5f, AttackPower = 20, AttackRange = 0.5f };
        _stats[("U-JOS-002", 1)] = new UnitStat { UnitID = "U-JOS-002", Level = 1, MaxHP =  80, Speed = 0.8f, AttackPower = 60, AttackRange = 0.3f };
        _stats[("U-JOS-003", 1)] = new UnitStat { UnitID = "U-JOS-003", Level = 1, MaxHP = 150, Speed = 0.6f, AttackPower = 30, AttackRange =   2f };
        _stats[("U-JOS-004", 1)] = new UnitStat { UnitID = "U-JOS-004", Level = 1, MaxHP = 200, Speed = 0.5f, AttackPower = 25, AttackRange = 1.4f };

        // England
        _stats[("U-ENG-001", 1)] = new UnitStat { UnitID = "U-ENG-001", Level = 1, MaxHP = 250, Speed = 0.7f, AttackPower = 35, AttackRange = 0.8f };
        _stats[("U-ENG-002", 1)] = new UnitStat { UnitID = "U-ENG-002", Level = 1, MaxHP = 220, Speed = 0.4f, AttackPower = 45, AttackRange = 2.8f };
        _stats[("U-ENG-003", 1)] = new UnitStat { UnitID = "U-ENG-003", Level = 1, MaxHP = 280, Speed = 0.6f, AttackPower = 50, AttackRange = 1.2f };
        _stats[("U-ENG-004", 1)] = new UnitStat { UnitID = "U-ENG-004", Level = 1, MaxHP = 550, Speed = 0.5f, AttackPower = 30, AttackRange =   1f };

        // Spell
        _stats[("SP-PNT-001", 1)] = new UnitStat { UnitID = "SP-PNT-001", Level = 1, MaxHP = -1, Speed = -1, AttackPower = -1, AttackRange = -1 };
        _stats[("SP-PNT-002", 1)] = new UnitStat { UnitID = "SP-PNT-002", Level = 1, MaxHP = -1, Speed = -1, AttackPower = -1, AttackRange = -1 };

        // Tower
        _stats[("TWR-ATK-001", 1)] = new UnitStat { UnitID = "TWR-ATK-001", Level = 1, MaxHP = 750, Speed = 0, AttackPower = -1, AttackRange = -1 };
        _stats[("TWR-DEF-001", 1)] = new UnitStat { UnitID =  "SP-PNT-002", Level = 1, MaxHP = 750, Speed = 0, AttackPower = -1, AttackRange = -1 };


    }

    public static UnitStat GetStat(string unitID, int level)
    {
        if (_stats.TryGetValue((unitID, level), out var stat))
            return stat;
        throw new Exception($"스탯 정보 없음: {unitID}, LV {level}");
    }
}



public static class UnitFactory
{
    public static Unit CreateUnit(string unitID, int level)
    {
        UnitStat stat = UnitStatDatabase.GetStat(unitID, level);
        Unit unit = new Unit(unitID, level);

        unit.SetStats(stat.MaxHP, stat.Speed, stat.AttackPower, stat.AttackRange);
        return unit;
    }
}
