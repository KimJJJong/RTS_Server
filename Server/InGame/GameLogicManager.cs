using Server;
using System.Collections.Generic;
using System;
using ServerCore;
using System.Net.Sockets;
using Shared;
using System.Diagnostics;
using System.Net.Mail;

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


    public void Init()
    {
        _timer = new Timer();
        _tickManager = new TickManager();

        unitPoolSize = 20;

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

        // CardData -> Card 변환 후 저장
        foreach (var cardData in packet.cardCombinations)
        {
            Card newCard = new Card(cardData.uid, cardData.lv);
            playerDecks[session.SessionID].Add(newCard);
        }

        Console.WriteLine($"Player {session.SessionID} sent their deck.");

        if (playerDecks.Count == 2) // 두 플레이어의 덱을 모두 받았으면 카드 풀 생성
        {
            LogManager.Instance.LogInfo("GameLogic", $"CardPool sent to players");
            cardPool.Clear();
            foreach (var deck in playerDecks.Values)
                cardPool.AddRange(deck);

            //Console.WriteLine("Card pool is ready, sending to players.");

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
            foreach (var card in poolPacket.cardCombinations)
            {
                Console.WriteLine($"UID : {card.uid} || LV : {card.lv}");
            }
            Console.WriteLine("UnitPoolSize");
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
        Console.WriteLine($"Player [ {response.reqSessionID} ]" +
                          $" || Oid[ {response.oid} ]" +
                          $" || Position [ {response.x}, {response.y} ]" +
                          $" || Excute Tick [ {executeTick} ]" +
                          $" || CurrentTick [ {currentTick} ]" +
                          $" || CurrentTimeMs [ {response.ServerReceiveTimeMs} ]");
        _room.BroadCast(response.Write());

        UnitPool[response.oid].Summon(response);
        //UnitPool[packet.oid].SetActive(true);
        //for (int i = 0; i < UnitPool.Count; i++)
        //{
        //    Console.WriteLine($" oid [{i}] : {UnitPool[i].IsActive}");
        //}
    }

    public void OnReciveAttack(ClientSession clientSession, C_AttackedRequest packet)
    {
        int hpDecreaseTick = packet.hpDecreaseTick;
        int clientAttackedTick = packet.clientAttackedTick;
        int currentTick = _tickManager.GetCurrentTick();

        int excuteAttackVerifyTick = clientAttackedTick + hpDecreaseTick;

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

        // Set LastAttackExcuteTick wich UnitPool[oid]
        UnitPool[packet.attackerOid].SetLastAttackExcuteTick(clientAttackedTick);

        if (isDead)
        {
            UnitPool[packet.targetOid].SetDeadTick(clientAttackedTick); // 피격자의 사망 Tick 저장
            Console.WriteLine($"Player [ {clientSession.SessionID} ] || CurrentTick [ {currentTick} ] || Target [ {packet.targetOid} died by {packet.attackerOid} in {clientAttackedTick} ]");
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

        //  응답 전송
        _room.BroadCast(response.Write());

        Console.WriteLine($"Player [ {clientSession.SessionID} ] || [Attack] {packet.attackerOid} → {packet.targetOid} :  HP[{_unitPool[packet.targetOid].CurrentHP}] IsDead? {isDead} | Damage: {UnitPool[packet.attackerOid].AttackPower}, || VerifyTick: {excuteAttackVerifyTick} CurrentTick[ {currentTick}");
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
    }


    public bool ValidateAttackRequest(C_AttackedRequest packet, int executeTick, int currentTick, out string reason)
    {
        reason = "";

        if (packet.attackerOid < 0 || packet.targetOid < 0)
        {
            reason = $"Invalid oid || attacket [ {packet.attackerOid} ] || target[ {packet.targetOid} ]";
            return false;
        }

        if (!_unitPool[packet.attackerOid].IsActive || !_unitPool[packet.targetOid].IsActive)
        {
            //reason = "Inactive unit";
            reason = $"Inactive unit || attacket [ {packet.attackerOid} is [ {UnitPool[packet.attackerOid].IsActive} ] || target[ {packet.targetOid} is [ {UnitPool[packet.targetOid].IsActive} ]";

            return false;
        }

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
