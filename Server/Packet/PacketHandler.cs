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
        // 근데 login은 내가 처리 안하는거 아닌감..;
        C_Login logPacket = packet as C_Login;
        ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"===============\n" +
                          $"SessionID : {clientSession.SessionID}\n" +
                          $"UserName  : {logPacket.username}\n" +
                          $"PassWord  : {logPacket.password}\n" +
                          $"===============");

        Program.Lobby.Push(() => Program.Lobby.Enter(clientSession));

        S_Login sLogPacket = new S_Login();
        sLogPacket.success = true;
        sLogPacket.message = "TestLogin";

        Program.Lobby.Push(() => clientSession.Send(sLogPacket.Write()));

    }
    public static void C_EnterLobbyHandler(PacketSession session, IPacket packet)
    {

    }
    public static void C_CreateRoomHandler(PacketSession session, IPacket packet)
    {
        ClientSession clientSession = session as ClientSession;
        
        S_CreateRoom s_CreateRoom = new S_CreateRoom();
        s_CreateRoom.success = true;
        
        Program.Lobby.Push(() => s_CreateRoom.roomId = clientSession.Lobby.CreateRoom());
        // s_CreateRoom.roomId = clientSession.Lobby.CreateRoom(); // MainThread에서 처리 하지 않아도 괜찮은가?
        Program.Lobby.Push(() => clientSession.Lobby.FindRoom(s_CreateRoom.roomId).Enter(clientSession));
        Program.Lobby.Push(() => clientSession.Send(s_CreateRoom.Write()));

        Console.WriteLine($"Client [{clientSession.SessionID}] Create Room [{s_CreateRoom.roomId}]");

    }
    public static void C_JoinRoomHandler(PacketSession session, IPacket packet)
    {
        C_JoinRoom c_JoinRoom = packet as C_JoinRoom;
        ClientSession clientSession = session as ClientSession;

        clientSession.Lobby.Push(() => clientSession.Lobby.FindRoom(c_JoinRoom.roomId)?.Enter(clientSession));

    }
    #endregion

    #region Room BroadCast
    public static void C_ReadyHandler(PacketSession session, IPacket packet)
    {
        C_Ready c_Ready = packet as C_Ready;
        ClientSession clientSession =session as ClientSession;

        clientSession.isReady = true;

        foreach(ClientSession _session in clientSession.Room.Sessions.Values )
        {
            if(_session.isReady == false) return;
        }

        clientSession.Room.StartGame();
        
    }
    public static void C_GameActionHandler(PacketSession session, IPacket packet)
    {

    }
    public static void C_ReqSummonHandler(PacketSession session, IPacket packet)
    {
        C_ReqSummon sumPacket = packet as C_ReqSummon;
        ClientSession cliSsession = session as ClientSession;
        GameRoom room = cliSsession.Room;

        // Mana Check
        if( room.GameLogic.Manas[sumPacket.reqSessionID].UseMana(sumPacket.needMana))
        {
            S_AnsSummon ansPacket = new S_AnsSummon();
            ansPacket.x = sumPacket.x;
            ansPacket.y = sumPacket.y;
            ansPacket.uid = sumPacket.uid;  
            ansPacket.reqSessionID = cliSsession.SessionID;
            ansPacket.reducedMana = room.GameLogic.Manas[sumPacket.reqSessionID].GetMana();

            Console.WriteLine($"uid : {sumPacket.uid}\nx : {sumPacket.x} y : {sumPacket.y}");
            room.BroadCast(ansPacket.Write());
        }


        //room.Push(() => room.BroadCast(ansPacket.Write()));
    }
    public static void C_RequestManaStatusHandler(PacketSession session, IPacket packet)
    {

    }
    #endregion
}
