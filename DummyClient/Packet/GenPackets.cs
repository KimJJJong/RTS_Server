using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using ServerCore;

public enum PacketID
{
	C_LoginAuth = 1,
	C_Login = 2,
	S_Login = 3,
	C_EnterLobby = 4,
	S_EnterLobby = 5,
	C_CreateRoom = 6,
	S_CreateRoom = 7,
	C_JoinRoom = 8,
	S_JoinRoom = 9,
	C_MatchRequest = 10,
	C_MatchCancel = 11,
	C_Ready = 12,
	S_Ready = 13,
	S_StartGame = 14,
	C_SetCardPool = 15,
	C_SceneLoaded = 16,
	S_SceneLoad = 17,
	S_InitGame = 18,
	S_CardPool = 19,
	S_GameUpdate = 20,
	C_ReqSummon = 21,
	S_AnsSummon = 22,
	C_TargetCapture = 23,
	S_VerifyCapture = 24,
	C_AttackedRequest = 25,
	S_AttackConfirm = 26,
	C_SummonProJectile = 27,
	S_ShootConfirm = 28,
	C_RequestManaStatus = 29,
	S_SyncTime = 30,
	S_GameStateUpdate = 31,
	S_ManaUpdate = 32,
	S_UnitAction = 33,
	C_GoToLobby = 34,
	S_GameOver = 35,
	
}

public  interface IPacket
{
	ushort Protocol { get; }
	void Read(ArraySegment<byte> segment);
	ArraySegment<byte> Write();
}


public class C_LoginAuth : IPacket
{
	public string accessToken;

