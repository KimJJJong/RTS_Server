// GameLogicManager.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Server;
using Shared;


class GameLogicManager
{
    private GameRoom _room;
    private bool _gameOver;
    private TickManager _tickManager;
    private PlayerManager _playerManager;
    private DeckManager _deckManager;
    private BattleManager _battleManager;
    private UnitPoolManager _unitPoolManager;
    private GameTimerManager _gameTimerManager;
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
        _deckManager = new DeckManager();
        _unitPoolManager = new UnitPoolManager();
        _playerManager = new PlayerManager(_tickManager);
        _positionCache = new PositionCache(_room.Sessions.Values.Select(s =>s.SessionID).ToArray());
        _tickDrivenUnitManager = new TickDrivenUnitManager(_room, _tickManager);
        _occupationManager = new OccupationManager(this, _positionCache, _tickManager);
        _tileManager = new TileManager(_occupationManager, _positionCache);
        _battleManager = new BattleManager(_unitPoolManager, _room, _tickManager, _occupationManager);
        _gameTimerManager = new GameTimerManager(_tickManager);
        _dimensionManager = new DimensionManager(_tickManager);

        Init();
    }

    public void Init()
    {
        _gameTimerManager.Init();
        _occupationManager.Init(_room.Sessions.Keys.ToList());
        _tileManager.Init(_room.Sessions.Keys.ToList());
        _battleManager.Init(_room.Sessions.Keys.ToList());
        _dimensionManager.Init();
        _room.BroadCast(_gameTimerManager.MakeInitPacket().Write());
        JobTimer.Instance.Push(Update);
    }

    public void AddPlayer(ClientSession session)
    {
        _playerManager.AddPlayer(session);
        Console.WriteLine($"[GameLogicManager] Player {session.SessionID} added.");
    }

    public void OnReceiveDeck(ClientSession session, C_SetCardPool packet)
    {
        Console.WriteLine($"[GameLogicManager] Receive deck from session {session.SessionID}");
        bool ready = _deckManager.ReceiveDeck(session, packet);

        if (ready)
        {
            List<Card> allCards = _deckManager.GetAllCards();
            _unitPoolManager.Initialize(allCards);
            _room.BroadCast(_deckManager.MakeCardPoolPacket().Write());

            foreach (Card card in allCards)
            {
                Console.WriteLine($"UID : {card.ID} LV : {card.LV}");
            }

            Console.WriteLine("[GameLogicManager] Decks ready and unit pool initialized.");
        }
    }

    public void OnReceiveSummon(ClientSession session, C_ReqSummon packet)
    {
        if (_gameOver)
        {
            Console.WriteLine($"[GameLogicManager] Game has ended. Ignoring request from session {session.SessionID}");
            return;
        }


        try
        {
           // Console.WriteLine($"[GameLogicManager] Summon requested: OID={packet.oid}, Session={session.SessionID}");

            if (!Manas.TryGetValue(packet.reqSessionID, out Mana mana))
            {
                Console.WriteLine("[GameLogicManager] X 마나 정보 없음");
                return;
            }

            if (!mana.UseMana(packet.needMana))
            {
                Console.WriteLine("[GameLogicManager] X 마나 부족");
                S_AnsSummon s_AnsSummon = new S_AnsSummon()
                {
                    canSummon = false
                };
                _room.SendToPlayer(packet.reqSessionID, s_AnsSummon.Write());
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
                    S_AnsSummon s_AnsSummon = new S_AnsSummon()
                    {
                        canSummon = false
                    };
                    _room.SendToPlayer(packet.reqSessionID, s_AnsSummon.Write());

                    return;
                }
                packet.oid = available.Value;
                unit = _unitPoolManager.GetUnit(packet.oid);
            }

            unit.OnDead += HandleUnitDeActivate;

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
        if (_gameOver)
        {
            Console.WriteLine($"[GameLogicManager] Game has ended. Ignoring request from session {session.SessionID}");
            return;
        }


        try
        {
           // Console.WriteLine($"[GameLogicManager] Projectile summon: OID={packet.projectileOid}, Tick={packet.clientRequestTick}");

            Unit unit = _unitPoolManager.GetUnit(packet.projectileOid);
            if (unit?.IsActive == true)
            {
                int? available = _unitPoolManager.GetAvailableOid(packet.projectileOid);
                if (available == null)
                {
                //    Console.WriteLine($"[GameLogicManager] X No available projectile in group for OID={packet.projectileOid}");
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
        if (_gameOver)
        {
            Console.WriteLine($"[GameLogicManager] Game has ended. Ignoring request from session {session.SessionID}");
            return;
        }

        try
        {
            Unit attacker = _unitPoolManager.GetUnit(packet.attackerOid);
            Unit target = _unitPoolManager.GetUnit(packet.targetOid);

            if (packet.targetOid < 0)
            {
                // 투사체가 시간 초과로 자동 제거
                attacker.Deactivate(_tickManager.GetCurrentTick());
                Console.WriteLine($"[GameLogicManager] Projectile timed out: {attacker.UnitID}");
                return;
            }

            bool isAttackerProjectile = attacker.UnitTypeIs() == UnitType.Projectile;
            bool isTargetWall = target.UnitTypeIs() == UnitType.WallMaria;

            if (isAttackerProjectile)
            {
                if (isTargetWall)
                    _battleManager.ProcessWallMariaProjectileAttacked(session, packet);
                else
                    _battleManager.ProcessProjectileAttack(session, packet);
            }
            else
            {
                if (isTargetWall)
                    _battleManager.ProcessWallMariaAttacked(session, packet);
                else
                    _battleManager.ProcessAttack(session, packet);
            }

           // Console.WriteLine($"[GameLogicManager] Attack: {packet.attackerOid} -> {packet.targetOid}, Tick={packet.clientAttackedTick} [ {attacker.UnitTypeIs()} ]");
        }
        catch (Exception ex)
        {
            LogManager.Instance.LogError("GameLogicManager", $"OnReciveAttack Error: {ex.Message}");
        }
    }

    /* public void OnReciveAttack(ClientSession session, C_AttackedRequest packet)
     {
         if (_gameOver)
         {
             Console.WriteLine($"[GameLogicManager] Game has ended. Ignoring request from session {session.SessionID}");
             return;
         }


         try
         {
             Unit attackerUnit = _unitPoolManager.GetUnit(packet.attackerOid);
             Unit targetUnit = _unitPoolManager.GetUnit(packet.targetOid);


             if( packet.targetOid < 0 ) // Projectile이 시간 초과
             {
                 attackerUnit.Deactivate(_tickManager.GetCurrentTick());
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
     }*/

    public void OnReceiveTileClaim(ClientSession session, C_TileClaimReq packet)
    {
        if (_gameOver)
        {
            Console.WriteLine($"[GameLogicManager] Game has ended. Ignoring request from session {session.SessionID}");
            return;
        }


        Unit unit = _unitPoolManager.GetUnit(packet.unitOid);
        if (unit == null || !unit.IsActive || unit.PlayerID != session.SessionID)
        {
            Console.WriteLine($"[TileClaim] ❌ Invalid claim attempt: oid={packet.unitOid} || IsUnit Active? :{unit.IsActive} || IsSameWith ReqSession : {unit.PlayerID != session.SessionID}");
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

       // _dimensionManager.Update(this);
        
        Console.WriteLine($"CurrentTime {_gameTimerManager.RemainingSeconds}");

        JobTimer.Instance.Push(Update, 1000);
    }

    private void RegisterTickUnit(Unit unit) => _tickDrivenUnitManager.Register(unit);
    private void UnregisterTickUnit(Unit unit) => _tickDrivenUnitManager.Unregister(unit);
    private void HandleUnitDeActivate(Unit unit)
    {
        S_DeActivateConfirm packet = new S_DeActivateConfirm()
        {
            attackerOid = -1,
            deActivateOid = unit.OId,
            deActivateTick = unit.DeactivateTick,
        };
        Console.WriteLine($"[Dead] {unit.OId} at Tick {unit.DeactivateTick}");
        _room.BroadCast(packet.Write());
    }

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
    public bool GameOver => _gameOver;
    public BattleManager Battle => _battleManager;
    public IReadOnlyDictionary<int, Mana> Manas => _playerManager.Manas;
    public List<Unit> UnitPool => _unitPoolManager.GetAllUnits() as List<Unit>;
    public TickManager TickManager => _tickManager;
    public TileManager TileManager => _tileManager;
    public OccupationManager OccupationManager => _occupationManager;

}
