using Server;
using System.Collections.Generic;
using System;
using ServerCore;

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
        // SetCards가 어딘가 호출 되고
        List<Card> testSet01 = new List<Card>
        {
            new Card(1, 1),
            new Card(2, 1),
            new Card(3, 1),
            new Card(4, 1),
            new Card(5, 1)
        };
        List<Card> testSet02 = new List<Card>
        {
            new Card(1, 1),
            new Card(2, 1),
            new Card(3, 1),
            new Card(4, 1),
            new Card(5, 1)
        };
        SetCards( testSet01, testSet02 );

        int poolSize = 10;
        SetPool(poolSize);    // HardCoding ( ! )

        S_InitGame initPackt= new S_InitGame();
        initPackt.size = poolSize;
        initPackt.cards. = _cardCombination


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
            int uid = _cardCombination[i].ID;
            for(int ii=0; ii< size; ii++)
            {
                index++;
                _unitPool.Add(new Unit(index, uid));
            }
        }
        Console.WriteLine($"GameRoom[{_room.RoomId}] SetPool Size[{size}] Compl");
    }

    


    public void AddPlayer(ClientSession session)
    {
        _playerMana[session.SessionID] = new Mana();
        _sessions[session.SessionID] = session;

        Init();

        Console.WriteLine($"[게임 로직] 플레이어 {session.SessionID} 추가");
    }
    /// <summary>
    /// 1second loop
    /// </summary>
    public void Update()
    {
        if (_gameOver) 
            return;


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
        return DateTime.UtcNow.Ticks / 10000000.0; // 초 단위 변환 (1 Tick = 100ns)
    }

    private void 
}
