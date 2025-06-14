using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using ServerCore;

class InternalPacketHandler
{

    public static void M_S_CreateRoomHandler(PacketSession session, IPacket packet)
    {
        MatchSession matchSession = session as MatchSession;
        M_S_CreateRoom mPacket = packet as M_S_CreateRoom;

        Console.WriteLine($"{mPacket.player1} and {mPacket.player2} Request CreateRoom");

        // 플레이어 ID 리스트 구성
        List<string> playerList = new List<string> { mPacket.player1, mPacket.player2 };

        // 카드 조합 리스트 파싱
        List<Card> deckCombination = new();


        foreach (var card in mPacket.cardCombinations)
        {
            deckCombination.Add(new Card( card.uid, card.lv ) );

            Console.WriteLine($"Card [ UID :{card.uid}] [ LV : {card.lv} ]");
        }

        // GameRoom 생성 (RoomId 생성 후 덱까지 넘김)
        string roomId = GameRoomManager.Instance.CreateRoom(playerList, deckCombination);

        // GameServer -> MatchServer 응답
        S_M_CreateRoom ansPacket = new S_M_CreateRoom
        {
            roomId = roomId,
            player1 = mPacket.player1,
            player2 = mPacket.player2
        };

        matchSession.Send(ansPacket.Write());
    }
}


