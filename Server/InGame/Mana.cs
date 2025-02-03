using System;

class Mana
{
    private int _maxMana = 10;
    private int _currentMana;
    private int _regenRate = 1; // 초당 회복량

    public Mana()
    {
        _currentMana = 5; // 초기 마나 5
    }

    public int GetMana() => _currentMana;

    public bool UseMana(int cost)
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
            Console.WriteLine($"마나 회복됨: 현재 마나 {_currentMana}");
        }
    }
}
