using System.Collections.Generic;

class TickDrivenUnitManager
{
    private readonly List<Unit> _tickUnits = new List<Unit>();

    public void Register(Unit unit)
    {
        if (!_tickUnits.Contains(unit))
            _tickUnits.Add(unit);
    }

    public void Unregister(Unit unit)
    {
        _tickUnits.Remove(unit);
    }

    public void Update(int currentTick)
    {
        foreach (var unit in _tickUnits)
            unit?.TickUpdate(currentTick);
    }
}

// ITickable.cs
public interface ITickable
{
    void TickUpdate(int currentTick);
}
