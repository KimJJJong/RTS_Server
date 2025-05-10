using System;
using System.Collections.Generic;
using System.Text;

class DimensionManager
{
    private TickManager _tickManager;
    private Queue<IGameEvent> _dimensionEventList;

    public const long  Morning =     1818;
    public const long  Afternoon =   1818 * 2;
    public const long  Evening =     1818 * 3;

    public DimensionManager (TickManager tickManager)
    {
        _tickManager = tickManager;
    }

    public void Init()  // Event Sequn Setting
    {
        _dimensionEventList = SetEventBundle(1);
    }

    public void Update(GameLogicManager logic)
    {
        long currentTick = _tickManager.GetCurrentTick();
        if (_dimensionEventList.Count > 0 &&
            currentTick >= _dimensionEventList.Peek().SchedulingTick)
        {
            var gameEvent = _dimensionEventList.Dequeue();
            Console.WriteLine($"[DimensionEvent] Executing at Tick {currentTick}: {gameEvent.GetType().Name}");
            gameEvent.Execute(logic);
        }
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
    public void Clear()
    {
        _dimensionEventList.Clear();
    }
}
