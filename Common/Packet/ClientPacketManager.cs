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
		_makeFunc.Add((ushort)PacketID.S_Login, MakePacket<S_Login>);
		_handler.Add((ushort)PacketID.S_Login, PacketHandler.S_LoginHandler);
		_makeFunc.Add((ushort)PacketID.S_EnterLobby, MakePacket<S_EnterLobby>);
		_handler.Add((ushort)PacketID.S_EnterLobby, PacketHandler.S_EnterLobbyHandler);
		_makeFunc.Add((ushort)PacketID.S_CreateRoom, MakePacket<S_CreateRoom>);
		_handler.Add((ushort)PacketID.S_CreateRoom, PacketHandler.S_CreateRoomHandler);
		_makeFunc.Add((ushort)PacketID.S_JoinRoom, MakePacket<S_JoinRoom>);
		_handler.Add((ushort)PacketID.S_JoinRoom, PacketHandler.S_JoinRoomHandler);
		_makeFunc.Add((ushort)PacketID.S_Ready, MakePacket<S_Ready>);
		_handler.Add((ushort)PacketID.S_Ready, PacketHandler.S_ReadyHandler);
		_makeFunc.Add((ushort)PacketID.S_StartGame, MakePacket<S_StartGame>);
		_handler.Add((ushort)PacketID.S_StartGame, PacketHandler.S_StartGameHandler);
		_makeFunc.Add((ushort)PacketID.S_SceneLoad, MakePacket<S_SceneLoad>);
		_handler.Add((ushort)PacketID.S_SceneLoad, PacketHandler.S_SceneLoadHandler);
		_makeFunc.Add((ushort)PacketID.S_InitGame, MakePacket<S_InitGame>);
		_handler.Add((ushort)PacketID.S_InitGame, PacketHandler.S_InitGameHandler);
		_makeFunc.Add((ushort)PacketID.S_CardPool, MakePacket<S_CardPool>);
		_handler.Add((ushort)PacketID.S_CardPool, PacketHandler.S_CardPoolHandler);
		_makeFunc.Add((ushort)PacketID.S_GameUpdate, MakePacket<S_GameUpdate>);
		_handler.Add((ushort)PacketID.S_GameUpdate, PacketHandler.S_GameUpdateHandler);
		_makeFunc.Add((ushort)PacketID.S_AnsSummon, MakePacket<S_AnsSummon>);
		_handler.Add((ushort)PacketID.S_AnsSummon, PacketHandler.S_AnsSummonHandler);
		_makeFunc.Add((ushort)PacketID.S_VerifyCapture, MakePacket<S_VerifyCapture>);
		_handler.Add((ushort)PacketID.S_VerifyCapture, PacketHandler.S_VerifyCaptureHandler);
		_makeFunc.Add((ushort)PacketID.S_AttackConfirm, MakePacket<S_AttackConfirm>);
		_handler.Add((ushort)PacketID.S_AttackConfirm, PacketHandler.S_AttackConfirmHandler);
		_makeFunc.Add((ushort)PacketID.S_ShootConfirm, MakePacket<S_ShootConfirm>);
		_handler.Add((ushort)PacketID.S_ShootConfirm, PacketHandler.S_ShootConfirmHandler);
		_makeFunc.Add((ushort)PacketID.S_OccupationSync, MakePacket<S_OccupationSync>);
		_handler.Add((ushort)PacketID.S_OccupationSync, PacketHandler.S_OccupationSyncHandler);
		_makeFunc.Add((ushort)PacketID.S_TileClaimed, MakePacket<S_TileClaimed>);
		_handler.Add((ushort)PacketID.S_TileClaimed, PacketHandler.S_TileClaimedHandler);
		_makeFunc.Add((ushort)PacketID.S_TileBulkClaimed, MakePacket<S_TileBulkClaimed>);
		_handler.Add((ushort)PacketID.S_TileBulkClaimed, PacketHandler.S_TileBulkClaimedHandler);
		_makeFunc.Add((ushort)PacketID.S_SyncTime, MakePacket<S_SyncTime>);
		_handler.Add((ushort)PacketID.S_SyncTime, PacketHandler.S_SyncTimeHandler);
		_makeFunc.Add((ushort)PacketID.S_GameStateUpdate, MakePacket<S_GameStateUpdate>);
		_handler.Add((ushort)PacketID.S_GameStateUpdate, PacketHandler.S_GameStateUpdateHandler);
		_makeFunc.Add((ushort)PacketID.S_ManaUpdate, MakePacket<S_ManaUpdate>);
		_handler.Add((ushort)PacketID.S_ManaUpdate, PacketHandler.S_ManaUpdateHandler);
		_makeFunc.Add((ushort)PacketID.S_UnitAction, MakePacket<S_UnitAction>);
		_handler.Add((ushort)PacketID.S_UnitAction, PacketHandler.S_UnitActionHandler);
		_makeFunc.Add((ushort)PacketID.S_GameOver, MakePacket<S_GameOver>);
		_handler.Add((ushort)PacketID.S_GameOver, PacketHandler.S_GameOverHandler);

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