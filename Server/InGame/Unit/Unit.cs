using System;
using System.Security.Cryptography;

public enum UnitType
{
    Deaful = 1,
    Tower = 2,
    Projectile = 3,
    WallMaria = 4,
}

public abstract class Unit
{
    public int OId { get; protected set; }
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
    public int SpawnTick {  get; protected set; }
    public int DeadTick { get; protected set; }

    public Action<Unit> OnDead; // Event callback for external use

    public void InitializeID(string unitID, int level, int oid)
    {
        UnitID = unitID;
        CardLV = level;
        OId = oid;
    }

    public virtual void SetStats(UnitStat stat)
    {
        MaxHP = stat.MaxHP;
        Speed = stat.Speed;
        AttackPower = stat.AttackPower;
        AttackRange = stat.AttackRange;

        Console.WriteLine($"OID : {OId} || UID : {UnitID} || MaxHP: {MaxHP} || AttackPower :{AttackPower}");
    }

    public virtual void Summon(float x, float y, float playerId, int spawnTick)
    {
        PositionX = x;
        PositionY = y;
        PlayerID = playerId;
        SpawnTick = spawnTick;
        CurrentHP = MaxHP;
        DeadTick = 999999;
        IsActive = true;
    }


    public virtual void Dead(int tick)
    {
        SetDeadTick(tick);
        OnDead?.Invoke(this);
        Reset();
    }

    public virtual void Reset()
    {
        SpawnTick = -1;
        PositionX = -99;
        PositionY = -99;
        PlayerID = -1;
        CurrentHP = -1;
        IsActive = false;
    }

    public void SetDeadTick(int tick) => DeadTick = tick;

    public abstract void TickUpdate(int tick);
    public abstract UnitType UnitTypeIs();
}