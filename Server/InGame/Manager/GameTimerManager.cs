class GameTimerManager
{
    private Timer _timer;
    private TickManager _tickManager;

    public GameTimerManager(TickManager tickManager)
    {
        _tickManager = tickManager;
    }

    public void Init()
    {
        _timer = new Timer();
    }

    public void Update()
    {
        // 필요시 추가 동작
    }

    public S_InitGame MakeInitPacket()
    {
        return new S_InitGame
        {
            gameStartTime = _timer.GameStartTime,
            duration = _timer.GameDuration
        };
    }

    public void Clear()
    {
        _timer = null;
        _tickManager = null;
    }
}