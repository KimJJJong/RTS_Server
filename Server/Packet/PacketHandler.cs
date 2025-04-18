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
      
        C_ReqSummon c_ReqSummon = packet as C_ReqSummon;
        ClientSession clientSession = session as ClientSession ;
        GameRoom room = clientSession.Room;  // ToDo : 

        Console.WriteLine($"씨발 {room.GameLogic.UnitPool[c_ReqSummon.oid].IsActive}");

        // 유효성 검사
        if (c_ReqSummon.oid < 0 || c_ReqSummon.oid >= room.GameLogic.UnitPool.Count)
        {
            Console.WriteLine($"[Summon] 잘못된 oid 요청: {c_ReqSummon.oid}");
            return;
        }
        // 사용 중이면 같은 카드 그룹 내 빈 OID 탐색
        if (room.GameLogic.UnitPool[c_ReqSummon.oid].IsActive)
        {
            int? available = room.GameLogic.GetAvailableOid(c_ReqSummon.oid);
            if (available == null)
            {
                Console.WriteLine($"[Summon] 사용 가능한 유닛 없음 - 카드 OID 기준 {c_ReqSummon.oid}");
                return;
            }

            c_ReqSummon.oid = available.Value;
        }


        if (!room.GameLogic.Manas[c_ReqSummon.reqSessionID].UseMana(c_ReqSummon.needMana))
        {
            Console.WriteLine($"[Summon] 실패: 마나 부족");
            return;
        }

        //room.GameLogic.UnitPool[c_ReqSummon.oid].SetActive( true );
        room.GameLogic.OnReceiveSummon(clientSession, c_ReqSummon);


    }
    public static void C_TargetCaptureHandler(PacketSession session, IPacket packet)
    {

    }
    public static void C_AttackRequestHandler(PacketSession session, IPacket packet)
    {
        C_AttackRequest req = packet as C_AttackRequest;
        ClientSession client = session as ClientSession;
        GameRoom room = client.Room;

        if (room == null || room.GameLogic == null)
            return;

        room.GameLogic.OnReciveAttack(client, req);
    }
    public static void C_RequestManaStatusHandler(PacketSession session, IPacket packet)
    {

    }
    #endregion
}
