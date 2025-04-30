using System;
using System.Collections.Generic;

class TickDrivenUnitManager
{
    private readonly List<Unit> _tickUnits = new List<Unit>();

    public void Register(Unit unit)
    {
       // Console.WriteLine($"[Unit : {unit.UnitID} is Registing]");
        if (!_tickUnits.Contains(unit))
            _tickUnits.Add(unit);
    }

    public void Unregister(Unit unit)
    {
        //Console.WriteLine($"[Unit : {unit.UnitID} is UnRegisting]");
        _tickUnits.Remove(unit);
    }

    public void Update(int currentTick)
    {
        foreach (var unit in _tickUnits.ToArray()) // 복사본을 순회
        {
            Console.WriteLine($"[TickDriven] Tick {currentTick} → Unit: {unit.UnitID}");
            unit?.TickUpdate(currentTick);
        }
    }



    public void Clear()
    {
        _tickUnits.Clear();
    }
}

// ITickable.cs
public interface ITickable
{
    void TickUpdate(int currentTick);
}
