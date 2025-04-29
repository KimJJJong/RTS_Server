using System.Collections.Generic;
using Server;

class UnitPoolManager
{
    private List<Unit> _unitPool = new List<Unit>();
    private int _unitPoolSize = 10;

    public void Initialize(List<Card> cards)
    {
        _unitPool.Clear();

        foreach (var card in cards)
        {
            for (int i = 0; i < _unitPoolSize; i++)
            {
                Unit unit = UnitFactory.CreateUnit(card.ID, card.LV);
                _unitPool.Add(unit);
            }
        }
    }

    public Unit GetUnit(int oid)
    {
        if (oid < 0 || oid >= _unitPool.Count)
            return null;
        return _unitPool[oid];
    }

    public void Clear()
    {
        _unitPool.Clear();
    }
}
