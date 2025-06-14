using System;
using System.Collections.Generic;
using System.Text;

class EventManager
{
    private TickManager _tickManager;
    private PlayerManager _playerManager;
    private Queue<IGameEvent> _dimensionEventList;
    private Queue<IGameEvent> _feverEventList;

    public const long Morning = 1818;
    public const long Afternoon = 1818 * 2;
    public const long Evening = 1818 * 3;
    public const long FeverTime = 1818 * 3;
    public EventManager(TickManager tickManager, PlayerManager playerManager)
    {
        _tickManager = tickManager;
        _playerManager = playerManager;
    }

    public void Init()  // Event Sequn Setting
    {
        _dimensionEventList = SetEventBundle(1);
        _feverEventList = FeverEventBundle(1);
    }

    public void Update(GameLogicManager logic)
    {
        long currentTick = _tickManager.GetCurrentTick();
        if (_dimensionEventList.Count > 0 &&
             currentTick >= _dimensionEventList.Peek().SchedulingTick)
        {
            var gameEvent = _dimensionEventList.Dequeue();
            Console.WriteLine($"[Event] Scheduled In {gameEvent.SchedulingTick} || Executing at Tick {gameEvent.ExcutionTick}: {gameEvent.GetType().Name}");
            gameEvent.Execute(logic);
        }
        if (_feverEventList.Count > 0 && currentTick >= _feverEventList.Peek().SchedulingTick)
        {
            var feverEvent = _feverEventList.Dequeue();
            Console.WriteLine($"[Event] Scheduled In {feverEvent.SchedulingTick} || Executing at Tick {feverEvent.ExcutionTick}: {feverEvent.GetType().Name}");
            feverEvent.Execute(logic);
        }
    }
    public Queue<IGameEvent> FeverEventBundle(int bundleNum)        //Fevet에서 Player Mana 접근 해야하는데 EventContent에서 접근하기 너무 지저분 하닌까 CallBack으로 할까?
    {
        Queue<IGameEvent> tmpEvent = new Queue<IGameEvent>();

        switch (bundleNum)
        {
            case 1:
                FeverEvent tmpFever = new FeverEvent(FeverTime);
                tmpFever.OnActive += FeverActiveHandler;
                tmpEvent.Enqueue(tmpFever);
                break;

            case 2:
                break;

            case 3:
                break;

            default:
                break;
        }

        return tmpEvent;
    }
    public Queue<IGameEvent> SetEventBundle(int bundleNum)
    {
        Queue<IGameEvent> tmpEvent = new Queue<IGameEvent>();

        switch (bundleNum)
        {
            case 1:
                tmpEvent.Enqueue(new UnitSpeedBuffEvent(Morning));
                tmpEvent.Enqueue(new OccupationResetEvent(Afternoon));
                tmpEvent.Enqueue(new TimeStormEvent(Evening));
                break;

            case 2:
                break;

            case 3:
                break;

            default:
                break;
        }

        return tmpEvent;
    }

    public void FeverActiveHandler()
    {
        Console.WriteLine("Fever");
        _playerManager.SetManaRegenRate(0.6f);
    }

    public void Clear()
    {
        _dimensionEventList.Clear();
    }
}
