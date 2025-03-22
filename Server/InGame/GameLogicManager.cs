using Server;
using System.Collections.Generic;
using System;
using ServerCore;
using System.Net.Sockets;
using Shared;

class GameLogicManager
{
    private Dictionary<int,ClientSession> _sessions = new Dictionary<int, ClientSession>();
    private Dictionary<int, Mana> _playerMana = new Dictionary<int, Mana>(); // Mana
    private GameRoom _room;
   

    ////////
    private Dictionary<int, List<Card>> playerDecks = new Dictionary<int, List<Card>>();
    private List<Card> cardPool = new List<Card>();

    private bool _gameOver = false;
    private Timer _timer;
    //public IReadOnlyDictionary<int,ClientSession> Sessions => _sessions;
    public IReadOnlyDictionary<int, Mana> Manas => _playerMana;
    public Timer Timer
    {
        get => _timer;
    }
    public GameLogicManager(GameRoom room)
    {
        _room = room;
    }
    /// <summary>
    /// CardSet Init
    /// Pooling Init
    /// Loop Sync Event Init
    /// </summary>
    public void Init()
    {
        S_InitGame initPackt= new S_InitGame();

    

        _timer = new Timer();
        initPackt.gameStartTime = _timer.GameStartTime;
        initPackt.duration = _timer.GameDuration;

        _room.BroadCast(initPackt.Write());

        JobTimer.Instance.Push(Update);

    }
    /////////
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
            cardPool.Clear();
            foreach (var deck in playerDecks.Values)
                cardPool.AddRange(deck);

            Console.WriteLine("Card pool is ready, sending to players.");
            LogManager.Instance.LogInfo("GameLogic", $"CardPool sent to players");

            S_CardPool poolPacket = new S_CardPool();
            // Card -> CardData 변환 후 패킷에 추가
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
                Console.WriteLine($"UID : {card.uid}");
            }

            // 양쪽 플레이어에게 카드 풀 전송
            _room.BroadCast(poolPacket.Write());
            
            
        }
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

        foreach(var mana in _playerMana)
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
        //_room.BroadcastToAll("시간 초과! 게임이 종료되었습니다!");
       // _room.EndGame();
    }

}
