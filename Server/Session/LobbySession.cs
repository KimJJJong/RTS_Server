//using System;
//using System.Net;
//using ServerCore;

//class LobbySession : PacketSession
//{
//    public override void OnConnected(EndPoint endPoint)
//    {
//        Console.WriteLine($"[Lobby Server Connected] {endPoint}");
//    }

//    public override void OnRecvPacket(ArraySegment<byte> buffer)
//    {
//        throw new NotImplementedException();
//    }

//    public override void OnDisconnected(EndPoint endPoint)
//    {
//        throw new NotImplementedException();
//    }

//    public override void OnSend(int numOfBytes)
//    {
//        throw new NotImplementedException();    
//    }
//}