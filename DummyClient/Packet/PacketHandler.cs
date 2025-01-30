using DummyClient;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DummyClient
{
    class PacketHandler
    {
        public static void S_ChatHandler(PacketSession session, IPacket packet)
        {
            S_Chat testPacket = packet as S_Chat;
            Console.WriteLine($"ID :  [{testPacket.playerId}]    Chat : [{testPacket.chat}]");
            


        }

        public static void S_TestHandler(PacketSession session, IPacket packet)
        {

        }
        public static void S_LoginHandler(PacketSession session, IPacket packet)
        {



        }
    }
}