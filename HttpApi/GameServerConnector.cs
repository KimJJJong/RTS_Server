using ServerCore;
using System.Net;

public class GameServerConnector
{
    private GameServerSession _session;

    public void Start()
    {
        Connector connector = new Connector();
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("192.168.0.2"), 13222);
        connector.Connect(endPoint, () => _session = new GameServerSession());
    }

    public void SendCreateRoom(string userA, string userB)
    {
        M_S_CreateRoom packet = new M_S_CreateRoom();
        packet.player1 = userA;
        packet.player2 = userB;
        Console.WriteLine($"{userA} and {userB} Send?");
        _session.Send(packet.Write());

    }
}
