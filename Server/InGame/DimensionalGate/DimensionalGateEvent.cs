//using System;
//using System.Collections.Generic;

//interface IGameEvent
//{
//    public float Interval { get; }        // 주기
//    public float NextTriggerTime { get; } // 다음 실행 예정 시간
//    public void Execute(GameLogicManager logic); // 실행
//    public void ScheduleNext(float currentTime); // 다음 실행 예약
//}

///// <summary>
///// 점령도 초기화
///// </summary>
//class OccupationResetEvent : IGameEvent
//{
//    public float Interval => 45f;
//    public float NextTriggerTime { get; private set; }

//    public void Execute(GameLogicManager logic)
//    {
//        Console.WriteLine("[Event] 점령도 초기화 이벤트 실행");
//        logic.OccupationManager.Clear();
//        logic.TileManager.ResetTiles(); // 예: 모든 타일 초기화 및 균등 분배
//    }

//    public void ScheduleNext(float currentTime)
//    {
//        NextTriggerTime = currentTime + Interval;
//    }
//}

///// <summary>
///// 속도 가속
///// </summary>
//class UnitSpeedBuffEvent : IGameEvent
//{
//    public float Interval => 30f;
//    public float NextTriggerTime { get; private set; }

//    public void Execute(GameLogicManager logic)
//    {
//        Console.WriteLine("[Event] 유닛 이동 속도 증가 이벤트 실행");
//        foreach (var unit in logic.UnitPool)
//        {
//            if (unit.IsActive)
//                unit.ApplySpeedBuff(); // 이 함수는 유닛 내부에 존재한다고 가정
//        }
//    }

//    public void ScheduleNext(float currentTime)
//    {
//        NextTriggerTime = currentTime + Interval;
//    }
//}

///// <summary>
///// Position 이동
///// </summary>
// class TimeStormEvent : IGameEvent
//{
//    public float Interval => 60f; // 예: 60초 주기
//    public float NextTriggerTime { get; private set; }

//    private static Random _rand = new Random();

//    public void Execute(GameLogicManager logic)
//    {
//        Console.WriteLine("[Event] 시간의 폭풍 실행 - 모든 유닛 위치 무작위 변경");

     
//    }

//    public void ScheduleNext(float currentTime)
//    {
//        NextTriggerTime = currentTime + Interval;
//    }
//}

