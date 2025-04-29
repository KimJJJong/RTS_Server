// BattleManager.cs
using Server;
using System;

class BattleManager
{
    private UnitPoolManager _unitPoolManager;
    private GameRoom _room;
    private TickManager _tickManager;

    public BattleManager(UnitPoolManager unitPoolManager, GameRoom room, TickManager tickManager)
    {
        _unitPoolManager = unitPoolManager;
        _room = room;
        _tickManager = tickManager;
    }

    public void ProcessSummon(ClientSession session, C_ReqSummon packet)
    {
        int delayTick = 30;
        int currentTick = _tickManager.GetCurrentTick();
        int executeTick = currentTick + delayTick;

        Random rng = new Random(currentTick * 1000 + packet.reqSessionID);

        S_AnsSummon response = new S_AnsSummon
        {
            oid = packet.oid,
            reqSessionID = session.SessionID,
            x = packet.x,
            y = packet.y,
            randomValue = rng.Next(0, 10),
            reducedMana = 0, // TODO: 실제 마나 시스템 연결
            ExcuteTick = executeTick,
            ServerReceiveTimeMs = _tickManager.GetNowTimeMs(),
            ServerStartTimeMs = _tickManager.GetStartTimeMs(),
            ClientSendTimeMs = packet.ClientSendTimeMs
        };

        _room.BroadCast(response.Write());

        Unit unit = _unitPoolManager.GetUnit(response.oid);
        unit.Summon(packet.x, packet.y, session.SessionID);
    }

    public void ProcessAttack(ClientSession session, C_AttackedRequest packet)
    {
        Unit attacker = _unitPoolManager.GetUnit(packet.attackerOid);
        Unit target = _unitPoolManager.GetUnit(packet.targetOid);

        if (attacker == null || target == null)
            return;

        float curHp = target.CurrentHP - attacker.AttackPower;
        bool isDead = curHp <= 0;

        if (isDead)
        {
            target.SetDeadTick(_tickManager.GetCurrentTick());
            target.Dead();
        }
        else
        {
            target.CurrentHP = curHp;
        }

        S_AttackConfirm response = new S_AttackConfirm
        {
            attackerOid = packet.attackerOid,
            targetOid = packet.targetOid,
            targetVerifyHp = Math.Max(0, curHp),
            attackVerifyTick = _tickManager.GetCurrentTick() + 10
        };

        _room.BroadCast(response.Write());
    }

    public void ProcessSummonProjectile(ClientSession session, C_SummonProJectile packet)
    {
        int shootTick = _tickManager.GetCurrentTick() + 5;
        S_ShootConfirm response = new S_ShootConfirm
        {
            projcetileOid = packet.projectileOid,
            summonerOid = packet.summonerOid,
            projectileSpeed = 1f, // 예시
            startX = packet.summonerX,
            startY = packet.summonerY,
            targetX = packet.targetX,
            targetY = packet.targetY,
            shootTick = shootTick
        };
        Unit proj = _unitPoolManager.GetUnit(packet.projectileOid);
        proj.Summon(packet.summonerX, packet.summonerY, session.SessionID);
        _room.BroadCast(response.Write());
    }
}
