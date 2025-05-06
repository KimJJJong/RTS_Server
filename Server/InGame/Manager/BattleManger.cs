// BattleManager.cs
using Server;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

class BattleManager
{
    private UnitPoolManager _unitPoolManager;
    private GameRoom _room;
    private TickManager _tickManager;
    private OccupationManager _occupationManager;

    private int HpDecreassTick = 15;  // 근접 공격 체력 decreass Tick Delay
    private int HpDecreassProjectileTick = 3; // 투사체 공격 체력 decreass Tick Delay
    private int SummonProjectileDelayTick = 5; // 투사체 생성 Tick Delay

    private int WallMariaHitTick = 3; // 월마리아 공격 Tick Delay 

    private int player0;
    public BattleManager(UnitPoolManager unitPoolManager, GameRoom room, TickManager tickManager, OccupationManager occupationManager)
    {
        _unitPoolManager = unitPoolManager;
        _room = room;
        _tickManager = tickManager;
        _occupationManager = occupationManager;

        
    }
    public void Init(List<int> sessionListId)
    {
        player0 = sessionListId[0];
    }
    #region Summon

    public void ProcessSummon(ClientSession session, C_ReqSummon packet)
    {
        int delayTick = 30;
        int currentTick = _tickManager.GetCurrentTick();
        int executeTick = currentTick + delayTick;

        Random rng = new Random(currentTick * 1000 + packet.reqSessionID);

        // Server Data Save
        var (serverX, serverY) = PositionConverter.ClientToServer(
            packet.reqSessionID, packet.x, packet.y, player0);
        Unit unit = _unitPoolManager.GetUnit(packet.oid);
        unit.Summon(serverX, serverY, session.SessionID);

        // Send To Client With Convert
        foreach (int target in _occupationManager.GetPlayerSessionIds())
        {
            var (clientX, clientY) = PositionConverter.ServerToClient(
                target, serverX, serverY, player0);

            S_AnsSummon response = new S_AnsSummon
            {
                oid = packet.oid,
                reqSessionID = session.SessionID,
                x = clientX,
                y = clientY,
                randomValue = rng.Next(0, 10),
                reducedMana = packet.needMana, // TODO: 실제 마나 시스템 연결
                ExcuteTick = executeTick,
                ServerReceiveTimeMs = _tickManager.GetNowTimeMs(),
                ServerStartTimeMs = _tickManager.GetStartTimeMs(),
                ClientSendTimeMs = packet.ClientSendTimeMs
            };


            _room.SendToPlayer(target, response.Write());
        }
    }
    public void ProcessSummonProjectile(ClientSession session, C_SummonProJectile packet)
    {
        int shootTick = packet.clientRequestTick + SummonProjectileDelayTick;

        // Server Data Save
        var (serverX, serverY) = PositionConverter.ClientToServer(
            session.SessionID, packet.summonerX, packet.summonerY, player0);
        Unit proj = _unitPoolManager.GetUnit(packet.projectileOid);
        proj.Summon(serverX, serverY, session.SessionID);

        var (serverTargetX, serverTargetY) = PositionConverter.ClientToServer(
            session.SessionID, packet.targetX, packet.targetY, player0);

        // Send To Client With Convert
        foreach (int target in _occupationManager.GetPlayerSessionIds())
        {
            var (clientX, clientY) = PositionConverter.ServerToClient(
                target, serverX, serverY, player0);

            var (clientTargetX, clientTargetY) = PositionConverter.ServerToClient(
                target, serverTargetX, serverTargetY, player0);

            S_ShootConfirm response = new S_ShootConfirm
            {
                projcetileOid = packet.projectileOid,
                summonerOid = packet.summonerOid,
                targetOid = packet.targetOid,
                projectileSpeed = _unitPoolManager.GetUnit(packet.projectileOid).Speed, // 예시
                startX = clientX,
                startY = clientY,
                targetX = clientTargetX,
                targetY = clientTargetY,
                shootTick = shootTick
            };
            _room.SendToPlayer(target, response.Write());
        }
    }
    #endregion
    #region Attack - Melee
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
            target.Dead(_tickManager.GetCurrentTick());
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
            attackVerifyTick = packet.clientAttackedTick + HpDecreassTick  // hp decreass Rate
        };

        _room.BroadCast(response.Write());
    }
    public void ProcessWallMariaAttacked(ClientSession session, C_AttackedRequest packet)
    {
        Unit attacker = _unitPoolManager.GetUnit(packet.attackerOid);
        Unit target = _unitPoolManager.GetUnit(packet.targetOid);

        if (attacker == null || target == null || target.UnitTypeIs() != UnitType.WallMaria)
            return;

        _occupationManager.OnWallHit(session.SessionID);

 /*       S_AttackConfirm response = new S_AttackConfirm
        {
            attackerOid = packet.attackerOid,
            targetOid = packet.targetOid,
            //targetVerifyHp = Math.Max(0, curHp),
            attackVerifyTick = packet.clientAttackedTick + HpDecreassTick // hp decreass Rate
        };

        _room.BroadCast(response.Write());*/
    }

    #endregion
    #region Attack - Projectile
    public void ProcessProjectileAttack(ClientSession session, C_AttackedRequest packet)
    {
        Unit projectile = _unitPoolManager.GetUnit(packet.attackerOid);
        Unit target = _unitPoolManager.GetUnit(packet.targetOid);

        if (projectile == null || target == null) return;


        float curHp = target.CurrentHP - projectile.AttackPower;
        target.CurrentHP = curHp;
        Console.WriteLine($"targetHP : {target.CurrentHP} || Damage : { projectile.AttackPower } || Resaurt : {curHp}");
        S_AttackConfirm response = new S_AttackConfirm
        {
            attackerOid = packet.attackerOid,
            targetOid = packet.targetOid,
            targetVerifyHp = Math.Max(0, curHp),
            attackVerifyTick = packet.clientAttackedTick + HpDecreassProjectileTick // hp decreass Rate
        };

        projectile.Dead(_tickManager.GetCurrentTick());

        _room.BroadCast(response.Write());
    }
 

    public void ProcessWallMariaProjectileAttacked(ClientSession session, C_AttackedRequest packet)
    {
        Unit projectile = _unitPoolManager.GetUnit(packet.attackerOid);
        Unit target = _unitPoolManager.GetUnit(packet.targetOid);

        if (projectile == null || target == null || target.UnitTypeIs() != UnitType.WallMaria)
            return;

        _occupationManager.OnWallHit(session.SessionID);

/*        S_AttackConfirm response = new S_AttackConfirm
        {
            attackerOid = packet.attackerOid,
            targetOid = packet.targetOid,
            //targetVerifyHp = Math.Max(0, curHp),
            attackVerifyTick = packet.clientAttackedTick + HpDecreassProjectileTick // hp decreass Rate
        };

        _room.BroadCast(response.Write());*/

        projectile.Dead(_tickManager.GetCurrentTick());
    }
    #endregion 


} 