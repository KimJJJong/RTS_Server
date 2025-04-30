using System;

public enum UnitType
{
    Deaful = 1,
    Tower = 2,
    Projectile = 3,
}

public abstract class Unit
{
    public string UnitID { get; protected set; }
    public int CardLV { get; protected set; }
    public float PlayerID { get; protected set; }

    public float MaxHP { get; protected set; }
    public float Speed { get; protected set; }
    public float AttackPower { get; protected set; }
    public float AttackRange { get; protected set; }

    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float CurrentHP { get; set; }

    public bool IsActive { get; protected set; }
    public int DeadTick { get; protected set; }

    public Action<Unit> OnDead; // Event callback for external use

    public void InitializeID(string unitID, int level)
    {
        UnitID = unitID;
        CardLV = level;
    }

    public virtual void SetStats(UnitStat stat)
    {
        MaxHP = stat.MaxHP;
        Speed = stat.Speed;
        AttackPower = stat.AttackPower;
        AttackRange = stat.AttackRange;
    }

    public virtual void Summon(float x, float y, float playerId)
    {
        PositionX = x;
        PositionY = y;
        PlayerID = playerId;
        CurrentHP = MaxHP;
        IsActive = true;
    }


    public virtual void Dead(int tick)
    {
        SetDeadTick(tick);
        IsActive = false;
        OnDead?.Invoke(this);
        Reset();
    }

    public virtual void Reset()
    {
        PositionX = -99;
        PositionY = -99;
        PlayerID = -1;
        CurrentHP = -1;
        DeadTick = 9999999;
    }

    public void SetDeadTick(int tick) => DeadTick = tick;

    public abstract void TickUpdate(int tick);
    public abstract UnitType UnitTypeIs();
}