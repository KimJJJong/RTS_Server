using Server;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

class PacketHandler
{
    #region Lobby BroadCast
    public static void C_LoginAuthHandler(PacketSession session, IPacket packet)
    {

    }

    public static void C_LoginHandler(PacketSession session, IPacket packet)
    {
        // ToDo : 이후 처리는 Api 서버로 넘기고 지금은 연결만 설정 해 줍시다
        C_Login logPacket = packet as C_Login;
        ClientSession clientSession = session as ClientSession;


        Console.WriteLine($"===============\n" +
                          $"SessionID : {clientSession.SessionID}\n" +
                         // $"UserName  : {logPacket.username}\n" +
                         // $"PassWord  : {logPacket.password}\n" +
                          $"===============");

        Program.Lobby.Push(() => Program.Lobby.Enter(clientSession));

        S_Login sLogPacket = new S_Login();
        sLogPacket.success = true;
        sLogPacket.message = $"{clientSession.SessionID}";

        Program.Lobby.Push(() => clientSession.Send(sLogPacket.Write()));

    }
    public static void C_EnterLobbyHandler(PacketSession session, IPacket packet)
    {
    }

    public static void C_MatchRequestHandler(PacketSession session, IPacket packet)
    {
        ClientSession clientSession = session as ClientSession;
        if (clientSession == null || clientSession.isMatching)
            return;
        clientSession.isMatching = true;

        Console.WriteLine($"Client :{ clientSession.SessionID } Mtaching...");

        Program.Lobby.Push(() => clientSession.Lobby.EnterMatchQueue(clientSession));
    }
    public static void C_MatchCancelHandler(PacketSession session, IPacket packet)
    {
        ClientSession clientSession = session as ClientSession;
       
        if (clientSession == null)
            return;

        Program.Lobby.Push(() => Program.Lobby.LeaveMatchQueue(clientSession));
    }
    public static void C_CreateRoomHandler(PacketSession session, IPacket packet)
    {
        ClientSession clientSession = session as ClientSession;
        
        S_CreateRoom s_CreateRoom = new S_CreateRoom();
        s_CreateRoom.success = true;
        
        Program.Lobby.Push(() => s_CreateRoom.roomId = clientSession.Lobby.CreateRoom());
        Console.WriteLine($"Client [{clientSession.SessionID}] Create Room [{s_CreateRoom.roomId}]");

        Program.Lobby.Push(() => clientSession.Lobby.FindRoom(s_CreateRoom.roomId).Enter(clientSession));
        Program.Lobby.Push(() => clientSession.Send(s_CreateRoom.Write()));


    }
    public static void C_JoinRoomHandler(PacketSession session, IPacket packet)
    {
        C_JoinRoom c_JoinRoom = packet as C_JoinRoom;
        ClientSession clientSession = session as ClientSession;
        Program.Lobby.Push(() => clientSession.Lobby.FindRoom(c_JoinRoom.roomId)?.Enter(clientSession));

    }
    #endregion

