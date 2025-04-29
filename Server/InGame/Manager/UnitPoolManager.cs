using System;
using System.Collections.Generic;

class UnitPoolManager
{
    private List<Unit> _unitPool = new List<Unit>();
    private int _unitPoolSize = 10; // 각 카드당 풀 크기

    public void Initialize(List<Card> cardPool)
    {
        _unitPool.Clear();
        foreach (var card in cardPool)
        {
            for (int i = 0; i < _unitPoolSize; i++)
            {
                Unit unit = UnitFactory.CreateUnit(card.ID, card.LV);
                _unitPool.Add(unit);
            }
        }
        Console.WriteLine($"[UnitPoolManager] 총 유닛 수: {_unitPool.Count}");
    }

    public Unit GetUnit(int oid)
    {
        if (oid >= 0 && oid < _unitPool.Count)
            return _unitPool[oid];
        return null;
    }

    public IEnumerable<Unit> GetAllUnits() => _unitPool;

    public int? GetAvailableOid(string cardID, int poolSize)
    {
        int definitionIndex = _unitPoolSize;

        for (int i = 0; i < _unitPool.Count; i += definitionIndex)
        {
            for (int j = 0; j < definitionIndex; j++)
            {
                int index = i + j;
                if (index < _unitPool.Count && !_unitPool[index].IsActive)
                    return index;
            }
        }

        return null; // 사용 가능한 유닛 없음
    }

    public void ResetAll()
    {
        foreach (var unit in _unitPool)
            unit.Reset();
    }

    public void Clear()
    {
        _unitPool.Clear();
    }
}
