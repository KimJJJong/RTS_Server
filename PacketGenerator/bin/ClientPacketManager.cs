using ServerCore;
using System;
using System.Collections.Generic;

class PacketManager
{
	#region Singleton
	static PacketManager _instance;
	public static PacketManager Instance
	{
		get
		{
			if (_instance == null)
				_instance = new PacketManager();
			return _instance;
		}
	}
	#endregion

	Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> _onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>>();
	Dictionary<ushort, Action<PacketSession, IPacket>> _handler = new Dictionary<ushort, Action<PacketSession, IPacket>>();
		
	public void Register()
	{
		_onRecv.Add((ushort)PacketID.S_Login, MakePacket<S_Login>);
		_handler.Add((ushort)PacketID.S_Login, PacketHandler.S_LoginHandler);
		_onRecv.Add((ushort)PacketID.S_EnterLobby, MakePacket<S_EnterLobby>);
		_handler.Add((ushort)PacketID.S_EnterLobby, PacketHandler.S_EnterLobbyHandler);
		_onRecv.Add((ushort)PacketID.S_CreateRoom, MakePacket<S_CreateRoom>);
		_handler.Add((ushort)PacketID.S_CreateRoom, PacketHandler.S_CreateRoomHandler);
		_onRecv.Add((ushort)PacketID.S_JoinRoom, MakePacket<S_JoinRoom>);
		_handler.Add((ushort)PacketID.S_JoinRoom, PacketHandler.S_JoinRoomHandler);
		_onRecv.Add((ushort)PacketID.S_Ready, MakePacket<S_Ready>);
		_handler.Add((ushort)PacketID.S_Ready, PacketHandler.S_ReadyHandler);
		_onRecv.Add((ushort)PacketID.S_StartGame, MakePacket<S_StartGame>);
		_handler.Add((ushort)PacketID.S_StartGame, PacketHandler.S_StartGameHandler);
		_onRecv.Add((ushort)PacketID.S_GameUpdate, MakePacket<S_GameUpdate>);
		_handler.Add((ushort)PacketID.S_GameUpdate, PacketHandler.S_GameUpdateHandler);
		_onRecv.Add((ushort)PacketID.S_GameOver, MakePacket<S_GameOver>);
		_handler.Add((ushort)PacketID.S_GameOver, PacketHandler.S_GameOverHandler);

	}

	public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
	{
		ushort count = 0;

		ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
		count += 2;
		ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
		count += 2;

		Action<PacketSession, ArraySegment<byte>> action = null;
		if (_onRecv.TryGetValue(id, out action))
			action.Invoke(session, buffer);
	}

	void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
	{
		T pkt = new T();
		pkt.Read(buffer);
		Action<PacketSession, IPacket> action = null;
		if (_handler.TryGetValue(pkt.Protocol, out action))
			action.Invoke(session, pkt);
	}
}