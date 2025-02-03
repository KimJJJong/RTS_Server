using Server;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

class PacketHandler
{
    #region Lobby BroadCast
    public static void C_LoginHandler(PacketSession session, IPacket packet)
    {

    }
    public static void C_EnterLobbyHandler(PacketSession session, IPacket packet)
    {

    }
    public static void C_CreateRoomHandler(PacketSession session, IPacket packet)
    {

    }
    public static void C_JoinRoomHandler(PacketSession session, IPacket packet)
    {

    }
    #endregion

    #region Room BroadCast
    public static void C_ReadyHandler(PacketSession session, IPacket packet)
    {

    }
    public static void C_GameActionHandler(PacketSession session, IPacket packet)
    {

    }
    public static void C_ReqSummonHandler(PacketSession session, IPacket packet)
    {
        C_ReqSummon sumPacket = packet as C_ReqSummon;
        ClientSession cliSsession = session as ClientSession;
        
        Console.WriteLine($"uid : {sumPacket.uid}\nx : {sumPacket.x} y : {sumPacket.y}");
        
        // TODO : Mana 확인 필요
        S_AnsSummon ansPacket = new S_AnsSummon();
        ansPacket.x = sumPacket.x;
        ansPacket.y = sumPacket.y;
        ansPacket.uid = sumPacket.uid;
        ansPacket.reqSessionID = cliSsession.SessionID;

        GameRoom room = cliSsession.Room;
        //room.Push(() => room.BroadCast(ansPacket.Write()));
        room.BroadCast(ansPacket.Write());
    }
    public static void C_RequestManaStatusHandler(PacketSession session, IPacket packet)
    {

    }
    #endregion
}
