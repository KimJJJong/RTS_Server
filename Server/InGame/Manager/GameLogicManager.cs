// GameLogicManager.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Server;


public class GameLogicManager
{
    private GameRoom _room;
    private bool _gameOver;
    private TickManager _tickManager;
    private PlayerManager _playerManager;
    private DeckManager _deckManager;
    private BattleManager _battleManager;
    private UnitPoolManager _unitPoolManager;
    private TimerManager _timerManager;
    private TickDrivenUnitManager _tickDrivenUnitManager;
    private OccupationManager _occupationManager;
    private TileManager _tileManager;
    private PositionCache _positionCache;
    private DimensionManager _dimensionManager;



    public GameLogicManager(GameRoom room)
    {
        _gameOver = false;
        _room = room;
        _tickManager = new TickManager();
        _playerManager = new PlayerManager();
        _deckManager = new DeckManager();
        _unitPoolManager = new UnitPoolManager();
        _positionCache = new PositionCache(_room.Players.ToArray());
        _tickDrivenUnitManager = new TickDrivenUnitManager();
        _occupationManager = new OccupationManager(this, _positionCache, _tickManager);
        _tileManager = new TileManager(_occupationManager, _positionCache);
        _battleManager = new BattleManager(_unitPoolManager, _room, _tickManager, _occupationManager);
        _timerManager = new TimerManager(_tickManager);
        _dimensionManager = new DimensionManager(_tickManager);

        Init();
    }

    public void Init()
    {
        _timerManager.Init();
        _dimensionManager.Init();
        _occupationManager.Init(_room.Players);
        _tileManager.Init(_room.Players);
        _battleManager.Init(_room.Players);
        _deckManager.Init(_room.ClientSessions);
        _playerManager.Init(_room.ClientSessions);
        _unitPoolManager.Init(_deckManager.CardPoolList);

        _room.BroadCast(MakeInitBundlePacket().Write());

        JobTimer.Instance.Push(Update);
    }




