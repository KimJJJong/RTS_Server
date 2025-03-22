using DummyClient;
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


    }
    public static void S_ReadyHandler(PacketSession session, IPacket packet)
    {

    }
    public static void S_StartGameHandler(PacketSession session, IPacket packet)
    {
        Console.WriteLine("Game Start");
    }
    public static void S_SceneLoadHandler(PacketSession session, IPacket packet)
    {

    }
    public static void S_InitGameHandler(PacketSession session, IPacket packet)
    {

    }
    public static void S_GameUpdateHandler(PacketSession session, IPacket packet)
    {

    }
    public static void S_GameOverHandler(PacketSession session, IPacket packet)
    {

    }
    public static void S_AnsSummonHandler(PacketSession session, IPacket packet)
    {
        S_AnsSummon ansPacket = packet as S_AnsSummon;
        //ServerSession serverSession = session as ServerSession;

        Console.WriteLine($"uid : {ansPacket.oid}\nx : {ansPacket.x} y : {ansPacket.y}");

    }
    public static void S_GameStateUpdateHandler(PacketSession session, IPacket packet)
    {
        
    }
    public static void S_ManaUpdateHandler(PacketSession session, IPacket packet)
    {

    }
    public static void S_SyncTimeHandler (PacketSession session, IPacket packet)
    {

    }
    public static void S_UnitActionHandler(PacketSession session, IPacket packet)
    {

    }
}
