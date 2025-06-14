using Microsoft.Extensions.Hosting;

public class MatchMakerService : BackgroundService
{
    private readonly MatchQueue _queue;
    private readonly GameServerConnector _connector;
    private readonly RoomMapping _roomMapping;

    public MatchMakerService(MatchQueue queue, GameServerConnector connector, RoomMapping mapping)
    {
        _queue = queue;
        _connector = connector;
        _roomMapping = mapping;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _connector.Start();

        while (!stoppingToken.IsCancellationRequested)
        {
            if (_queue.Count >= 2)
            {
                _queue.TryDequeue(out var userA);
                _queue.TryDequeue(out var userB);

                _connector.SendCreateRoom(userA, userB);
                // Room 생성 결과는 GameServer에서 응답받아 RoomMapping 기록
            }

            await Task.Delay(100);
        }
    }
}
