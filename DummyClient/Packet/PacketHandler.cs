﻿using DummyClient;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

    class PacketHandler
    {
    public static void S_LoginHandler(PacketSession session, IPacket packet)
	{

	}

    public static void S_EnterLobbyHandler(PacketSession session, IPacket packet)
    {

    }
    public static void S_CreateRoomHandler(PacketSession session, IPacket packet)
    {

    }
    public static void S_JoinRoomHandler(PacketSession session, IPacket packet)
    {
        //S_JoinRoom joinPacket= packet as S_JoinRoom;
        //Console.WriteLine($" {joinPacket.roomId} : {joinPacket.message}");
        Console.WriteLine("Game Start");

    }
    public static void S_ReadyHandler(PacketSession session, IPacket packet)
    {

    }
    public static void S_StartGameHandler(PacketSession session, IPacket packet)
    {
        Console.WriteLine("Game Start");
    }
    public static void S_GameUpdateHandler(PacketSession session, IPacket packet)
    {

    }
    public static void S_GameOverHandler(PacketSession session, IPacket packet)
    {

    }

}
