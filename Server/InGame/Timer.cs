using System;
using System.Collections.Generic;
using System.Text;


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
        }
        public void Start(long startTime)
        {
            _gameStartTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        }
        public double GetServerTime()
        {
            _gameCurrentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            double currentTime = _gameDuration + (_gameStartTime - _gameCurrentTime);
            return currentTime; // 초 단위 변환 (1 Tick = 100ns) / 밀리초(1ms = 0.001s) 단위 반환 (1 Tick = 100ns = 0.0001ms = 0.0000001s)
        }
    }
