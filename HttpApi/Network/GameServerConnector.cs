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

    public void SendCreateRoom(MatchPlayerInfo user1, MatchPlayerInfo user2)
    {
        M_S_CreateRoom packet = new M_S_CreateRoom();
        packet.player1 = user1.UserId;
        packet.player2 = user2.UserId;

        // 두 유저의 덱을 합친다
        List<M_S_CreateRoom.CardCombination> combinedDeck = new();

        foreach (var card in user1.Deck)
        {
            combinedDeck.Add(new M_S_CreateRoom.CardCombination
            {
                lv = card.Lv,
                uid = card.Uid
            });
        }

        foreach (var card in user2.Deck)
        {
            combinedDeck.Add(new M_S_CreateRoom.CardCombination
            {
                lv = card.Lv,
                uid = card.Uid
            });
        }

        packet.cardCombinations = combinedDeck;

        Console.WriteLine($"{user1.UserId} and {user2.UserId} req CreateRoom");
        _session.Send(packet.Write());
    }
}
