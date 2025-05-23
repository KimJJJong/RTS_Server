using Server;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

class PacketHandler
{
    #region Lobby BroadCast
   

    //public static void C_LoginHandler(PacketSession session, IPacket packet)
    //{
    //    // ToDo : 이후 처리는 Api 서버로 넘기고 지금은 연결만 설정 해 줍시다
    //    C_Login logPacket = packet as C_Login;
    //    ClientSession clientSession = session as ClientSession;


    //    Console.WriteLine($"===============\n" +
    //                      $"SessionID : {clientSession.SessionID}\n" +
    //                     // $"UserName  : {logPacket.username}\n" +
    //                     // $"PassWord  : {logPacket.password}\n" +
    //                      $"===============");

    //    Program.Lobby.Push(() => Program.Lobby.Enter(clientSession));

    //    S_Login sLogPacket = new S_Login();
    //    sLogPacket.success = true;
    //    sLogPacket.message = $"{clientSession.SessionID}";

    //    Program.Lobby.Push(() => clientSession.Send(sLogPacket.Write()));

    //}
    public static void C_ReqJoinGameServer(PacketSession session, IPacket packet)
    {
        C_ReqJoinGameServer reqPacket = packet as C_ReqJoinGameServer;
        ClientSession clientSession = session as ClientSession;

        int playerId = reqPacket.playerId;
        GameRoom gameRoom = GameRoomManager.Instance.FindRoom(reqPacket.roomId);
        
        if (gameRoom is null) return;

        // 어떻게 하면 더 좋은 구조일까..? ClientDeck을 관리 방법의 문제
        foreach (var cardData in reqPacket.cardCombinations)
            clientSession.OwnDeck.Add(new Card(cardData.uid, cardData.lv));

        if ( gameRoom.AddClient(playerId, clientSession) is false) return;




    }

    //public static void C_MatchRequestHandler(PacketSession session, IPacket packet)
    //{
    //    ClientSession clientSession = session as ClientSession;
    //    if (clientSession == null || clientSession.isMatching)
    //        return;
    //    clientSession.isMatching = true;

    //    Console.WriteLine($"Client :{ clientSession.SessionID } Mtaching...");

    //    Program.Lobby.Push(() => clientSession.Lobby.EnterMatchQueue(clientSession));
    //}
    //public static void C_MatchCancelHandler(PacketSession session, IPacket packet)
    //{
    //    ClientSession clientSession = session as ClientSession;

    //    if (clientSession == null)
    //        return;

    //    Program.Lobby.Push(() => Program.Lobby.LeaveMatchQueue(clientSession));
    //}
    //public static void C_CreateRoomHandler(PacketSession session, IPacket packet)
    //{
    //    ClientSession clientSession = session as ClientSession;

    //    S_CreateRoom s_CreateRoom = new S_CreateRoom();
    //    s_CreateRoom.success = true;

    //    Program.Lobby.Push(() => s_CreateRoom.roomId = clientSession.Lobby.CreateRoom());
    //    Console.WriteLine($"Client [{clientSession.SessionID}] Create Room [{s_CreateRoom.roomId}]");

    //    Program.Lobby.Push(() => clientSession.Lobby.FindRoom(s_CreateRoom.roomId).Enter(clientSession));
    //    Program.Lobby.Push(() => clientSession.Send(s_CreateRoom.Write()));


    //}
    //public static void C_JoinRoomHandler(PacketSession session, IPacket packet)
    //{
    //    C_JoinRoom c_JoinRoom = packet as C_JoinRoom;
    //    ClientSession clientSession = session as ClientSession;
    //    Program.Lobby.Push(() => clientSession.Lobby.FindRoom(c_JoinRoom.roomId)?.Enter(clientSession));

    //}
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
            if (gameRoom.Players.Count == 2)
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
        var loadPacket = packet as C_SceneLoaded;
        var client = session as ClientSession;
        var room = client.Room;

        client.isLoad = loadPacket.isLoad;

        if (room.Sessions.Count == 2)
        {
            foreach (ClientSession s in room.Sessions.Values)
            {
                if (!s.isLoad)
                    return;
            }

            var response = new S_SceneLoad
            {
                ServerSendTime = DateTime.UtcNow.Ticks * 1e-7,
                StartTime = 180 + 2d
            };

            room.BroadCast(response.Write());
        }
        else
        {
            Console.WriteLine($"[SceneLoad] Invalid player count: {room.Sessions.Count}");
        }
    }

    public static void C_GameActionHandler(PacketSession session, IPacket packet) { }

    public static void C_ReqSummonHandler(PacketSession session, IPacket packet)
    {
        var req = packet as C_ReqSummon;
        var client = session as ClientSession;
        var logic = client.Room?.GameLogicManager;

        if (logic == null)
        {
            Console.WriteLine("[SummonHandler] ❌ GameLogic is null");
            return;
        }

        try
        {
           // Console.WriteLine($"[SummonHandler] Session:{client.SessionID}, OID:{req.oid}, Mana:{req.needMana}");
            logic.OnReceiveSummon(client, req);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SummonHandler] ❌ 예외 발생: {ex.Message}");
        }
    }

    public static void C_TargetCaptureHandler(PacketSession session, IPacket packet) { }



    public static void C_SummonProJectileHandler(PacketSession session, IPacket packet)
    {
        var req = packet as C_SummonProJectile;
        var client = session as ClientSession;
        var logic = client.Room.GameLogicManager;

        if (logic == null)
        {
            Console.WriteLine("[ProjectileHandler] ❌ GameLogic is null");
            return;
        }

        try
        {
           // Console.WriteLine($"[ProjectileHandler] ProjectileOid:{req.projectileOid}, Tick:{req.clientRequestTick}");
            logic.OnReciveSummonProject(client, req);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ProjectileHandler] ❌ 예외 발생: {ex.Message}");
        }
    }
    public static void C_AttackedRequestHandler(PacketSession session, IPacket packet)
    {
        var req = packet as C_AttackedRequest;
        var client = session as ClientSession;
        var logic = client.Room?.GameLogicManager;

        if (logic == null)
        {
            Console.WriteLine("[AttackHandler] ❌ GameLogic is null");
            return;
        }

        try
        {
            //Console.WriteLine($"[AttackHandler] 요청: {req.attackerOid} -> {req.targetOid}, Tick: {req.clientAttackedTick}");
            logic.OnReciveAttack(client, req);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AttackHandler] ❌ 예외 발생: {ex.Message}");
        }
    }
    public static void C_TileClaimReqHandler(PacketSession session, IPacket packet)
    {
        ClientSession client = session as ClientSession;
        GameRoom room = client.Room;

        if (room == null || room.GameLogicManager == null)
        {
            Console.WriteLine("[TileClaim] ❌ Room or GameLogic is null.");
            return;
        }

        C_TileClaimReq req = packet as C_TileClaimReq;
        room.GameLogicManager.OnReceiveTileClaim(client, req);
    }

    public static void C_RequestManaStatusHandler(PacketSession session, IPacket packet) { }

    public static void C_GoToLobbyHandler(PacketSession session, IPacket packet)
    {
//        client.Room?.Leave(client);

    
            ClientSession client = session as ClientSession;
            GameRoom room = client.Room;

            if (room == null)
            {
                Console.WriteLine($"[Error] Client {client.SessionID} is not in a room.");
                return;
            }

            room.Leave(client);
        


    }
    #endregion
}
