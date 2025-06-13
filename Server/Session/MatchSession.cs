using ServerCore;
using System;
using System.Net;
using Shared;
using System.Collections.Generic;


public class MatchSession : PacketSession
{


    public override void OnConnected(EndPoint endPoint)
    {

        Console.WriteLine($"GameServer와 MatchServer 가 연결되었습니다: {endPoint}");


    }

    public override void OnRecvPacket(ArraySegment<byte> buffer)
    {
        InternalServerPakcetManager.Instance.OnRecvPacket(this, buffer);
    }

    public override void OnDisconnected(EndPoint endPoint)
    {

        SessionManager.Instance.Remove(this);
        LogManager.Instance.LogInfo("MatchServer", $"Disconnected: {endPoint}");
    }

    public override void OnSend(int numOfBytes)
    {

    }


}