    #region Room BroadCast
    public static void C_ReadyHandler(PacketSession session, IPacket packet)
    {
        C_Ready c_Ready = packet as C_Ready;
        ClientSession clientSession =session as ClientSession;
        GameRoom gameRoom = clientSession.Room;
        clientSession.isReady = true;
        Console.WriteLine($"Client[{clientSession.SessionID}] Ready[{clientSession.isReady}]");

        if (gameRoom.Sessions.Count == 2)
        {
            foreach (ClientSession _session in gameRoom.Sessions.Values)
            {
                if (_session.isReady == false) return;
            }
            clientSession.Room.ReadyStartGame();
        }
        else
        {
            Console.WriteLine($"Not Enough Room Cound{gameRoom.Sessions.Count}");
            return;
        }

        
    }
    public static void C_SetCardPoolHandler(PacketSession session, IPacket pacekt)
    {
        ClientSession clientSession = session as ClientSession;
        C_SetCardPool c_SetCardPool = pacekt as C_SetCardPool;

        GameRoom gameRoom = clientSession.Room;
        clientSession.isReady = true;

        try
        {
            if (gameRoom.Sessions.Count == 2)
            {
                Console.WriteLine("GetTwo");
                foreach (ClientSession _session in gameRoom.Sessions.Values)
                {
                    if (_session.isReady == false) return;
                }
                clientSession.Room.ReadyStartGame();
                foreach(var player in gameRoom.Sessions.Values)
                clientSession.Room.GameLogic.OnReceiveDeck(player, c_SetCardPool);
            }
            else
            {
                Console.WriteLine($"Not Enough Room Cound{gameRoom.Sessions.Count}");
                return;
            }


        }catch (Exception e)
        {
            Console.WriteLine(e.ToString()); return;
        }


    }
    public static void C_SceneLoadedHandler(PacketSession session, IPacket packet)
    {

        C_SceneLoaded loadpacket = packet as C_SceneLoaded;
        ClientSession clientSession = session as ClientSession;
        GameRoom gameRoom = clientSession.Room;

        clientSession.isLoad = loadpacket.isLoad;

        if (gameRoom.Sessions.Count == 2)
        {
            foreach (ClientSession _session in gameRoom.Sessions.Values)
            {
                if (_session.isLoad == false) return;
            }
            S_SceneLoad sPakcet = new S_SceneLoad();
            sPakcet.ServerSendTime = DateTime.UtcNow.Ticks * 1e-7;
            sPakcet.StartTime = gameRoom.GameLogic.Timer.GetServerTime() + 2d; /*after Load Delay*/

            gameRoom.BroadCast(sPakcet.Write());
        }
        else
        {
            Console.WriteLine($"err : RoomCound[{gameRoom.Sessions.Count}]");
            return;
        }

    }
    public static void C_GameActionHandler(PacketSession session, IPacket packet)
    {

    }
    public static void C_ReqSummonHandler(PacketSession session, IPacket packet)
    {
        //C_ReqSummon sumPacket = packet as C_ReqSummon;
        //ClientSession cliSsession = session as ClientSession;
        //GameRoom room = cliSsession.Room;

        //// Mana Check
        //if( room.GameLogic.Manas[sumPacket.reqSessionID].UseMana(sumPacket.needMana))
        //{
        //    double summonTime = room.GameLogic.Timer.GetServerTime()+1d/*summonDelay*/;

        //    Console.WriteLine($" serverTime          : { summonTime-1d }\n" +
        //                      $" summonTime          : { summonTime}\n" +
        //                      $" summonSession       : { sumPacket.reqSessionID}");

        //    S_AnsSummon ansPacket = new S_AnsSummon();

        //    ansPacket.x = sumPacket.x;
        //    ansPacket.y = sumPacket.y;
        //    ansPacket.oid = sumPacket.oid;  
        //    ansPacket.reqSessionID = sumPacket.reqSessionID;
        //    ansPacket.reducedMana = room.GameLogic.Manas[sumPacket.reqSessionID].GetMana();
        //    ansPacket.ServersummonTime = summonTime;
        //    ansPacket.ServerSendTime = DateTime.UtcNow.Ticks * 1e-7;    
        //    ansPacket.ranValue = new Random().Next(0, 10);
        //    Console.WriteLine($"uid : {sumPacket.oid}\nx : {sumPacket.x} y : {sumPacket.y} sumTime : {summonTime:F6}");
        //    room.BroadCast(ansPacket.Write());
        //}
        //// deficient mana
        ///
        C_ReqSummon c_ReqSummon = packet as C_ReqSummon;
        ClientSession clientSession = session as ClientSession ;
        GameRoom room = clientSession.Room;  // ToDo : 

        if ( room.GameLogic.Manas[c_ReqSummon.reqSessionID].UseMana(c_ReqSummon.needMana))
            room.GameLogic.OnReceiveSummon(clientSession, c_ReqSummon);

    }
    public static void C_RequestManaStatusHandler(PacketSession session, IPacket packet)
    {

    }
    #endregion
}
