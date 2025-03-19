using ServerCore;
using System;
using System.Net;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace Server
{
    class ClientSession : PacketSession
    {
        public string AccessToken { get; set; } // JWT 토큰 저장
        public bool IsAuthenticated { get; private set; } = false;

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
            // bool 연산은 미미하기에 크게 상관이 없지만 이후 최적화, 보안 강화를 위해 AuthSession을 만들어 관리하는것도 하나의 방법
      /*      if (!IsAuthenticated)
            {
                Console.WriteLine("인증되지 않은 사용자의 패킷 수신 시도, 연결을 차단합니다.");
                Disconnect();
                return;
            }*/


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
                //room.Push(() => room.Leave(this));
                room.Leave(this);
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
