using ServerCore;
using System;
using System.Net;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Net.Sockets;

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
            //SessionManager.Instance.Remove(this);


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


            //CleanUp(); // 새로운 정리 메서드 호출
            SessionManager.Instance.Remove(this);
            //ClientSessionPool.Instance.ReturnSession(this); //Pool로 반환

            
            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        public override void OnSend(int numOfBytes)
        {
            // Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }


        public void Reset()
        {
            isReady = false;
            isLoad = false;
            isMatching = false;
            Room = null;
            Lobby = null;
        }

        /// <summary>
        /// Used Memory CleanUp
        /// </summary>
        public void CleanUp()
        {
            try
            {
                // 네트워크 소켓 해제
                Socket socket = GetSocket();
                if (socket != null)
                {
                    Console.WriteLine("CleanUp");
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Cleanup] Socket Error: {ex.Message}");
            }

          
        }

        private Socket GetSocket()
        {
            // PacketSession에 _socket이 private이라면, 이를 상속받아 가져오거나, 네트워크 정리 로직을 내부에 추가할 필요가 있음.
            return null;
        }


    }
}
