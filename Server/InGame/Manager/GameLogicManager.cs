// GameLogicManager.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Server;


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
        _playerManager = new PlayerManager();
        _deckManager = new DeckManager();
        _unitPoolManager = new UnitPoolManager();
        _positionCache = new PositionCache(_room.Sessions.Values.Select(s =>s.SessionID).ToArray());
        _tickDrivenUnitManager = new TickDrivenUnitManager();
        _occupationManager = new OccupationManager(this, _positionCache);
        _tileManager = new TileManager(_occupationManager, _positionCache);
        _battleManager = new BattleManager(_unitPoolManager, _room, _tickManager, _occupationManager);
        _gameTimerManager = new GameTimerManager(_tickManager);
        _dimensionManager = new DimensionManager(_tickManager);

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
        try
        {
            Console.WriteLine($"[GameLogicManager] Summon requested: OID={packet.oid}, Session={session.SessionID}");

            if (!Manas.TryGetValue(packet.reqSessionID, out Mana mana))
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

        
        Console.WriteLine($"CurrentTime {_gameTimerManager.RemainingSeconds}");

        JobTimer.Instance.Push(Update, 1000);
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
    public BattleManager Battle => _battleManager;
    public IReadOnlyDictionary<int, Mana> Manas => _playerManager.Manas;
    public List<Unit> UnitPool => _unitPoolManager.GetAllUnits() as List<Unit>;
    public TickManager TickManager => _tickManager;
    public TileManager TileManager => _tileManager;
    public OccupationManager OccupationManager => _occupationManager;

}

