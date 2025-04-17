using System.Collections.Generic;
using System;

public class DamageCalculator
{
    private List<Unit> _unitPool;

    public DamageCalculator(List<Unit> unitPool)
    {
        _unitPool = unitPool;
    }

    public float Calculate(int attackerOid, int targetOid)
    {
        if (_unitPool[attackerOid].IsActive || _unitPool[targetOid].IsActive)
            return 0; //이거 데미지 처리도 들어가는거여서 조심해서 처리

        Unit attacker = _unitPool[attackerOid];
        Unit target = _unitPool[targetOid];

        float Damage = attacker.AttackPower;


        return Damage; 
    }



    public bool ApplyDamageAndCheckDeath(int attackerOid, int targetOid, out float damage)
    {
        damage = 0;

        if (attackerOid < 0 || targetOid < 0) return false;
        if (!_unitPool[attackerOid].IsActive || !_unitPool[targetOid].IsActive) return false;
        // IsActive 가 False일 수 있는 상황 1. 소환 전, 2. 사망 로직에서? : TODO - 사망 로직 짤때 고려해서 짜기
        Unit attacker = _unitPool[attackerOid];
        Unit target = _unitPool[targetOid];

        damage = attacker.AttackPower;

        target.CurrentHP -= damage;
        target.CurrentHP = Math.Max(0, target.CurrentHP); // 음수 방지

        return target.CurrentHP <= 0;
    }

    public bool IsDead(int targetOid)
    {
        if (targetOid < 0 || targetOid >= _unitPool.Count)
            return true;

        return _unitPool[targetOid].CurrentHP <= 0;
    }


}
