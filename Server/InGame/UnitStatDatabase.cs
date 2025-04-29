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
    public bool IsProjectile;
}

public static class UnitStatDatabase
{
    private static Dictionary<(string, int), UnitStat> _stats = new Dictionary<(string, int), UnitStat>();

    public static void Load()
    {
        // Joseon
        _stats[("U-JOS-001", 1)] = new UnitStat { UnitID = "U-JOS-001", Level = 1, MaxHP = 300, Speed = 0.5f, AttackPower = 20, AttackRange = 0.5f, IsProjectile = false };
        _stats[("U-JOS-002", 1)] = new UnitStat { UnitID = "U-JOS-002", Level = 1, MaxHP = 80, Speed = 0.8f, AttackPower = 60, AttackRange = 0.3f, IsProjectile = false };
        _stats[("U-JOS-003", 1)] = new UnitStat { UnitID = "U-JOS-003", Level = 1, MaxHP = 150, Speed = 0.6f, AttackPower = 0, AttackRange = 2f, IsProjectile = false };
        _stats[("U-JOS-004", 1)] = new UnitStat { UnitID = "U-JOS-004", Level = 1, MaxHP = 200, Speed = 0.5f, AttackPower = 0, AttackRange = 1.4f, IsProjectile = false };

        // England
        _stats[("U-ENG-001", 1)] = new UnitStat { UnitID = "U-ENG-001", Level = 1, MaxHP = 250, Speed = 0.7f, AttackPower = 35, AttackRange = 0.8f, IsProjectile = false};
        _stats[("U-ENG-002", 1)] = new UnitStat { UnitID = "U-ENG-002", Level = 1, MaxHP = 220, Speed = 0.4f, AttackPower = 45, AttackRange = 2.8f, IsProjectile = false };
        _stats[("U-ENG-003", 1)] = new UnitStat { UnitID = "U-ENG-003", Level = 1, MaxHP = 280, Speed = 0.6f, AttackPower = 50, AttackRange = 1.2f, IsProjectile = false };
        _stats[("U-ENG-004", 1)] = new UnitStat { UnitID = "U-ENG-004", Level = 1, MaxHP = 550, Speed = 0.5f, AttackPower = 30, AttackRange = 1f, IsProjectile = false };

        // Spell
        _stats[("SP-PNT-001", 1)] = new UnitStat { UnitID = "SP-PNT-001", Level = 1, MaxHP = -1, Speed = -1, AttackPower = -1, AttackRange = -1, IsProjectile = false };
        _stats[("SP-PNT-002", 1)] = new UnitStat { UnitID = "SP-PNT-002", Level = 1, MaxHP = -1, Speed = -1, AttackPower = -1, AttackRange = -1, IsProjectile = false };
        _stats[("SP-SUP-001", 1)] = new UnitStat { UnitID = "SP-SUP-001", Level = 1, MaxHP = 750, Speed = 0, AttackPower = -1, AttackRange = -1, IsProjectile = false };

        // Tower
        _stats[("TWR-ATK-001", 1)] = new UnitStat { UnitID = "TWR-ATK-001", Level = 1, MaxHP = 750, Speed = 0, AttackPower = -1, AttackRange = -1, IsProjectile = false };
        _stats[("TWR-DEF-001", 1)] = new UnitStat { UnitID = "SP-PNT-002", Level = 1, MaxHP = 750, Speed = 0, AttackPower = -1, AttackRange = -1, IsProjectile = false };

        //Projection
        _stats[("PRJ-TWR-ATK-001", 1)] = new UnitStat { UnitID = "PRJ-TWR-ATK-001", Level = 1, MaxHP = 750, Speed = 3f, AttackPower = -1, AttackRange = -1, IsProjectile = true };
        _stats[("PRJ-U-JOS-003", 1)] = new UnitStat { UnitID = "PRJ-U-JOS-003", Level = 1, MaxHP = 750, Speed = 1.8f, AttackPower = 30, AttackRange = -1 ,IsProjectile = true };
        _stats[("PRJ-U-JOS-004", 1)] = new UnitStat { UnitID = "PRJ-U-JOS-004", Level = 1, MaxHP = 750, Speed = 1.5f, AttackPower = 25, AttackRange = -1 , IsProjectile = true };
        _stats[("PRJ-U-ENG-003", 1)] = new UnitStat { UnitID = "PRJ-U-ENG-003", Level = 1, MaxHP = 750, Speed = 0, AttackPower = -1, AttackRange = -1 , IsProjectile = true };


        // Castle
        _stats[("CASTLE-U-01", 1)] = new UnitStat { UnitID = "CASTLE-U-01", Level = 1, MaxHP = 9999999, Speed = 0, AttackPower = -1, AttackRange = -1, IsProjectile = false };
        _stats[("CASTLE-U-02", 1)] = new UnitStat { UnitID = "CASTLE-U-02", Level = 1, MaxHP = 9999999, Speed = 0, AttackPower = -1, AttackRange = -1, IsProjectile = false };

        //etc... : 데미지 관련 X -> 이후 제외 해도 게임 진행에 문제가 없어야 함
        //_stats[("POS-SP-SUP-001", 1)] = new UnitStat { UnitID = "POS-SP-SUP-001", Level = 1, MaxHP = 750, Speed = 0, AttackPower = -1, AttackRange = -1 };
        //_stats[("TMR-SP-001", 1)] = new UnitStat { UnitID = "TMR_SP_001", Level = 1, MaxHP = 750, Speed = 0, AttackPower = -1, AttackRange = -1 };


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

        Unit unit;
        if (CardMetaDatabase.GetMeta(unitID, level)?.IsRanged == true && unitID.StartsWith("TWR"))
            unit = new TowerUnit(unitID, level); // 예: 타워 유닛일 경우
        else
            unit = new Unit(unitID, level);

        unit.SetStats(stat.MaxHP, stat.Speed, stat.AttackPower, stat.AttackRange, stat.IsProjectile);
        return unit;
    }
}
