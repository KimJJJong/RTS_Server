using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using ServerCore;

namespace DummyClient
{

    class ServerSession : PacketSession
    {
        public string AccessToken { get; set; }

        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected [Cli]: {endPoint}");

            if (!string.IsNullOrEmpty(AccessToken))
            {
                Console.WriteLine("인증을 위해 C_LoginAuth 패킷을 전송합니다.");

                // C_LoginAuth 패킷을 생성하고 전송
                var authPacket = new C_LoginAuth { accessToken = AccessToken };
                Send(authPacket.Write());
            }
            else
            {
                Console.WriteLine("AccessToken이 존재하지 않습니다. 연결을 종료합니다.");
                Disconnect();
            }
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
          
            Console.WriteLine($"get : {buffer}");
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }




     
    }
}
