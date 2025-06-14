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
		_makeFunc.Add((ushort)PacketID.C_ReqJoinGameServer, MakePacket<C_ReqJoinGameServer>);
		_handler.Add((ushort)PacketID.C_ReqJoinGameServer, PacketHandler.C_ReqJoinGameServerHandler);
		_makeFunc.Add((ushort)PacketID.C_SceneLoaded, MakePacket<C_SceneLoaded>);
		_handler.Add((ushort)PacketID.C_SceneLoaded, PacketHandler.C_SceneLoadedHandler);
		_makeFunc.Add((ushort)PacketID.C_ReqSummon, MakePacket<C_ReqSummon>);
		_handler.Add((ushort)PacketID.C_ReqSummon, PacketHandler.C_ReqSummonHandler);
		_makeFunc.Add((ushort)PacketID.C_TargetCapture, MakePacket<C_TargetCapture>);
		_handler.Add((ushort)PacketID.C_TargetCapture, PacketHandler.C_TargetCaptureHandler);
		_makeFunc.Add((ushort)PacketID.C_AttackedRequest, MakePacket<C_AttackedRequest>);
		_handler.Add((ushort)PacketID.C_AttackedRequest, PacketHandler.C_AttackedRequestHandler);
		_makeFunc.Add((ushort)PacketID.C_SummonProJectile, MakePacket<C_SummonProJectile>);
		_handler.Add((ushort)PacketID.C_SummonProJectile, PacketHandler.C_SummonProJectileHandler);
		_makeFunc.Add((ushort)PacketID.C_TileClaimReq, MakePacket<C_TileClaimReq>);
		_handler.Add((ushort)PacketID.C_TileClaimReq, PacketHandler.C_TileClaimReqHandler);
		_makeFunc.Add((ushort)PacketID.C_RequestManaStatus, MakePacket<C_RequestManaStatus>);
		_handler.Add((ushort)PacketID.C_RequestManaStatus, PacketHandler.C_RequestManaStatusHandler);
		_makeFunc.Add((ushort)PacketID.C_GoToLobby, MakePacket<C_GoToLobby>);
		_handler.Add((ushort)PacketID.C_GoToLobby, PacketHandler.C_GoToLobbyHandler);

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