	public ushort Protocol { get { return (ushort)PacketID.C_LoginAuth; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		ushort accessTokenLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		this.accessToken = Encoding.Unicode.GetString(s.Slice(count, accessTokenLen));
		count += accessTokenLen;
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.C_LoginAuth);
		count += sizeof(ushort);
		ushort accessTokenLen = (ushort)Encoding.Unicode.GetBytes(this.accessToken, 0, this.accessToken.Length, segment.Array, segment.Offset + count + sizeof(ushort));
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), accessTokenLen);
		count += sizeof(ushort);
		count += accessTokenLen;
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class C_Login : IPacket
{
	public string username;
	public string password;

	public ushort Protocol { get { return (ushort)PacketID.C_Login; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		ushort usernameLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		this.username = Encoding.Unicode.GetString(s.Slice(count, usernameLen));
		count += usernameLen;
		ushort passwordLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		this.password = Encoding.Unicode.GetString(s.Slice(count, passwordLen));
		count += passwordLen;
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.C_Login);
		count += sizeof(ushort);
		ushort usernameLen = (ushort)Encoding.Unicode.GetBytes(this.username, 0, this.username.Length, segment.Array, segment.Offset + count + sizeof(ushort));
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), usernameLen);
		count += sizeof(ushort);
		count += usernameLen;
		ushort passwordLen = (ushort)Encoding.Unicode.GetBytes(this.password, 0, this.password.Length, segment.Array, segment.Offset + count + sizeof(ushort));
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), passwordLen);
		count += sizeof(ushort);
		count += passwordLen;
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class S_Login : IPacket
{
	public bool success;
	public string message;

	public ushort Protocol { get { return (ushort)PacketID.S_Login; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.success = BitConverter.ToBoolean(s.Slice(count, s.Length - count));
		count += sizeof(bool);
		ushort messageLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		this.message = Encoding.Unicode.GetString(s.Slice(count, messageLen));
		count += messageLen;
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.S_Login);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.success);
		count += sizeof(bool);
		ushort messageLen = (ushort)Encoding.Unicode.GetBytes(this.message, 0, this.message.Length, segment.Array, segment.Offset + count + sizeof(ushort));
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), messageLen);
		count += sizeof(ushort);
		count += messageLen;
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class C_EnterLobby : IPacket
{
	public int userId;

	public ushort Protocol { get { return (ushort)PacketID.C_EnterLobby; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.userId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.C_EnterLobby);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.userId);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class S_EnterLobby : IPacket
{
	public class RoomList
	{
		public int roomId;
	
		public void Read(ReadOnlySpan<byte> s, ref ushort count)
		{
			this.roomId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
			count += sizeof(int);
		}
	
		public bool Write(Span<byte> s, ref ushort count)
		{
			ArraySegment<byte> segment = SendBufferHelper.Open(4096);
	
			bool success = true;
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.roomId);
			count += sizeof(int);
			return success;
		}	
	}
	public List<RoomList> roomLists = new List<RoomList>();

	public ushort Protocol { get { return (ushort)PacketID.S_EnterLobby; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.roomLists.Clear();
		ushort roomListLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		for (int i = 0; i < roomListLen; i++)
		{
			RoomList roomList = new RoomList();
			roomList.Read(s, ref count);
			roomLists.Add(roomList);
		}
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.S_EnterLobby);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)this.roomLists.Count);
		count += sizeof(ushort);
		foreach (RoomList roomList in this.roomLists)
			success &= roomList.Write(s, ref count);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class C_CreateRoom : IPacket
{
	public string roomName;

	public ushort Protocol { get { return (ushort)PacketID.C_CreateRoom; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		ushort roomNameLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		this.roomName = Encoding.Unicode.GetString(s.Slice(count, roomNameLen));
		count += roomNameLen;
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.C_CreateRoom);
		count += sizeof(ushort);
		ushort roomNameLen = (ushort)Encoding.Unicode.GetBytes(this.roomName, 0, this.roomName.Length, segment.Array, segment.Offset + count + sizeof(ushort));
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), roomNameLen);
		count += sizeof(ushort);
		count += roomNameLen;
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class S_CreateRoom : IPacket
{
	public bool success;
	public string roomId;

	public ushort Protocol { get { return (ushort)PacketID.S_CreateRoom; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.success = BitConverter.ToBoolean(s.Slice(count, s.Length - count));
		count += sizeof(bool);
		ushort roomIdLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		this.roomId = Encoding.Unicode.GetString(s.Slice(count, roomIdLen));
		count += roomIdLen;
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.S_CreateRoom);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.success);
		count += sizeof(bool);
		ushort roomIdLen = (ushort)Encoding.Unicode.GetBytes(this.roomId, 0, this.roomId.Length, segment.Array, segment.Offset + count + sizeof(ushort));
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), roomIdLen);
		count += sizeof(ushort);
		count += roomIdLen;
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class C_JoinRoom : IPacket
{
	public string roomId;

	public ushort Protocol { get { return (ushort)PacketID.C_JoinRoom; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		ushort roomIdLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		this.roomId = Encoding.Unicode.GetString(s.Slice(count, roomIdLen));
		count += roomIdLen;
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.C_JoinRoom);
		count += sizeof(ushort);
		ushort roomIdLen = (ushort)Encoding.Unicode.GetBytes(this.roomId, 0, this.roomId.Length, segment.Array, segment.Offset + count + sizeof(ushort));
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), roomIdLen);
		count += sizeof(ushort);
		count += roomIdLen;
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class S_JoinRoom : IPacket
{
	public int sessionID;
	public string roomId;

	public ushort Protocol { get { return (ushort)PacketID.S_JoinRoom; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.sessionID = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		ushort roomIdLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		this.roomId = Encoding.Unicode.GetString(s.Slice(count, roomIdLen));
		count += roomIdLen;
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.S_JoinRoom);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.sessionID);
		count += sizeof(int);
		ushort roomIdLen = (ushort)Encoding.Unicode.GetBytes(this.roomId, 0, this.roomId.Length, segment.Array, segment.Offset + count + sizeof(ushort));
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), roomIdLen);
		count += sizeof(ushort);
		count += roomIdLen;
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class C_MatchRequest : IPacket
{
	

	public ushort Protocol { get { return (ushort)PacketID.C_MatchRequest; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.C_MatchRequest);
		count += sizeof(ushort);
		
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class C_MatchCancel : IPacket
{
	

	public ushort Protocol { get { return (ushort)PacketID.C_MatchCancel; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.C_MatchCancel);
		count += sizeof(ushort);
		
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class C_Ready : IPacket
{
	public bool isReady;

	public ushort Protocol { get { return (ushort)PacketID.C_Ready; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.isReady = BitConverter.ToBoolean(s.Slice(count, s.Length - count));
		count += sizeof(bool);
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.C_Ready);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.isReady);
		count += sizeof(bool);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class S_Ready : IPacket
{
	

	public ushort Protocol { get { return (ushort)PacketID.S_Ready; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.S_Ready);
		count += sizeof(ushort);
		
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class S_StartGame : IPacket
{
	public string gameId;

	public ushort Protocol { get { return (ushort)PacketID.S_StartGame; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		ushort gameIdLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		this.gameId = Encoding.Unicode.GetString(s.Slice(count, gameIdLen));
		count += gameIdLen;
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.S_StartGame);
		count += sizeof(ushort);
		ushort gameIdLen = (ushort)Encoding.Unicode.GetBytes(this.gameId, 0, this.gameId.Length, segment.Array, segment.Offset + count + sizeof(ushort));
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), gameIdLen);
		count += sizeof(ushort);
		count += gameIdLen;
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class C_SetCardPool : IPacket
{
	public class CardCombination
	{
		public int lv;
		public string uid;
	
		public void Read(ReadOnlySpan<byte> s, ref ushort count)
		{
			this.lv = BitConverter.ToInt32(s.Slice(count, s.Length - count));
			count += sizeof(int);
			ushort uidLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
			count += sizeof(ushort);
			this.uid = Encoding.Unicode.GetString(s.Slice(count, uidLen));
			count += uidLen;
		}
	
		public bool Write(Span<byte> s, ref ushort count)
		{
			ArraySegment<byte> segment = SendBufferHelper.Open(4096);
	
			bool success = true;
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.lv);
			count += sizeof(int);
			ushort uidLen = (ushort)Encoding.Unicode.GetBytes(this.uid, 0, this.uid.Length, segment.Array, segment.Offset + count + sizeof(ushort));
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), uidLen);
			count += sizeof(ushort);
			count += uidLen;
			return success;
		}	
	}
	public List<CardCombination> cardCombinations = new List<CardCombination>();

	public ushort Protocol { get { return (ushort)PacketID.C_SetCardPool; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.cardCombinations.Clear();
		ushort cardCombinationLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		for (int i = 0; i < cardCombinationLen; i++)
		{
			CardCombination cardCombination = new CardCombination();
			cardCombination.Read(s, ref count);
			cardCombinations.Add(cardCombination);
		}
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.C_SetCardPool);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)this.cardCombinations.Count);
		count += sizeof(ushort);
		foreach (CardCombination cardCombination in this.cardCombinations)
			success &= cardCombination.Write(s, ref count);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class C_SceneLoaded : IPacket
{
	public bool isLoad;

	public ushort Protocol { get { return (ushort)PacketID.C_SceneLoaded; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.isLoad = BitConverter.ToBoolean(s.Slice(count, s.Length - count));
		count += sizeof(bool);
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.C_SceneLoaded);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.isLoad);
		count += sizeof(bool);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class S_SceneLoad : IPacket
{
	public double StartTime;
	public double ServerSendTime;

	public ushort Protocol { get { return (ushort)PacketID.S_SceneLoad; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.StartTime = BitConverter.ToDouble(s.Slice(count, s.Length - count));
		count += sizeof(double);
		this.ServerSendTime = BitConverter.ToDouble(s.Slice(count, s.Length - count));
		count += sizeof(double);
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.S_SceneLoad);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.StartTime);
		count += sizeof(double);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.ServerSendTime);
		count += sizeof(double);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class S_InitGame : IPacket
{
	public double gameStartTime;
	public double duration;

	public ushort Protocol { get { return (ushort)PacketID.S_InitGame; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.gameStartTime = BitConverter.ToDouble(s.Slice(count, s.Length - count));
		count += sizeof(double);
		this.duration = BitConverter.ToDouble(s.Slice(count, s.Length - count));
		count += sizeof(double);
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.S_InitGame);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.gameStartTime);
		count += sizeof(double);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.duration);
		count += sizeof(double);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class S_CardPool : IPacket
{
	public int size;
	public class CardCombination
	{
		public int lv;
		public string uid;
	
		public void Read(ReadOnlySpan<byte> s, ref ushort count)
		{
			this.lv = BitConverter.ToInt32(s.Slice(count, s.Length - count));
			count += sizeof(int);
			ushort uidLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
			count += sizeof(ushort);
			this.uid = Encoding.Unicode.GetString(s.Slice(count, uidLen));
			count += uidLen;
		}
	
		public bool Write(Span<byte> s, ref ushort count)
		{
			ArraySegment<byte> segment = SendBufferHelper.Open(4096);
	
			bool success = true;
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.lv);
			count += sizeof(int);
			ushort uidLen = (ushort)Encoding.Unicode.GetBytes(this.uid, 0, this.uid.Length, segment.Array, segment.Offset + count + sizeof(ushort));
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), uidLen);
			count += sizeof(ushort);
			count += uidLen;
			return success;
		}	
	}
	public List<CardCombination> cardCombinations = new List<CardCombination>();

	public ushort Protocol { get { return (ushort)PacketID.S_CardPool; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.size = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.cardCombinations.Clear();
		ushort cardCombinationLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		for (int i = 0; i < cardCombinationLen; i++)
		{
			CardCombination cardCombination = new CardCombination();
			cardCombination.Read(s, ref count);
			cardCombinations.Add(cardCombination);
		}
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.S_CardPool);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.size);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)this.cardCombinations.Count);
		count += sizeof(ushort);
		foreach (CardCombination cardCombination in this.cardCombinations)
			success &= cardCombination.Write(s, ref count);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class S_GameUpdate : IPacket
{
	public class Pool
	{
		public int unitId;
		public int hp;
		public float x;
		public float y;
	
		public void Read(ReadOnlySpan<byte> s, ref ushort count)
		{
			this.unitId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
			count += sizeof(int);
			this.hp = BitConverter.ToInt32(s.Slice(count, s.Length - count));
			count += sizeof(int);
			this.x = BitConverter.ToSingle(s.Slice(count, s.Length - count));
			count += sizeof(float);
			this.y = BitConverter.ToSingle(s.Slice(count, s.Length - count));
			count += sizeof(float);
		}
	
		public bool Write(Span<byte> s, ref ushort count)
		{
			ArraySegment<byte> segment = SendBufferHelper.Open(4096);
	
			bool success = true;
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.unitId);
			count += sizeof(int);
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.hp);
			count += sizeof(int);
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.x);
			count += sizeof(float);
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.y);
			count += sizeof(float);
			return success;
		}	
	}
	public List<Pool> pools = new List<Pool>();

	public ushort Protocol { get { return (ushort)PacketID.S_GameUpdate; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.pools.Clear();
		ushort poolLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		for (int i = 0; i < poolLen; i++)
		{
			Pool pool = new Pool();
			pool.Read(s, ref count);
			pools.Add(pool);
		}
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.S_GameUpdate);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)this.pools.Count);
		count += sizeof(ushort);
		foreach (Pool pool in this.pools)
			success &= pool.Write(s, ref count);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class C_ReqSummon : IPacket
{
	public float x;
	public float y;
	public int oid;
	public float needMana;
	public int reqSessionID;
	public long ClientSendTimeMs;

	public ushort Protocol { get { return (ushort)PacketID.C_ReqSummon; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.x = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		this.y = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		this.oid = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.needMana = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		this.reqSessionID = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.ClientSendTimeMs = BitConverter.ToInt64(s.Slice(count, s.Length - count));
		count += sizeof(long);
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.C_ReqSummon);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.x);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.y);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.oid);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.needMana);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.reqSessionID);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.ClientSendTimeMs);
		count += sizeof(long);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class S_AnsSummon : IPacket
{
	public float x;
	public float y;
	public float reducedMana;
	public int oid;
	public int reqSessionID;
	public int randomValue;
	public int ExcuteTick;
	public long ServerReceiveTimeMs;
	public long ServerStartTimeMs;
	public long ClientSendTimeMs;

	public ushort Protocol { get { return (ushort)PacketID.S_AnsSummon; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.x = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		this.y = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		this.reducedMana = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		this.oid = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.reqSessionID = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.randomValue = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.ExcuteTick = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.ServerReceiveTimeMs = BitConverter.ToInt64(s.Slice(count, s.Length - count));
		count += sizeof(long);
		this.ServerStartTimeMs = BitConverter.ToInt64(s.Slice(count, s.Length - count));
		count += sizeof(long);
		this.ClientSendTimeMs = BitConverter.ToInt64(s.Slice(count, s.Length - count));
		count += sizeof(long);
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.S_AnsSummon);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.x);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.y);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.reducedMana);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.oid);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.reqSessionID);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.randomValue);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.ExcuteTick);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.ServerReceiveTimeMs);
		count += sizeof(long);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.ServerStartTimeMs);
		count += sizeof(long);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.ClientSendTimeMs);
		count += sizeof(long);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class C_TargetCapture : IPacket
{
	public int attackerOid;
	public int targetOid;
	public float attackerX;
	public float attackerY;
	public float targetX;
	public float targetY;
	public int localTick;

	public ushort Protocol { get { return (ushort)PacketID.C_TargetCapture; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.attackerOid = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.targetOid = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.attackerX = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		this.attackerY = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		this.targetX = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		this.targetY = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		this.localTick = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.C_TargetCapture);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.attackerOid);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.targetOid);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.attackerX);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.attackerY);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.targetX);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.targetY);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.localTick);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class S_VerifyCapture : IPacket
{
	public float correctedDir;
	public float correctedX;
	public float correctedY;
	public int correctedTick;

	public ushort Protocol { get { return (ushort)PacketID.S_VerifyCapture; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.correctedDir = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		this.correctedX = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		this.correctedY = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		this.correctedTick = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.S_VerifyCapture);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.correctedDir);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.correctedX);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.correctedY);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.correctedTick);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class C_AttackedRequest : IPacket
{
	public int attackerOid;
	public int targetOid;
	public float attackerX;
	public float attackerY;
	public float targetX;
	public float targetY;
	public int hpDecreaseTick;
	public int clientAttackedTick;

	public ushort Protocol { get { return (ushort)PacketID.C_AttackedRequest; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.attackerOid = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.targetOid = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.attackerX = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		this.attackerY = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		this.targetX = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		this.targetY = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		this.hpDecreaseTick = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.clientAttackedTick = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.C_AttackedRequest);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.attackerOid);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.targetOid);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.attackerX);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.attackerY);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.targetX);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.targetY);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.hpDecreaseTick);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.clientAttackedTick);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class S_AttackConfirm : IPacket
{
	public int attackerOid;
	public int targetOid;
	public float dir;
	public float correctedX;
	public float correctedY;
	public float targetVerifyHp;
	public int attackVerifyTick;

	public ushort Protocol { get { return (ushort)PacketID.S_AttackConfirm; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.attackerOid = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.targetOid = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.dir = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		this.correctedX = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		this.correctedY = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		this.targetVerifyHp = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		this.attackVerifyTick = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.S_AttackConfirm);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.attackerOid);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.targetOid);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.dir);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.correctedX);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.correctedY);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.targetVerifyHp);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.attackVerifyTick);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class C_SummonProJectile : IPacket
{
	public int summonerOid;
	public int targetOid;
	public int projectileOid;
	public float summonerX;
	public float summonerY;
	public float targetX;
	public float targetY;
	public int clientRequestTick;

	public ushort Protocol { get { return (ushort)PacketID.C_SummonProJectile; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.summonerOid = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.targetOid = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.projectileOid = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.summonerX = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		this.summonerY = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		this.targetX = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		this.targetY = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		this.clientRequestTick = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.C_SummonProJectile);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.summonerOid);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.targetOid);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.projectileOid);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.summonerX);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.summonerY);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.targetX);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.targetY);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.clientRequestTick);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class S_ShootConfirm : IPacket
{
	public int summonerOid;
	public int targetOid;
	public int projcetileOid;
	public float projectileSpeed;
	public float targetX;
	public float targetY;
	public float startX;
	public float startY;
	public int shootTick;

	public ushort Protocol { get { return (ushort)PacketID.S_ShootConfirm; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.summonerOid = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.targetOid = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.projcetileOid = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.projectileSpeed = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		this.targetX = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		this.targetY = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		this.startX = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		this.startY = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		this.shootTick = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.S_ShootConfirm);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.summonerOid);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.targetOid);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.projcetileOid);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.projectileSpeed);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.targetX);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.targetY);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.startX);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.startY);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.shootTick);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class C_RequestManaStatus : IPacket
{
	public int playerId;

	public ushort Protocol { get { return (ushort)PacketID.C_RequestManaStatus; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.playerId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.C_RequestManaStatus);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class S_SyncTime : IPacket
{
	public double serverTime;

	public ushort Protocol { get { return (ushort)PacketID.S_SyncTime; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.serverTime = BitConverter.ToDouble(s.Slice(count, s.Length - count));
		count += sizeof(double);
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.S_SyncTime);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.serverTime);
		count += sizeof(double);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class S_GameStateUpdate : IPacket
{
	public class Units
	{
		public int objectID;
		public int unitID;
		public float x;
		public float y;
		public int hp;
	
		public void Read(ReadOnlySpan<byte> s, ref ushort count)
		{
			this.objectID = BitConverter.ToInt32(s.Slice(count, s.Length - count));
			count += sizeof(int);
			this.unitID = BitConverter.ToInt32(s.Slice(count, s.Length - count));
			count += sizeof(int);
			this.x = BitConverter.ToSingle(s.Slice(count, s.Length - count));
			count += sizeof(float);
			this.y = BitConverter.ToSingle(s.Slice(count, s.Length - count));
			count += sizeof(float);
			this.hp = BitConverter.ToInt32(s.Slice(count, s.Length - count));
			count += sizeof(int);
		}
	
		public bool Write(Span<byte> s, ref ushort count)
		{
			ArraySegment<byte> segment = SendBufferHelper.Open(4096);
	
			bool success = true;
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.objectID);
			count += sizeof(int);
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.unitID);
			count += sizeof(int);
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.x);
			count += sizeof(float);
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.y);
			count += sizeof(float);
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.hp);
			count += sizeof(int);
			return success;
		}	
	}
	public List<Units> unitss = new List<Units>();
	public class Mana
	{
		public float playerMana;
		public int sessionID;
	
		public void Read(ReadOnlySpan<byte> s, ref ushort count)
		{
			this.playerMana = BitConverter.ToSingle(s.Slice(count, s.Length - count));
			count += sizeof(float);
			this.sessionID = BitConverter.ToInt32(s.Slice(count, s.Length - count));
			count += sizeof(int);
		}
	
		public bool Write(Span<byte> s, ref ushort count)
		{
			ArraySegment<byte> segment = SendBufferHelper.Open(4096);
	
			bool success = true;
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerMana);
			count += sizeof(float);
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.sessionID);
			count += sizeof(int);
			return success;
		}	
	}
	public List<Mana> manas = new List<Mana>();
	public double serverTime;

	public ushort Protocol { get { return (ushort)PacketID.S_GameStateUpdate; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.unitss.Clear();
		ushort unitsLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		for (int i = 0; i < unitsLen; i++)
		{
			Units units = new Units();
			units.Read(s, ref count);
			unitss.Add(units);
		}
		this.manas.Clear();
		ushort manaLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		for (int i = 0; i < manaLen; i++)
		{
			Mana mana = new Mana();
			mana.Read(s, ref count);
			manas.Add(mana);
		}
		this.serverTime = BitConverter.ToDouble(s.Slice(count, s.Length - count));
		count += sizeof(double);
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.S_GameStateUpdate);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)this.unitss.Count);
		count += sizeof(ushort);
		foreach (Units units in this.unitss)
			success &= units.Write(s, ref count);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)this.manas.Count);
		count += sizeof(ushort);
		foreach (Mana mana in this.manas)
			success &= mana.Write(s, ref count);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.serverTime);
		count += sizeof(double);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class S_ManaUpdate : IPacket
{
	public int playerId;
	public float currentMana;

	public ushort Protocol { get { return (ushort)PacketID.S_ManaUpdate; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.playerId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.currentMana = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.S_ManaUpdate);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.currentMana);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class S_UnitAction : IPacket
{
	public int unitId;
	public int targetX;
	public int targetY;
	public string actionType;

	public ushort Protocol { get { return (ushort)PacketID.S_UnitAction; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.unitId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.targetX = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.targetY = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		ushort actionTypeLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		this.actionType = Encoding.Unicode.GetString(s.Slice(count, actionTypeLen));
		count += actionTypeLen;
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.S_UnitAction);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.unitId);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.targetX);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.targetY);
		count += sizeof(int);
		ushort actionTypeLen = (ushort)Encoding.Unicode.GetBytes(this.actionType, 0, this.actionType.Length, segment.Array, segment.Offset + count + sizeof(ushort));
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), actionTypeLen);
		count += sizeof(ushort);
		count += actionTypeLen;
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class C_GoToLobby : IPacket
{
	

	public ushort Protocol { get { return (ushort)PacketID.C_GoToLobby; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.C_GoToLobby);
		count += sizeof(ushort);
		
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class S_GameOver : IPacket
{
	public int winnerId;
	public string resultMessage;

	public ushort Protocol { get { return (ushort)PacketID.S_GameOver; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.winnerId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		ushort resultMessageLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		this.resultMessage = Encoding.Unicode.GetString(s.Slice(count, resultMessageLen));
		count += resultMessageLen;
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.S_GameOver);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.winnerId);
		count += sizeof(int);
		ushort resultMessageLen = (ushort)Encoding.Unicode.GetBytes(this.resultMessage, 0, this.resultMessage.Length, segment.Array, segment.Offset + count + sizeof(ushort));
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), resultMessageLen);
		count += sizeof(ushort);
		count += resultMessageLen;
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

