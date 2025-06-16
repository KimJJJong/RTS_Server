using Server;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

class PacketHandler
{
    #region Lobby BroadCast
   


    public static void C_ReqJoinGameServerHandler(PacketSession session, IPacket packet)
    {
     
        C_ReqJoinGameServer reqPacket = packet as C_ReqJoinGameServer;
        ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"player : {reqPacket.playerSUid}is ReqJoinGameServer num {reqPacket.roomId}");

        S_ConfirmJoinGameServer confirmPacket = new S_ConfirmJoinGameServer();


        GameRoom gameRoom = GameRoomManager.Instance.FindRoom(reqPacket.roomId);
        bool canAddClient = gameRoom.AddClient(reqPacket.playerSUid, clientSession);
        if ( gameRoom is null || 
             canAddClient is false )
        {
            confirmPacket.confirm = false;
            Console.WriteLine($"[{reqPacket.roomId}]는 없습니다.");
        }
        else
        {
            confirmPacket.confirm = true;
    
        }
       
        clientSession.Send(confirmPacket.Write());
       



    }

    public static void C_SceneLoadedHandler(PacketSession session, IPacket packet)
    {

    }


    #endregion

    #region Room BroadCast


    public static void C_ReqSummonHandler(PacketSession session, IPacket packet)
    {
        var req = packet as C_ReqSummon;
        var client = session as ClientSession;
        var logic = client.Room?.GameLogic;

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
        var logic = client.Room.GameLogic;

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
        var logic = client.Room?.GameLogic;

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

        if (room == null || room.GameLogic == null)
        {
            Console.WriteLine("[TileClaim] ❌ Room or GameLogic is null.");
            return;
        }

        C_TileClaimReq req = packet as C_TileClaimReq;
        room.GameLogic.OnReceiveTileClaim(client, req);
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

            room.RemoveClient(client.SessionID);
        


    }
    #endregion
}
