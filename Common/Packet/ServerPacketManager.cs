using ServerCore;
using System;
using System.Collections.Generic;

public class PacketManager
{
	#region Singleton
	static PacketManager _instance = new PacketManager();
	public static PacketManager Instance { get { return _instance; } 	}
	#endregion

	PacketManager()
	{
		Register();
	}

	Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>> _makeFunc = new Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>>();
	Dictionary<ushort, Action<PacketSession, IPacket>> _handler = new Dictionary<ushort, Action<PacketSession, IPacket>>();
		
	public void Register()
	{
		_makeFunc.Add((ushort)PacketID.C_Login, MakePacket<C_Login>);
		_handler.Add((ushort)PacketID.C_Login, PacketHandler.C_LoginHandler);
		_makeFunc.Add((ushort)PacketID.C_EnterLobby, MakePacket<C_EnterLobby>);
		_handler.Add((ushort)PacketID.C_EnterLobby, PacketHandler.C_EnterLobbyHandler);
		_makeFunc.Add((ushort)PacketID.C_CreateRoom, MakePacket<C_CreateRoom>);
		_handler.Add((ushort)PacketID.C_CreateRoom, PacketHandler.C_CreateRoomHandler);
		_makeFunc.Add((ushort)PacketID.C_JoinRoom, MakePacket<C_JoinRoom>);
		_handler.Add((ushort)PacketID.C_JoinRoom, PacketHandler.C_JoinRoomHandler);
		_makeFunc.Add((ushort)PacketID.C_Ready, MakePacket<C_Ready>);
		_handler.Add((ushort)PacketID.C_Ready, PacketHandler.C_ReadyHandler);
		_makeFunc.Add((ushort)PacketID.C_SceneLoaded, MakePacket<C_SceneLoaded>);
		_handler.Add((ushort)PacketID.C_SceneLoaded, PacketHandler.C_SceneLoadedHandler);
		_makeFunc.Add((ushort)PacketID.C_ReqSummon, MakePacket<C_ReqSummon>);
		_handler.Add((ushort)PacketID.C_ReqSummon, PacketHandler.C_ReqSummonHandler);
		_makeFunc.Add((ushort)PacketID.C_RequestManaStatus, MakePacket<C_RequestManaStatus>);
		_handler.Add((ushort)PacketID.C_RequestManaStatus, PacketHandler.C_RequestManaStatusHandler);

	}

	public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer, Action<PacketSession, IPacket> onRecvCallback = null )
	{
		ushort count = 0;

		ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
		count += 2;
		ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
		count += 2;

		Func < PacketSession, ArraySegment<byte>, IPacket > func = null;
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