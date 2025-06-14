using System;

interface IGameEvent
{
    public long SchedulingTick { get; }        // 주기
    public long ExcutionTick { get; } // 다음 실행 예정 시간
    public void Execute(GameLogicManager logic); // 실행
}

abstract class BaseGameEvent : IGameEvent
{
    public long SchedulingTick { get; private set; }
    public long ExcutionTick { get; private set; }
    public Action OnActive;
    protected BaseGameEvent(long excutionTick, int advanceTick = 40)
    {
        ExcutionTick = excutionTick;
        SchedulingTick = excutionTick - advanceTick;
    }
    public abstract void Execute(GameLogicManager logic);
}


class FeverEvent : BaseGameEvent
{

    public FeverEvent(long excutionTick) : base(excutionTick) { }


    public override void Execute(GameLogicManager logic)
    {
        OnActive.Invoke();
    }

}



/// <summary>
/// 속도 가속 || 차원 순풍
/// </summary>
class UnitSpeedBuffEvent : BaseGameEvent
{

    public UnitSpeedBuffEvent(long excutionTick) : base(excutionTick) { }

    public override void Execute(GameLogicManager logic)
    {
        Console.WriteLine("[Event] 유닛 이동 속도 증가 이벤트 실행");

    }

}
/// <summary>
/// 점령도 초기화 || 태초 상태
/// </summary>
class OccupationResetEvent : BaseGameEvent
{

    public OccupationResetEvent(long excutionTick) : base(excutionTick) { }
    public override void Execute(GameLogicManager logic)
    {
        Console.WriteLine("[Event] 점령도 초기화 이벤트 실행");

    }

}


/// <summary>
/// Position 이동 || 차원 폭풍
/// </summary>
class TimeStormEvent : BaseGameEvent
{
    public TimeStormEvent(long excutionTick) : base(excutionTick) { }


    public override void Execute(GameLogicManager logic)
    {
        Console.WriteLine("[Event] 시간의 폭풍 실행 - 모든 유닛 위치 무작위 변경");


    }

}

