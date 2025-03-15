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
        ClientSession clientSession = session as ClientSession;
        C_LoginAuth authPacket = packet as C_LoginAuth;

        Console.WriteLine($"JWT 토큰 검증 시도: {authPacket.accessToken}");

        if (clientSession.Authenticate(authPacket.accessToken))
        {
            Console.WriteLine("인증 성공! 세션을 활성화합니다.");
            SessionManager.Instance.Add(clientSession);

            Console.WriteLine($"===============\n" +
                  $"SessionID : {clientSession.SessionID}\n" +
                  $"===============");

            Program.Lobby.Push(() => Program.Lobby.Enter(clientSession));

            S_Login s_Login = new S_Login();
            s_Login.message = $"[{clientSession.SessionID}] Connect";
            Program.Lobby.Push(() => clientSession.Send(s_Login.Write()));
        }
        else
        {
            Console.WriteLine("인증 실패! 연결을 종료합니다.");
            clientSession.Disconnect();
        }
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
        // 일단 기본적으로 Lobby에 접속을 하기때문에 따로 handler가 필요 없다
    }

    public static void C_MatchRequestHandler(PacketSession session, IPacket packet)
    {
        ClientSession clientSession = session as ClientSession;
        if (clientSession == null)
            return;

        Program.Lobby.Push(() => clientSession.Lobby.EnterMatchQueue(clientSession));
       // Program.Lobby.EnterMatchQueue(clientSession);
    }
    public static void C_CreateRoomHandler(PacketSession session, IPacket packet)
    {
        ClientSession clientSession = session as ClientSession;
        
        S_CreateRoom s_CreateRoom = new S_CreateRoom();
        s_CreateRoom.success = true;
        
        Program.Lobby.Push(() => s_CreateRoom.roomId = clientSession.Lobby.CreateRoom());
        Console.WriteLine($"Client [{clientSession.SessionID}] Create Room [{s_CreateRoom.roomId}]");

        // s_CreateRoom.roomId = clientSession.Lobby.CreateRoom(); // MainThread에서 처리 하지 않아도 괜찮은가?
        Program.Lobby.Push(() => clientSession.Lobby.FindRoom(s_CreateRoom.roomId).Enter(clientSession));
        Program.Lobby.Push(() => clientSession.Send(s_CreateRoom.Write()));


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
        GameRoom gameRoom = clientSession.Room;
        clientSession.isReady = true;
        Console.WriteLine($"Client[{clientSession.SessionID}] Ready[{clientSession.isReady}]");

        if (gameRoom.Sessions.Count == 2)
        {
            foreach (ClientSession _session in gameRoom.Sessions.Values)
            {
                if (_session.isReady == false) return;
            }
            clientSession.Room.StartGame();
        }
        else
        {
            Console.WriteLine($"Not Enough Room Cound{gameRoom.Sessions.Count}");
            return;
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
        C_ReqSummon sumPacket = packet as C_ReqSummon;
        ClientSession cliSsession = session as ClientSession;
        GameRoom room = cliSsession.Room;

        // Mana Check
        if( room.GameLogic.Manas[sumPacket.reqSessionID].UseMana(sumPacket.needMana))
        {
            double summonTime = room.GameLogic.Timer.GetServerTime()+1d/*summonDelay*/;

            Console.WriteLine($" serverTime          : { summonTime-1d }\n" +
                              $" summonTime          : { summonTime}\n" +
                              $" summonSession       : { sumPacket.reqSessionID}");

            S_AnsSummon ansPacket = new S_AnsSummon();
            ansPacket.x = sumPacket.x;
            ansPacket.y = sumPacket.y;
            ansPacket.oid = sumPacket.oid;  
            ansPacket.reqSessionID = sumPacket.reqSessionID;
            ansPacket.reducedMana = room.GameLogic.Manas[sumPacket.reqSessionID].GetMana();
            ansPacket.summonTime = summonTime;
            ansPacket.ranValue = DateTime.UtcNow.Ticks * 1e-7;    // 이거 Packet구조 바꿔야 합니다 귀찮아서 재활용 한거라
            
            Console.WriteLine($"uid : {sumPacket.oid}\nx : {sumPacket.x} y : {sumPacket.y} sumTime : {summonTime:F6}");
            room.BroadCast(ansPacket.Write());
        }
        // deficient mana

    }
    public static void C_RequestManaStatusHandler(PacketSession session, IPacket packet)
    {

    }
    #endregion
}
