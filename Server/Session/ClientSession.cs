using ServerCore;
using System;
using System.Net;
using Shared;
using System.Collections.Generic;


public class ClientSession : PacketSession
{
    public bool isReady { get; set; }
    public bool isLoad {  get; set; }
    public bool isMatching {  get; set; }
    public string UserID { get; set; }
    public GameRoom Room { get; set; }
    public int PlayingID {  get; set; }
    public List<Card> OwnDeck {  get; set; }

    public override void OnConnected(EndPoint endPoint)
    {
        // tmp : Check PlayerNum
        // Console.WriteLine($"OnConnected : {SessionID} In");

        // TODO : Client 요청에 따른 Enter 관리
        //Program.Room.Enter(this); 직접 처리 하지 않고 JobQueue : Push
        //Program.Lobby.Push(() => Program.Lobby.Enter(this));

        //Program.Room.Push(() => Program.Room.Enter(this));
        //Program.Room.Enter(this);
        Console.WriteLine($"GameServer와 ClientSession 이 연결되었습니다: {endPoint}");

//    S_ReqSessionInit reqPacket = new S_ReqSessionInit();




    }

    public override void OnRecvPacket(ArraySegment<byte> buffer)
    {
        PacketManager.Instance.OnRecvPacket(this, buffer);
    }

    public override void OnDisconnected(EndPoint endPoint)
    {


        if (Room != null)
        {
            GameRoom room = Room;
            room.RemoveClient(PlayingID);
            Room = null;
        }


        SessionManager.Instance.Remove(this);
        LogManager.Instance.LogInfo("ClientSession", $"Disconnected: {endPoint}");

        //Console.WriteLine($"OnDisconnected : {endPoint}");
    }

    public override void OnSend(int numOfBytes)
    {
        // Console.WriteLine($"Transferred bytes: {numOfBytes}");
    }

      
}

