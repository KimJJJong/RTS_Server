using ServerCore;
using System;
using System.Net;

namespace Server
{
    class ClientSession : PacketSession
    {
        public int SessionID { get; set; }
        public GameRoom Room { get; set; }
        public Lobby Lobby { get; set; }
        public override void OnConnected(EndPoint endPoint)
        {
            // tmp : Check PlayerNum
            Console.WriteLine($"OnConnected : {endPoint}\n Player {SessionID} In");

            // TODO : Client 요청에 따른 Enter 관리
            //Program.Room.Enter(this); 직접 처리 하지 않고 JobQueue : Push
            Program.Lobby.Push(() => Program.Lobby.Enter(this));

            Program.Room.Push(() => Program.Room.Enter(this));

        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            SessionManager.Instance.Remove(this);


            if (Room != null)
            {
                // JobQueue를 이용시 명령어 처리가 순차적으로 미뤄지는 상황에 따라
                // Room.Leave(this) -> GameRoom room = Room; : 상태 저장 후 명령어 요청
                GameRoom room = Room;
                room.Push(() => room.Leave(this));
                Room = null;

            }

            if (Lobby != null)
            {
                Lobby lobby = Lobby;
                lobby.Push(() => lobby.Leave(this));
                Lobby = null;
            }

            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        public override void OnSend(int numOfBytes)
        {
            // Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }
    }
}
