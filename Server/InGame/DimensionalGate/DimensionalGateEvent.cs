using System;
using System.Collections.Generic;

interface IGameEvent
{
    public long SchedulingTick { get; }        // 주기
    public long ExcutionTick { get; } // 다음 실행 예정 시간
    public void Execute(GameLogicManager logic); // 실행
}




/// <summary>
/// 속도 가속 || 차원 순풍
/// </summary>
class UnitSpeedBuffEvent : IGameEvent
{
    public long SchedulingTick { get; } 
    public long ExcutionTick { get; } 
    public UnitSpeedBuffEvent(long excutionTick)
    {
        SchedulingTick = excutionTick - 30;
        ExcutionTick = excutionTick;
    }

    public void Execute(GameLogicManager logic)
    {
        Console.WriteLine("[Event] 유닛 이동 속도 증가 이벤트 실행");
        //foreach (var unit in logic.UnitPool)
        //{

        //}
    }

}
/// <summary>
/// 점령도 초기화 || 태초 상태
/// </summary>
class OccupationResetEvent : IGameEvent
{
    public long SchedulingTick { get; }
    public long ExcutionTick { get; }
    public OccupationResetEvent(long excutionTick)
    {
        SchedulingTick = excutionTick - 30;
        ExcutionTick = excutionTick;
    }
    public void Execute(GameLogicManager logic)
    {
        Console.WriteLine("[Event] 점령도 초기화 이벤트 실행");
        //logic.OccupationManager.Clear();
        //logic.TileManager.ResetTiles(); // 예: 모든 타일 초기화 및 균등 분배
    }

}


/// <summary>
/// Position 이동 || 차원 폭풍
/// </summary>
class TimeStormEvent : IGameEvent
{
    public long SchedulingTick { get; }
    public long ExcutionTick { get; }
    public TimeStormEvent(long excutionTick)
    {
        SchedulingTick = excutionTick - 30;
        ExcutionTick = excutionTick;
    }

    private static Random _rand = new Random();

    public void Execute(GameLogicManager logic)
    {
        Console.WriteLine("[Event] 시간의 폭풍 실행 - 모든 유닛 위치 무작위 변경");


    }

}

