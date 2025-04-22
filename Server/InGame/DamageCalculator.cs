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



    public bool ApplyDamageAndCheckDeath(int attackerOid, int targetOid, out float curHp)
    {

        Unit attacker = _unitPool[attackerOid];
        Unit target = _unitPool[targetOid];

        //        curHp = target.CurrentHP;

        float damage = attacker.AttackPower;
        Console.WriteLine($"damage : [{damage}]");
        target.CurrentHP -= damage;
        target.CurrentHP = Math.Max(0, target.CurrentHP); // 음수 방지

        curHp = target.CurrentHP;
        Console.WriteLine($"curHp : [{curHp}]");

        return target.CurrentHP <= 0;
    }

    public bool ApplyDirectDamage(int targetOid, float damage, out float newHp)
    {
        if (targetOid < 0 || targetOid >= _unitPool.Count)
        {
            newHp = 0;
            return true;
        }

        Unit target = _unitPool[targetOid];
        if (!target.IsActive)
        {
            newHp = target.CurrentHP;
            return false;
        }

        target.CurrentHP -= damage;
        target.CurrentHP = Math.Max(0, target.CurrentHP);

        newHp = target.CurrentHP;
        return newHp <= 0;
    }



    public bool IsDead(int targetOid)
    {
        if (targetOid < 0 || targetOid >= _unitPool.Count)
            return true;

        return _unitPool[targetOid].CurrentHP <= 0;
    }


}
