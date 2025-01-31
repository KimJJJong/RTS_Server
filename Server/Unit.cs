using System;

class Unit
{
    public int UnitID { get; private set; }
    public int OwnerID { get; private set; }
    public int Position { get; private set; }
    public int HP { get; private set; } = 100;
    private int _speed = 1;
    private int _attackPower = 10;
    private int _attackRange = 1;

    public Unit(int unitID, int ownerID, int startPosition)
    {
        UnitID = unitID;
        OwnerID = ownerID;
        Position = startPosition;
    }

    public void Update()
    {
        //TODO : 일정 기간 업데이트 할 내용
    }

    private void Attack()
    {

    }
}
