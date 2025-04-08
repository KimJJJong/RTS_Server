using System;

class Unit
{


    public string UnitID { get; private set; }
    public int OwnerID { get; private set; }
    public int CardLV {  get; private set; }
    public int PositionX { get; private set; }
    public int PositionY { get; private set; }
    public int HP { get; private set; } = 100;
    public  bool IsActive => _isActive;

    private bool _isActive = false;
    private int _speed = 1;
    private int _attackPower = 10;
    private int _attackRange = 1;

    public Unit(string unitID, int ownerID)
    {

        PositionX = -99;
        PositionY = -99;

        UnitID = unitID;
        OwnerID = ownerID;

    }

    public void Update()
    {
        //TODO : 일정 기간 업데이트 할 내용
    }

    private void Attack()
    {

    }
    public void SetActive( bool isActive)
    {
        _isActive = isActive;
    }
}
