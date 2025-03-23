using ServerCore;
using System;
using System.Net;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Net.Sockets;
using Shared;

namespace Server
{
    class ClientSession : PacketSession
    {
        public bool isReady { get; set; }
        public bool isLoad {  get; set; }
        public bool isMatching {  get; set; }
        public int SessionID { get; set; }
        public GameRoom Room { get; set; }
        public Lobby Lobby { get; set; }
        public override void OnConnected(EndPoint endPoint)
        {
            // tmp : Check PlayerNum
            // Console.WriteLine($"OnConnected : {SessionID} In");

            // TODO : Client 요청에 따른 Enter 관리
            //Program.Room.Enter(this); 직접 처리 하지 않고 JobQueue : Push
            //Program.Lobby.Push(() => Program.Lobby.Enter(this));

            //Program.Room.Push(() => Program.Room.Enter(this));
            //Program.Room.Enter(this);
            //Console.WriteLine($"GameServer와 연결되었습니다: {endPoint}");



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
                room.Leave(this);
                Room = null;
            }

            if (Lobby != null)
            {
                Lobby lobby = Lobby;
                lobby.Push(() => lobby.Leave(this));
                Lobby = null;
            }


            SessionManager.Instance.Remove(this);
            LogManager.Instance.LogInfo("ClientSession", $"Disconnected: {endPoint}");

            //Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        public override void OnSend(int numOfBytes)
        {
            // Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }

        public void CleanUp()
        {
            Lobby?.Leave(this);
            Room?.Leave(this);

            Lobby = null ;
            Room = null ;
        }

        public void Reset()
        {
            FlagSet();  // Session을 건드림 _disconnect;
            isReady = false;
            isLoad = false;
            isMatching = false;
            Room = null;
            Lobby = null;
        }
      
    }
}
