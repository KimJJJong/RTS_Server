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
    public void Init()
    {
        // SetCards가 어딘가 호출 되고
        SetPool(10);    // HardCoding ( ! )
    }
    public void SetTimer()
    {
        _gameCurrentTime = DateTime.UtcNow.Ticks / 10000000;
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
    }

    


    public void AddPlayer(ClientSession session)
    {
        _playerMana[session.SessionID] = new Mana();
        
        Console.WriteLine($"[게임 로직] 플레이어 {session.SessionID} 추가");
    }
    /// <summary>
    /// 1second loop
    /// </summary>
    public void Update()
    {
        if (_gameOver) 
            return;

        S_GameStateUpdate sysnc = new S_GameStateUpdate();

        // 마나 자동 회복
/*        foreach (var mana in _playerMana.Values)
        {
            mana.RegenMana();
            sysnc.manas[0].sessionID = _playerMana
        }*/
        // 시간 동기화

        sysnc.serverTime = DateTime.UtcNow.Ticks / 10000000.0;

        _room.BroadCast(sysnc.Write());

        // 게임 종료 체크
        /*        if (_remainingTime <= 0)
                {
                    EndGame();
                }*/
        JobTimer.Instance.Push(Update, 1000);
    }

    private void EndGame()
    {
        _gameOver = true;
        //_room.BroadcastToAll("시간 초과! 게임이 종료되었습니다!");
        _room.EndGame();
    }

    public int GetRemainingTime()
    {
        return _remainingTime;
    }
}
