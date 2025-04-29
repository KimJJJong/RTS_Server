using Server;

class BattleManager
{
    private UnitPoolManager _unitPoolManager;
    private DamageCalculator _damageCalculator;

    public BattleManager(UnitPoolManager unitPoolManager)
    {
        _unitPoolManager = unitPoolManager;
    }

    public void ProcessSummon(ClientSession session, C_ReqSummon packet)
    {
        // Summon 로직 구현 필요
    }

    public void ProcessAttack(ClientSession session, C_AttackedRequest packet)
    {
        // Attack 로직 구현 필요
    }
}