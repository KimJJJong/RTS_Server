using System;

public class Unit
{

    // Card Information
    public string UnitID { get; private set; }
    public float CardLV { get; private set; }
    public float PlayerID { get; private set; }
    // Needfloat Init state
    public float MaxHP { get; private set; }
    public float Speed { get; private set; }
    public float AttackPower { get; private set; }
    public float AttackRange { get; private set; }
    public bool IsProjectile { get; private set; }

    // InGame Modifi Value
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float CurrentHP { get; set; }
    public bool IsActive { get; private set; }
    public int DeadTick { get; private set; }
    public int LastAttackExcuteTick { get; private set; }
    public Unit(string unitID, int cardLV)
    {
        IsActive = false;

        PositionX = -99;
        PositionY = -99;

        UnitID = unitID;
        CardLV = cardLV;

    }

    public void Summon(S_AnsSummon s_AnsSummon)
    {
        IsActive = true;
        PlayerID = s_AnsSummon.reqSessionID;
        PositionX = s_AnsSummon.x;
        PositionY = s_AnsSummon.y;
        CurrentHP = MaxHP;

        Console.WriteLine($"CurrHp : {CurrentHP } || Power : {AttackPower}");
    }
    public void Summon(S_ShootConfirm s_ShootConfirm)
    {
        IsActive = true;
        //PlayerID = s_ShootConfirm.reqSessionID;
        PositionX = s_ShootConfirm.startX;
        PositionY = s_ShootConfirm.startY;
        //CurrentHP = MaxHP;

        Console.WriteLine($"CurrHp : {CurrentHP} || Power : {AttackPower}");
    }

    public void SetStats(float maxHp, float speed, float attackPower, float attackRange, bool isProjectile)
    {
        IsActive = false;
        MaxHP = maxHp;
        Speed = speed;
        AttackPower = attackPower;
        AttackRange = attackRange;
        IsProjectile = isProjectile;
    }



    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        if (!IsActive)
            Reset();
    }

    public void Dead()
    {
        IsActive = false;
        Reset();
    }

    public void Reset()
    {
        PositionX = -99;
        PositionY = -99;
        CurrentHP = -1;
        DeadTick = 9999999;
        PlayerID = -1;
        LastAttackExcuteTick = -1;
    }

    public void SetDeadTick(int deadTick)
    {
        DeadTick = deadTick;
    }
    public void SetLastAttackExcuteTick(int lastAttackTick)
    {
        LastAttackExcuteTick = lastAttackTick;
    }

}
