using Server;
using System.Collections.Generic;
using System;

class GameLogicManager
{
    private GameRoom _room;
    private Dictionary<int, Mana> _playerMana = new Dictionary<int, Mana>(); // Mana
    private bool[][] _grid;

    private int _gameDuration = 180; // 게임 제한 시간 (초)
    private int _remainingTime;
    private bool _gameOver = false;

    public GameLogicManager(GameRoom room)
    {
        _room = room;
        _remainingTime = _gameDuration;
    }

    public void AddPlayer(ClientSession session)
    {
        _playerMana[session.SessionID] = new Mana();
        //_grid;  :  grid Init ?
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
