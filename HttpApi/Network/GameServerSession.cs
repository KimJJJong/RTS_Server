using ServerCore;
using Shared;

using System.Net;

public class GameServerSession : PacketSession
{
    public override void OnConnected(EndPoint endPoint)
    {
        Console.WriteLine($"[MatchServer] GameServer Connected: {endPoint}");
    }

    public override void OnRecvPacket(ArraySegment<byte> buffer)
    {
        InternalMatchPakcetManager.Instance.OnRecvPacket(this, buffer);
    }

    public override void OnDisconnected(EndPoint endPoint)
    {

        LogManager.Instance.LogInfo("MatchServer", $"Disconnected: {endPoint}");
    }


    public override void OnSend(int numOfBytes)
    {
        // 필요시 송신 후처리 (거의 비워둬도 됨)
    }
}
