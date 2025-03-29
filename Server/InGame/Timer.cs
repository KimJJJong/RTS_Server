using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class Timer
    {
        private double _gameStartTime;
        private double _gameCurrentTime;

        private double _gameDuration; // 게임 제한 시간 (초)

        public double GameDuration => _gameDuration;
        public double GameStartTime => _gameStartTime;
        public Timer()
        {
            _gameDuration = 180;
            _gameStartTime = DateTime.UtcNow.Ticks * 1e-7;
        }
        public double GetServerTime()
        {
            _gameCurrentTime = DateTime.UtcNow.Ticks * 1e-7;
            double currentTime = _gameDuration + (_gameStartTime - _gameCurrentTime);
            Console.WriteLine($"CurrentServerTime : { currentTime }");
            return currentTime; // 초 단위 변환 (1 Tick = 100ns) / 밀리초(1ms = 0.001s) 단위 반환 (1 Tick = 100ns = 0.0001ms = 0.0000001s)
        }
        public double GetServerTick()
        {
            _gameCurrentTime = DateTime.UtcNow.Ticks * 1e-7;
            double currentTime = (_gameCurrentTime-_gameStartTime);
            Console.WriteLine($"CurrentServerTime : {currentTime}");
            return currentTime; // 초 단위 변환 (1 Tick = 100ns) / 밀리초(1ms = 0.001s) 단위 반환 (1 Tick = 100ns = 0.0001ms = 0.0000001s)
        }
    }
}
