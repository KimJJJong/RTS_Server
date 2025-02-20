using Server;
using System.Collections.Generic;
using System;
using ServerCore;
using System.Net.Sockets;

class GameLogicManager
{
    private Dictionary<int,ClientSession> _sessions = new Dictionary<int, ClientSession>();
    private Dictionary<int, Mana> _playerMana = new Dictionary<int, Mana>(); // Mana
    private GameRoom _room;
    private bool[][] _grid;
    private List<Unit> _unitPool;
    private List<Card> _cardCombination; // int = unitID;
    private double _gameCurrentTime;

    private int _gameDuration = 180; // 게임 제한 시간 (초)
    private int _remainingTime;
    private bool _gameOver = false;

    public IReadOnlyDictionary<int,ClientSession> Sessions => _sessions;
    public IReadOnlyDictionary<int, Mana> Manas => _playerMana;

    public GameLogicManager(GameRoom room)
    {
        _room = room;
        _remainingTime = _gameDuration;
    }
    /// <summary>
    /// CardSet Init
    /// Pooling Init
    /// Loop Sync Event Init
    /// </summary>
    public void Init()
    {
        List<Card> testSet01 = new List<Card>
        {
            new Card("U-JOS-001", 1),
            new Card("U-JOS-002", 1),
            new Card("U-JOS-003", 1),
            new Card("U-JOS-004", 1),
            new Card("SP-PNT-001", 1),
            new Card("SP-SUP-001", 1),
            new Card("TWR-ATK-001", 1),
            new Card("TWR-DEF-001", 1),
        };
        List<Card> testSet02 = new List<Card>
        {
            new Card("U-JOS-001", 1),
            new Card("U-JOS-002", 1),
            new Card("U-JOS-003", 1),
            new Card("U-JOS-004", 1),
            new Card("SP-PNT-001", 1),
            new Card("SP-SUP-001", 1),
            new Card("TWR-ATK-001", 1),
            new Card("TWR-DEF-001", 1),       
        };

        SetCards( testSet01, testSet02 );
        int poolSize = 10;
        SetPool(poolSize);    

        S_InitGame initPackt= new S_InitGame();
        foreach (var card in _cardCombination)
        {
            initPackt.cardCombinations.Add(new S_InitGame.CardCombination { uid = card.ID.ToString(), lv = card.LV });
        }
        
        initPackt.size = poolSize;
        _room.BroadCast(initPackt.Write());

        JobTimer.Instance.Push(Update);

    }

    public void SetCards(List<Card> p1, List<Card> p2)
    {
        _cardCombination = new List<Card>(p1);
        _cardCombination.AddRange(p2);
    }
    public void SetPool(int size)
    {
        _unitPool = new List<Unit>();

        int index = 0;
        for(int i=0; i< _cardCombination.Count; i++)
        {
           //int uid = _cardCombination[i].ID;
            for(int ii=0; ii< size; ii++)
            {
                index++;
                _unitPool.Add(new Unit(index));
            }
        }
        Console.WriteLine($"GameRoom[{_room.RoomId}] SetPool Size[{size}] Compl");
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


        //Time Sync : 일단 안쓸겁니다 TODO 단발성 이벤트 시간 동기화 먼저 처리하고 진행
      /*  S_SyncTime syncPacket = new S_SyncTime();
        syncPacket.serverTime = GetServerTime();
        _room.BroadCast(syncPacket.Write());*/


        JobTimer.Instance.Push(Update, 1000);
    }

    private void EndGame()
    {
        _gameOver = true;
        //_room.BroadcastToAll("시간 초과! 게임이 종료되었습니다!");
        _room.EndGame();
    }

    

    public double GetServerTime()
    {
        return DateTime.UtcNow.Ticks / 100000.0; // 초 단위 변환 (1 Tick = 100ns) / 밀리초(1ms = 0.001s) 단위 반환 (1 Tick = 100ns = 0.0001ms = 0.0000001s)
    }

}
