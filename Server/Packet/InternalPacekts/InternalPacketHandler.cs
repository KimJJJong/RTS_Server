using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using ServerCore;

class InternalPacketHandler
{

    public static void M_S_CreateRoomHandler(PacketSession session, IPacket packet)
    {
        MatchSession matchSession = session as MatchSession;
        M_S_CreateRoom mPacket = packet as M_S_CreateRoom;

        Console.WriteLine($"{mPacket.player1}and {mPacket.player2} Requset CreateRoom");
        List<string> playerList = new List<string>();
        playerList.Add( mPacket.player1 );
        playerList.Add( mPacket.player2 );
        string roomId = GameRoomManager.Instance.CreateRoom(playerList);


        S_M_CreateRoom ansPacket = new S_M_CreateRoom();
        ansPacket.roomId = roomId;
        matchSession.Send(ansPacket.Write());
    }

/*    <packet name = "C_ReqJoinGameServer" >

        < int name="playerUid"/>
		<string name = "roomId" />

        < list name="playerOwnCards">
			<int name = "lv" />

            < string name="uid"/>
		</list>
	</packet>*/
}