    public void OnReceiveSummon(ClientSession session, C_ReqSummon packet)
    {
        try
        {
            Console.WriteLine($"[GameLogicManager] Summon requested: OID={packet.oid}, Session={session.SessionID}");

            if (!_playerManager.Manas.TryGetValue(packet.reqSessionID, out Mana mana))
            {
                Console.WriteLine("[GameLogicManager] X 마나 정보 없음");
                return;
            }

            if (!mana.UseMana(packet.needMana))
            {
                Console.WriteLine("[GameLogicManager] X 마나 부족");
                return;
            }
            packet.needMana = mana.GetMana();

            Unit unit = _unitPoolManager.GetUnit(packet.oid);
            if (unit?.IsActive == true)     // 이거 그 oid 겹치는거 소환 요청 했을때 처리
            {
                int? available = _unitPoolManager.GetAvailableOid(packet.oid);
                if (available == null)
                {
                    Console.WriteLine($"[GameLogicManager] X No available unit in group for OID={packet.oid}");
                    return;
                }
                packet.oid = available.Value;
                unit = _unitPoolManager.GetUnit(packet.oid);
            }


            if (unit.UnitTypeIs() is UnitType.Tower && unit is ITickable)
            {
                RegisterTickUnit(unit);
                unit.OnDead += UnregisterTickUnit;
            }

            _battleManager.ProcessSummon(session, packet);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameLogicManager] (!) Error in OnReceiveSummon: {ex.Message}");
        }
    }
    public void OnReciveSummonProject(ClientSession session, C_SummonProJectile packet)
    {
        try
        {
            Console.WriteLine($"[GameLogicManager] Projectile summon: OID={packet.projectileOid}, Tick={packet.clientRequestTick}");

            Unit unit = _unitPoolManager.GetUnit(packet.projectileOid);
            if (unit?.IsActive == true)
            {
                int? available = _unitPoolManager.GetAvailableOid(packet.projectileOid);
                if (available == null)
                {
                    Console.WriteLine($"[GameLogicManager] X No available projectile in group for OID={packet.projectileOid}");
                    return;
                }
                packet.projectileOid = available.Value;
            }

            _battleManager.ProcessSummonProjectile(session, packet);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameLogicManager] (!) Error in OnReciveSummonProject: {ex.Message}");
        }
    }

    public void OnReciveAttack(ClientSession session, C_AttackedRequest packet)
    {
        try
        {
            Unit attackerUnit = _unitPoolManager.GetUnit(packet.attackerOid);
            Unit targetUnit = _unitPoolManager.GetUnit(packet.targetOid);


            if( packet.targetOid < 0 ) // Projectile이 시간 초과
            {
                attackerUnit.Dead(_tickManager.GetCurrentTick());
                Console.WriteLine(attackerUnit.UnitID);
            }
            else if (attackerUnit.UnitTypeIs() == UnitType.Projectile)
            {

                if (targetUnit.UnitTypeIs() == UnitType.WallMaria)
                {
                    _battleManager.ProcessWallMariaProjectileAttacked(session, packet);
                }
                else
                {
                    _battleManager.ProcessProjectileAttack(session, packet);
                }
            }
            else // 일단은 근접 공격
            {
                if (targetUnit.UnitTypeIs() == UnitType.WallMaria)
                {
                    _battleManager.ProcessWallMariaAttacked(session, packet);
                }
                else
                {
                    _battleManager.ProcessAttack(session, packet);
                }
            }


            Console.WriteLine($"[GameLogicManager] Attack: {packet.attackerOid} -> {packet.targetOid}, Tick={packet.clientAttackedTick}     [ {attackerUnit.UnitTypeIs().ToString()} ]");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameLogicManager] (!) Error in OnReceiveAttack: {ex.Message}");
        }
    }

    public void OnReceiveTileClaim(ClientSession session, C_TileClaimReq packet)
    {
        Unit unit = _unitPoolManager.GetUnit(packet.unitOid);
        if (unit == null || !unit.IsActive || unit.PlayerID != session.SessionID)
        {
            Console.WriteLine($"[TileClaim] ❌ Invalid claim attempt: oid={packet.unitOid}");
            return;
        }

        if (!_tileManager.TryCaptureFromClient(session, packet))
        {
            Console.WriteLine("[TileClaim] (!) Error");
        }

    }


    public void Update()
    {
        if (_gameOver)
            return;

        _playerManager.RegenManaAll();
        _tickDrivenUnitManager.Update(_tickManager.GetCurrentTick());

        _dimensionManager.Update(this);
        
        Console.WriteLine($"CurrentTime {_timerManager.RemainingSeconds}");

        JobTimer.Instance.Push(Update, 1000);
    }
    /// <summary>
    /// Duration, StartTime, PoolSize, AllObjectList
    /// </summary>
    /// <returns></returns>
    public S_GameInitBundle MakeInitBundlePacket()
    {
        var cardPool = _deckManager.GetAllCards();
        return new S_GameInitBundle
        {
            gameStartTime = _tickManager.GetStartTimeMs(),
            duration = _timerManager.Duratino,
            size = 10,      // ObjectPool Size
            cardCombinationss = cardPool.Select(card => new S_GameInitBundle.CardCombinations
            {
                uid = card.ID,
                lv = card.LV
            }).ToList()
        };
    }


    public void RegisterTickUnit(Unit unit) => _tickDrivenUnitManager.Register(unit);
    public void UnregisterTickUnit(Unit unit) => _tickDrivenUnitManager.Unregister(unit);

    public void EndGame(int winnerSessionId)
    {
        _gameOver = true;
        _playerManager.Clear();
        _deckManager.Clear();
        _unitPoolManager.Clear();
        _occupationManager.Clear();
        _tickDrivenUnitManager.Clear();
        _tileManager.Clear();
        Console.WriteLine($"[GameLogicManager] Game ended. Winner: Session {winnerSessionId}");
    }
    public void SendToPlayer(int sessionId, ArraySegment<byte> segment) => _room.SendToPlayer(sessionId, segment);


}
