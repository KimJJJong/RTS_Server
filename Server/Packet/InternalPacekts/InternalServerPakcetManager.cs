using ServerCore;
using System.Collections.Generic;
using System;

public class InternalServerPakcetManager
{
    #region Singleton
    static InternalServerPakcetManager _instance = new InternalServerPakcetManager();
    public static InternalServerPakcetManager Instance { get { return _instance; } }
    #endregion

    InternalServerPakcetManager()
    {
        Register();
    }

    private Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>> _makeFunc = new();
    private Dictionary<ushort, Action<PacketSession, IPacket>> _handler = new();

    public void Register()
    {
        _makeFunc.Add((ushort)InternalPacketID.M_S_CreateRoom, MakePacket<M_S_CreateRoom>);
        _handler.Add((ushort)InternalPacketID.M_S_CreateRoom, InternalPacketHandler.M_S_CreateRoomHandler);
 
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