/*using Server;
using System.Collections.Generic;
using System;
using ServerCore;
using System.Net.Sockets;
using Shared;
using System.Diagnostics;
using System.Net.Mail;
using System.Xml.Linq;
using System.Numerics;

class GameLogicManager
{
    // Admin
    private GameRoom _room;
    private Dictionary<int, ClientSession> _sessions = new Dictionary<int, ClientSession>();
    private Dictionary<int, List<Card>> playerDecks = new Dictionary<int, List<Card>>();

    // Unit
    private int unitPoolSize;
    private List<Unit> _unitPool = new List<Unit>();

    // Util
    private Dictionary<int, Mana> _playerMana = new Dictionary<int, Mana>(); // Mana
    private List<Card> cardPool = new List<Card>();
    private Timer _timer;
    private TickManager _tickManager;
    private DamageCalculator _damageCalculator;

    //state
    private bool _gameOver = false;
    public List<Unit> UnitPool => _unitPool;
    public IReadOnlyDictionary<int, Mana> Manas => _playerMana;
    public Timer Timer => _timer;
    public TickManager TickManager => _tickManager;
    public GameLogicManager(GameRoom room)
    {
        _room = room;
    }

    //DefineData
    const int HpDecreaseTick = 10;
    const int SummonProjectileDelayTick = 5;


    public void Init()
    {
        _timer = new Timer();
        _tickManager = new TickManager();

        unitPoolSize = 10;

        S_InitGame initPackt = new S_InitGame();
        initPackt.gameStartTime = _timer.GameStartTime;
        initPackt.duration = _timer.GameDuration;
        _room.BroadCast(initPackt.Write());

        JobTimer.Instance.Push(Update);

    }
    private void SetUnitPool(List<Card> cardList)
    {
        _unitPool.Clear();
        foreach (Card card in cardList)
        {
            for (int i = 0; i < unitPoolSize; i++)
            {
                Unit unit = UnitFactory.CreateUnit(card.ID, card.LV);
                _unitPool.Add(unit);
            }

        }
        Console.WriteLine($"SetUnitPool : [ {unitPoolSize} ]");
        _damageCalculator = new DamageCalculator(_unitPool);
    }

    public void OnReceiveDeck(ClientSession session, C_SetCardPool packet)
    {
        if (!playerDecks.ContainsKey(session.SessionID))
            playerDecks[session.SessionID] = new List<Card>();

        playerDecks[session.SessionID].Clear();

        // Card Collector
        foreach (var cardData in packet.cardCombinations)
        {
            Card newCard = new Card(cardData.uid, cardData.lv);
            playerDecks[session.SessionID].Add(newCard);
        }

        Console.WriteLine($"Player {session.SessionID} sent their deck to Server.");



        // CardPool Maker
        if (playerDecks.Count == 2) 
        {
            LogManager.Instance.LogInfo("GameLogic", $"CardPool sent each players");
            cardPool.Clear();

            // Castle ID Adition
            List<Card> cards = new List<Card>()
            {
                new Card("CASTLE-U-01", 1),
                new Card("CASTLE-U-02", 1)
            };
            cardPool.AddRange(cards);

            // Player CardPool Reference
            foreach (var deck in playerDecks.Values)
                cardPool.AddRange(deck);

            // Reference each Cards, Add reqire Information
            List<Card> tmpCardPool = new List<Card>(cardPool);

            foreach (Card card in tmpCardPool)
            {
                CardMeta meta = CardMetaDatabase.GetMeta(card.ID, card.LV);
                if (meta != null && meta.IsRanged && !string.IsNullOrEmpty(meta.ProjectileCardID))
                {
                    cardPool.Add(new Card(meta.ProjectileCardID, card.LV));
                    LogManager.Instance.LogInfo("GameLogic", $"[Projectile Add] {meta.ProjectileCardID} for {card.ID}");
                }
            }
            Console.WriteLine("===============CardPool==============");
            foreach(Card card in cardPool)
            {
                Console.WriteLine($"ID :{ card.ID } || LV :{ card.LV }");
            }
            Console.WriteLine("=======================================");

            // Make Packet to Send each client
            S_CardPool poolPacket = new S_CardPool();
            poolPacket.size = unitPoolSize;
            foreach (var card in cardPool)
            {
                poolPacket.cardCombinations.Add(new S_CardPool.CardCombination
                {
                    uid = card.ID,
                    lv = card.LV
                });
            }

            _room.BroadCast(poolPacket.Write());

            SetUnitPool(cardPool);
        }
    }

    public void OnReceiveSummon(ClientSession clientSession, C_ReqSummon packet)
    {
        int delayTick = 30;
        int currentTick = _tickManager.GetCurrentTick();
        int executeTick = currentTick + delayTick;

        Random rng = new Random(currentTick * 1000 + packet.reqSessionID);
        S_AnsSummon response = new S_AnsSummon
        {
            oid = packet.oid,
            reqSessionID = clientSession.SessionID,
            x = packet.x,
            y = packet.y,
            randomValue = rng.Next(0, 10),
            reducedMana = Manas[packet.reqSessionID].GetMana(),
            ExcuteTick = executeTick,
            ServerReceiveTimeMs = _tickManager.GetNowTimeMs(),
            ServerStartTimeMs = _tickManager.GetStartTimeMs(),
            ClientSendTimeMs = packet.ClientSendTimeMs // 클라이언트가 보낸 시각 그대로 회신
        };
        Console.WriteLine($"[Summon]  Player [ {response.reqSessionID} ]" +
                          $" || Oid[ {response.oid} ]" +
                          $" || Position [ {response.x}, {response.y} ]" +
                          $" || Excute Tick [ {executeTick} ]" +
                          $" || CurrentTick [ {currentTick} ]");
        _room.BroadCast(response.Write());

        UnitPool[response.oid].Summon(response);

    }

    public void OnReciveAttack(ClientSession clientSession, C_AttackedRequest packet)
    {
        int hpDecreaseTick = HpDecreaseTick;//packet.hpDecreaseTick;
        int clientAttackedTick = packet.clientAttackedTick;
        int currentTick = _tickManager.GetCurrentTick();

        int excuteAttackVerifyTick = clientAttackedTick + hpDecreaseTick;

        Console.WriteLine($"[Attack] || 실행 :{excuteAttackVerifyTick} || 클라 보낸 시간 : {clientAttackedTick}");

        ///////Check//////////Check//////////Check//////////Check//////////Check//////////Check//////////Check/////
        if (!ValidateAttackRequest(packet, excuteAttackVerifyTick, currentTick, out var reason))
        {
            Console.WriteLine($"[Reject] Attack blocked: {reason}");
            return;
        }
        ///////Check//////////Check//////////Check//////////Check//////////Check//////////Check//////////Check/////


        //Calcul Damage
        float curHp;
        bool isDead = _damageCalculator.ApplyDamageAndCheckDeath(packet.attackerOid, packet.targetOid, out curHp);



        if (isDead)
        {
            UnitPool[packet.targetOid].SetDeadTick(clientAttackedTick); // 피격자의 사망 Tick 저장
            Console.WriteLine($"[ Dead ] || CurrentTick [ {currentTick} ] ||  Kill [ {packet.attackerOid} -> {packet.targetOid}  At {clientAttackedTick} Tick ]");
            UnitPool[packet.targetOid].Dead();
        }

        //Calcul Hit Position


        S_AttackConfirm response = new S_AttackConfirm
        {
            attackerOid = packet.attackerOid,
            targetOid = packet.targetOid,
            targetVerifyHp = curHp,
            //dir =
            //correctedX =
            //correctedY =
            attackVerifyTick = excuteAttackVerifyTick,
        };
        // tmp 투사체 처리
        if (_unitPool[packet.attackerOid].IsProjectile)
        {
            _unitPool[packet.attackerOid].Dead();
            Console.WriteLine($"[ ProjectileActive ] ProjectileOid : {packet.attackerOid} || is Active? : {_unitPool[packet.attackerOid].IsActive} "); 
        }

        //  응답 전송
        _room.BroadCast(response.Write());


        Console.WriteLine($"[ Attck ] ||  {packet.attackerOid} → {packet.targetOid} :  HP[{_unitPool[packet.targetOid].CurrentHP}] IsDead? || {isDead} || Damage: {UnitPool[packet.attackerOid].AttackPower} ]");
    }

    public void OnReciveSummonProject(ClientSession clientSession, C_SummonProJectile packet)
    {
        int clientRequestTick = packet.clientRequestTick;
        int currentTick = _tickManager.GetCurrentTick();

        //Console.WriteLine("sumProjectile");
        int excuteSummonProjectileTick = clientRequestTick + SummonProjectileDelayTick;


        // dir Calcul
        Vector2 attckerPos = new Vector2(packet.summonerX, packet.summonerY);
        Vector2 targetPos = new Vector2(packet.targetX, packet.targetY);

        Vector2 dir = targetPos - attckerPos;

        //if(dir.LengthSquared() > 0.0001f)
        //    dir = Vector2.Normalize(dir);
        
        //float angle = MathF.Atan2(dir.X, dir.Y); //Radian 변환
        //float degress = angle * (180f / MathF.PI); //degress 변환

        //float distance = Vector2.Distance(attckerPos, targetPos);

        float speed = _unitPool[packet.projectileOid].Speed;

        
        // if(석궁 10새끼면 distance 통일 해 줘야함)

        S_ShootConfirm response = new S_ShootConfirm
        {
            projcetileOid = packet.projectileOid,
            summonerOid = packet.summonerOid,
            projectileSpeed = speed,
            startX = packet.summonerX,
            startY = packet.summonerY,
            targetX = targetPos.X,        //TODO : ㅆ~~~발 이거 안고치면 ㅈㄱ됨 
            targetY = targetPos.Y,            //TODO : ㅆ~~~발 이거 안고치면 ㅈㄱ됨 
            shootTick = excuteSummonProjectileTick,
                        
        };

        _unitPool[packet.projectileOid].Summon(response, excuteSummonProjectileTick);
        Console.WriteLine($"Summoner is {packet.summonerOid}  Projectile : {packet.projectileOid} isIsProjectile? :  {_unitPool[packet.projectileOid].IsProjectile} is Active? : {_unitPool[packet.projectileOid].IsActive} ");
        //  응답 전송
        _room.BroadCast(response.Write());

    }

    public int? GetAvailableOid(int oid)
    {


        int definitionIndex = oid % unitPoolSize;
        int objectStartIndex = oid - definitionIndex;
        int endIndex = objectStartIndex + unitPoolSize;

        for (int i = objectStartIndex; i < endIndex; i++)
        {
  


            Console.WriteLine($"ReQOID : {oid} / i : {i} is {_unitPool[i].IsActive} ");
            if (_unitPool[i].IsActive == false)
                return i;
        }

        return null; // 풀 사용 불가
    }


    public void AddPlayer(ClientSession session)
    {
        _playerMana[session.SessionID] = new Mana();
        _sessions[session.SessionID] = session;


        Console.WriteLine($"[게임 로직] 플레이어 {session.SessionID} 추가");
    }
    /// <summary>
    /// 1second loop
    /// </summary>
    public void Update()// :TODO Make JobQueue and push all packet 
    {
        if (_gameOver)
            return;

        S_GameStateUpdate updatePacket = new S_GameStateUpdate();

        foreach (var mana in _playerMana)
        {
            mana.Value.RegenMana();   //BaseRegen Mana = 1  
            S_ManaUpdate packet = new S_ManaUpdate { currentMana = mana.Value.GetMana() };
            _sessions[mana.Key].Send(packet.Write());
        }



        JobTimer.Instance.Push(Update, 1000);
    }

    public void EndGame()
    {
        _gameOver = true;

        // 유닛 풀 정리
        foreach (var unit in _unitPool)
            unit.Reset(); // IsActive, HP 등 초기화 (만약 재사용할 경우)
        _unitPool.Clear();

        // 플레이어 마나 정리
        _playerMana.Clear();

        // 세션 정보 정리
        _sessions.Clear();

        // 카드 정보 정리
        playerDecks.Clear();
        cardPool.Clear();

        // 데미지 계산기 초기화
        _damageCalculator = null;

        // 타이머 및 틱 매니저 (필요하면 null)
        _timer = null;
        _tickManager = null;

        // 로그
        LogManager.Instance.LogInfo("GameLogic", "게임 종료 및 리소스 정리 완료");
        Console.WriteLine("✅ Game Ended - All dynamic resources cleared.");
    }



    public bool ValidateAttackRequest(C_AttackedRequest packet, int executeTick, int currentTick, out string reason)
    {
        reason = "";

        if (packet.attackerOid < 0 || packet.targetOid < 0)
        {
            reason = $"Invalid oid || attacket [ {packet.attackerOid} ] || target[ {packet.targetOid} ]";
            return false;
        }

    *//*    if (!_unitPool[packet.attackerOid].IsActive || !_unitPool[packet.targetOid].IsActive)
        {
            //reason = "Inactive unit";
            reason = $"Inactive unit || attacket [ {packet.attackerOid} is [ {UnitPool[packet.attackerOid].IsActive} ] || target[ {packet.targetOid} is [ {UnitPool[packet.targetOid].IsActive} ]";

            return false;
        }*//*

        // 구종 변경으로 인한 수정
        //if (currentTick - UnitPool[packet.attackerOid].LastAttackExcuteTick < packet.animationDelayTick)
        //{
        //    reason = "Too fast attack";
        //    return false;
        //}

        //if ((currentTick - packet.clientAttackTick) * 2 < packet.animationDelayTick)
        //{
        //    reason = "RTT not enough";
        //    return false;
        //}

        //if (UnitPool[packet.attackerOid].DeadTick < executeTick)
        //{
        //    reason = "Attacker will die before attack";
        //    return false;
        //}

        return true;
    }


}
*/