using ServerCore;
using static System.Net.WebRequestMethods;

class InternalPacketHandler
{
    static string LobbyURL = "https://leeyoungwoo.shop";
    public static async void S_M_CreateRoomHandler(PacketSession session, IPacket packet)
    {
        S_M_CreateRoom smPacekt = packet as S_M_CreateRoom;

        RoomMapping.Instance.AddRoomInfo(smPacekt);

        Console.WriteLine($"{smPacekt.roomId} with player1 [{smPacekt.player1}] player2 [{smPacekt.player2}]");



        // Lobby로 매칭 성공 통보
        await LobbyApiSender.SendMatchCompleteAsync(
            LobbyURL,
            new MatchCompleteRequest
            {
                player1 = smPacekt.player1,//new List<string> { smPacekt.player1, smPacekt.player2 }
                player2 = smPacekt.player2,
                roomId = smPacekt.roomId,
            });

        //if (resultPacket.isDraw is true) Console.WriteLine($"[MatchServer] 게임 결과 수신: Room = {resultPacket.roomId}, 비김");
        //Console.WriteLine($"[MatchServer] 게임 결과 수신: Room = {resultPacket.roomId}, 승자 = {resultPacket.winnerId}, 패자");

/*        RoomMapping.Instance.Remove(smPacekt.roomId); // TODO : 정리

        // Lobby로 게임 결과 전송
        await LobbyApiSender.SendMatchResultAsync(
            LobbyURL,
            new MatchResultRequest
            {
                isDraw = true,
                winnerId = smPacekt.player2,
            });*/

    }


    public static async void S_M_GameResultHandler(PacketSession session, IPacket packet)
    {
        S_M_GameResult resultPacket = packet as S_M_GameResult;
        if(resultPacket.isDraw is true) Console.WriteLine($"[MatchServer] 게임 결과 수신: Room = {resultPacket.roomId}, 비김");
        Console.WriteLine($"[MatchServer] 게임 결과 수신: Room = {resultPacket.roomId}, 승자 = {resultPacket.winnerId}, 패자");

        RoomMapping.Instance.Remove(resultPacket.roomId); // TODO : 정리

        // Lobby로 게임 결과 전송
        await LobbyApiSender.SendMatchResultAsync(
            LobbyURL,
            new MatchResultRequest
            {
                isDraw = resultPacket.isDraw,
                winnerId = resultPacket.winnerId,
            });
    }


}