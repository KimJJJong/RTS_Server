using System;

class Mana
{
    private float _maxMana = 10;
    private float _currentMana;
    private float _regenRate = 0.4f; // 초당 회복량
    private double _lastUpdateTime;  // UTC 기준 마지막 갱신 시간 (초)

    public Mana()
    {
        _currentMana = 8f;
        _lastUpdateTime = GetCurrentTime();
    }

    private double GetCurrentTime() => DateTime.UtcNow.Ticks * 1e-7;

    public float SetManaRegenRate(float regenRate)
    {
        Console.WriteLine($"Set {regenRate}");
        return _regenRate = regenRate;
    }

    public float GetMana()
    {
        UpdateMana(); // 호출 시점까지 보정된 값
        return _currentMana;
    }

    public bool UseMana(float cost)
    {
        UpdateMana(); // 보정된 후 실제 사용

        if (_currentMana >= cost)
        {
            _currentMana -= cost;
            return true;
        }
        return false;
    }

    public void RegenMana()
    {
        UpdateMana(); // 주기적 호출에도 보정은 항상 반영되게
    }

    /// <summary>
    /// 현재 시점까지 경과한 시간만큼 마나를 회복
    /// </summary>
    private void UpdateMana()
    {
        double now = GetCurrentTime();
        double elapsed = now - _lastUpdateTime;

        if (elapsed <= 0 || _currentMana >= _maxMana)
            return;

        float regenAmount = (float)(elapsed * _regenRate);
        _currentMana = Math.Min(_maxMana, _currentMana + regenAmount);
        _lastUpdateTime = now;
        Console.WriteLine($"[Mana] : {_currentMana}");
    }
}
