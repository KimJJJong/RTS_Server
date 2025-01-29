using DummyClient;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DummyClient
{
    class PacketHandler
    {
        public static void S_TestHandler(PacketSession session, IPacket packet)
        {

        }
        public static void S_LoginHandler(PacketSession session, IPacket packet)
        {
            S_Login loginPacket = packet as S_Login;
            ServerSession serverSession = session as ServerSession;


        }
    }
}