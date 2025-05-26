using System;

class Mana
{
    private float _maxMana = 10;
    private float _currentMana;
    private float _regenRate = 0.5f; // 초당 회복량

    public Mana()
    {
        _currentMana = 0; // 초기 마나 
    }

    public float SetManaRegenRate(float regenRate)
    {
        Console.WriteLine($"Set{ regenRate}");
        return _regenRate = regenRate;
    }
    public float GetMana() => _currentMana;

    public bool UseMana(float cost)
    {
        if (_currentMana >= cost)
        {
            _currentMana -= cost;
            return true;
        }
        return false;
    }

    public void RegenMana()
    {
        if (_currentMana < _maxMana)
        {
            _currentMana += _regenRate;
            //Console.WriteLine($"마나 회복됨: 현재 마나 {_currentMana}");
        }
    }
}
