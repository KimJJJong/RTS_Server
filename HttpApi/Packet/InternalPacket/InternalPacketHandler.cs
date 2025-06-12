using ServerCore;

class InternalPacketHandler
{

    public static void S_M_CreateRoomHandler(PacketSession session, IPacket packet)
    {
        S_M_CreateRoom smPacekt = packet as S_M_CreateRoom;

        Console.WriteLine(smPacekt.roomId);
    }


}