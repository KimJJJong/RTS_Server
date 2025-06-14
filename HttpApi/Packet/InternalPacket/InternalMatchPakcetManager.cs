using ServerCore;


public class InternalMatchPakcetManager
{
    #region Singleton
    static InternalMatchPakcetManager _instance = new InternalMatchPakcetManager();
    public static InternalMatchPakcetManager Instance { get { return _instance; } }
    #endregion

    InternalMatchPakcetManager()
    {
        Register();
    }


    private Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>> _makeFunc = new();
    private Dictionary<ushort, Action<PacketSession, IPacket>> _handler = new();

    public void Register()
    {
        _makeFunc.Add((ushort)InternalPacketID.S_M_CreateRoom, MakePacket<S_M_CreateRoom>);
        _handler.Add((ushort)InternalPacketID.S_M_CreateRoom, InternalPacketHandler.S_M_CreateRoomHandler);
        _makeFunc.Add((ushort)InternalPacketID.S_M_GameResult, MakePacket<S_M_GameResult>);
        _handler.Add((ushort)InternalPacketID.S_M_GameResult, InternalPacketHandler.S_M_GameResultHandler);
    }


    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer, Action<PacketSession, IPacket> onRecvCallback = null)
    {
        ushort count = 0;

        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        Func<PacketSession, ArraySegment<byte>, IPacket> func = null;
        if (_makeFunc.TryGetValue(id, out func))
        {
            IPacket packet = func.Invoke(session, buffer);
            if (onRecvCallback != null)
                onRecvCallback.Invoke(session, packet);
            else
                HandlePacket(session, packet);
        }
    }

    T MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
    {
        T pkt = new T();
        pkt.Read(buffer);
        return pkt;
    }

    public void HandlePacket(PacketSession session, IPacket packet)
    {
        Action<PacketSession, IPacket> action = null;
        if (_handler.TryGetValue(packet.Protocol, out action))
            action.Invoke(session, packet);

    }



}
