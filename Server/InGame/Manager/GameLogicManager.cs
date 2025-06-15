// GameLogicManager.cs
using Server;
using Shared;
using System;
using System.Linq;


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
    private EventManager _dimensionManager;



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
        _dimensionManager = new EventManager(_tickManager, _playerManager);

        Init();
    }

    public void Init()
    {
        _timerManager.Init();
        _dimensionManager.Init();
        _occupationManager.Init(_room.Players);
        _tileManager.Init(_room.Players);
        _battleManager.Init(_room.Players);
        _deckManager.Init(_room.PlayeCardDeckCombination);
        _playerManager.Init(_room.ClientSessions, _tickManager);
        _unitPoolManager.Init(_deckManager.CardPoolList);

        _room.BroadCast(MakeInitBundlePacket().Write());
        S_MyPlayerInfoPacket();

        JobTimer.Instance.Push(Update);
    }




    public void OnReceiveSummon(ClientSession session, C_ReqSummon packet)
    {
        if (_gameOver)
        {
            Console.WriteLine($"[GameLogicManager] Game has ended. Ignoring request from session {session.PlayingID}");
            return;
        }

        try
        {
            //Console.WriteLine($"[GameLogicManager] Summon requested: OID={packet.oid}, Session={session.SessionID}");

            if (!_playerManager.Manas.TryGetValue(packet.reqSessionID, out Mana mana))
            {
                Console.WriteLine("[GameLogicManager] X 마나 정보 없음");
                return;
            }
            float available = mana.GetMana();
            bool success = mana.UseMana(packet.needMana);


            if (!success)
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
                int? _available = _unitPoolManager.GetAvailableOid(packet.oid);
                if (_available == null)
                {
                    Console.WriteLine($"[GameLogicManager] X No available unit in group for OID={packet.oid}");
                    S_AnsSummon s_AnsSummon = new S_AnsSummon()
                    {
                        canSummon = false
                    };
                    _room.SendToPlayer(packet.reqSessionID, s_AnsSummon.Write());

                    return;
                }
                packet.oid = _available.Value;
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
            Console.WriteLine($"[GameLogicManager] Game has ended. Ignoring request from session {session.PlayingID}");
            return;
        }

        try
        {
            //Console.WriteLine($"[GameLogicManager] Projectile summon: OID={packet.projectileOid}, Tick={packet.clientRequestTick}");

            Unit unit = _unitPoolManager.GetUnit(packet.projectileOid);
            if (unit?.IsActive == true)
            {
                int? available = _unitPoolManager.GetAvailableOid(packet.projectileOid);
                if (available == null)
                {
                    //Console.WriteLine($"[GameLogicManager] X No available projectile in group for OID={packet.projectileOid}");
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
            Console.WriteLine($"[GameLogicManager] Game has ended. Ignoring request from session {session.PlayingID}");
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

    public void OnReceiveTileClaim(ClientSession session, C_TileClaimReq packet)
    {
        if (_gameOver)
        {
            Console.WriteLine($"[GameLogicManager] Game has ended. Ignoring request from session {session.PlayingID}");
            return;
        }

        Unit unit = _unitPoolManager.GetUnit(packet.unitOid);
        if (unit == null || !unit.IsActive || unit.PlayerID != session.PlayingID)
        {
            Console.WriteLine($"[TileClaim] XXXX Invalid claim attempt: oid={packet.unitOid}");
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

        //Console.WriteLine($"CurrentTime {_timerManager.RemainingSeconds}");

        if (_timerManager.IsTimeUp()) _occupationManager.CheckFinalWinner();

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
            serverSendTime = _tickManager.GetNowTimeMs(),
            duration = _timerManager.DurationTick,
            size = 10,      // ObjectPool Size
            cardCombinationss = cardPool.Select(card => new S_GameInitBundle.CardCombinations
            {
                uid = card.ID,
                lv = card.LV
            }).ToList()
        };
    }
    public void S_MyPlayerInfoPacket()
    {
        foreach(var sessioId in _room.ClientSessions)
        {
            S_MyPlayerInfo packet = new S_MyPlayerInfo();
            packet.internalSessionId = sessioId.PlayingID;
            sessioId.Send(packet.Write());
        }
    }


    public void RegisterTickUnit(Unit unit) => _tickDrivenUnitManager.Register(unit);
    public void UnregisterTickUnit(Unit unit) => _tickDrivenUnitManager.Unregister(unit);
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

        if (_room.ConnectedCount == 2)
        {
            S_GameOver s_GameOver = new S_GameOver()
            {
                winnerId = winnerSessionId,
                resultMessage = ":<",
            };
            _room.BroadCast(s_GameOver.Write());
        }
        else if (_room.ConnectedCount == 1)
        {
            S_GameOver s_GameOver = new S_GameOver()
            {
                winnerId = winnerSessionId,
                resultMessage = ":<",
            };
            _room.SendToPlayer(winnerSessionId, s_GameOver.Write());
        }
        //////////////////to LobbyServer/////////////////

        SendGameResult(_room.RoomId, winnerSessionId);

        /////////////////////////////////////////////
        _gameOver = true;
        _playerManager.Clear();
        _deckManager.Clear();
        _unitPoolManager.Clear();
        _occupationManager.Clear();
        _tickDrivenUnitManager.Clear();
        _tileManager.Clear();



        Console.WriteLine($"[GameLogicManager] Game ended. Winner: Session {winnerSessionId}");
    }
    public void SendGameResult(string thisRoomId, int winnerIdforClient)
    {
        bool draw;
        if (winnerIdforClient == -1)
            draw = true;
        else
        {
            draw = false;
        }
        string winnerUserId = _room.GetExternalId(winnerIdforClient);
        string loserUserId = _room.GetExternalId(1 - winnerIdforClient); // 상대쪽 계산
        S_M_GameResult packet = new S_M_GameResult
        {
            roomId = thisRoomId,
            isDraw = draw, 
            winnerId = winnerUserId
        };

        SessionManager.Instance.SessionFind(1).Send(packet.Write());
    }

    public void SendToPlayer(int sessionId, ArraySegment<byte> segment) => _room.SendToPlayer(sessionId, segment);


}
