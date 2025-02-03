using Server;
using System.Collections.Generic;
using System;
using ServerCore;

class GameLogicManager
{
    public List<ClientSession> Sessions = new List<ClientSession>();
    private GameRoom _room;
    private Dictionary<int, Mana> _playerMana = new Dictionary<int, Mana>(); // Mana
    private bool[][] _grid;
    private List<Unit> _unitPool;
    private List<Card> _cardCombination; // int = unitID;

    private int _gameDuration = 180; // 게임 제한 시간 (초)
    private int _remainingTime;
    private bool _gameOver = false;

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

    public void Update()
    {
        if (_gameOver) 
            return;

        _remainingTime--;  // 1초 감소
        Console.WriteLine($"게임 남은 시간: {_remainingTime}초");

        // 마나 자동 회복
        foreach (var mana in _playerMana.Values)
        {
            mana.RegenMana();
        }

        // 게임 종료 체크
        if (_remainingTime <= 0)
        {
            EndGame();
        }
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
