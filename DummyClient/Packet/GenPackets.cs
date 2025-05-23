using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using ServerCore;

public enum PacketID
{
	S_InitGame = 1,
	S_CardPool = 2,
	S_GameUpdate = 3,
	C_ReqSummon = 4,
	S_AnsSummon = 5,
	C_TargetCapture = 6,
	S_VerifyCapture = 7,
	C_AttackedRequest = 8,
	S_AttackConfirm = 9,
	C_SummonProJectile = 10,
	S_ShootConfirm = 11,
	S_OccupationSync = 12,
	S_TileClaimed = 13,
	S_TileBulkClaimed = 14,
	C_TileClaimReq = 15,
	C_RequestManaStatus = 16,
	S_SyncTime = 17,
	S_GameStateUpdate = 18,
	S_ManaUpdate = 19,
	S_UnitAction = 20,
	C_GoToLobby = 21,
	S_GameOver = 22,
	
}

public  interface IPacket
{
	ushort Protocol { get; }
	void Read(ArraySegment<byte> segment);
	ArraySegment<byte> Write();
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

public class S_OccupationSync : IPacket
{
	public int playerSession;
	public int excutionTick;
	public float playerOccupation;
	public float opponentOccupation;

	public ushort Protocol { get { return (ushort)PacketID.S_OccupationSync; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.playerSession = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.excutionTick = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.playerOccupation = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		this.opponentOccupation = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.S_OccupationSync);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerSession);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.excutionTick);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerOccupation);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.opponentOccupation);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class S_TileClaimed : IPacket
{
	public int x;
	public int y;
	public int excutionTick;
	public int playerSession;
	public float playerOccupation;
	public float opponentOccupation;

	public ushort Protocol { get { return (ushort)PacketID.S_TileClaimed; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.x = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.y = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.excutionTick = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.playerSession = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.playerOccupation = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		this.opponentOccupation = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.S_TileClaimed);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.x);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.y);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.excutionTick);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerSession);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerOccupation);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.opponentOccupation);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class S_TileBulkClaimed : IPacket
{
	public int ReqPlayerSessionId;
	public int excutionTick;
	public float occupationRate;
	public class TileBulk
	{
		public int x;
		public int y;
		public int claimedBySessionId;
	
		public void Read(ReadOnlySpan<byte> s, ref ushort count)
		{
			this.x = BitConverter.ToInt32(s.Slice(count, s.Length - count));
			count += sizeof(int);
			this.y = BitConverter.ToInt32(s.Slice(count, s.Length - count));
			count += sizeof(int);
			this.claimedBySessionId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
			count += sizeof(int);
		}
	
		public bool Write(Span<byte> s, ref ushort count)
		{
			ArraySegment<byte> segment = SendBufferHelper.Open(4096);
	
			bool success = true;
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.x);
			count += sizeof(int);
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.y);
			count += sizeof(int);
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.claimedBySessionId);
			count += sizeof(int);
			return success;
		}	
	}
	public List<TileBulk> tileBulks = new List<TileBulk>();

	public ushort Protocol { get { return (ushort)PacketID.S_TileBulkClaimed; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.ReqPlayerSessionId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.excutionTick = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.occupationRate = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		this.tileBulks.Clear();
		ushort tileBulkLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		for (int i = 0; i < tileBulkLen; i++)
		{
			TileBulk tileBulk = new TileBulk();
			tileBulk.Read(s, ref count);
			tileBulks.Add(tileBulk);
		}
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.S_TileBulkClaimed);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.ReqPlayerSessionId);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.excutionTick);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.occupationRate);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)this.tileBulks.Count);
		count += sizeof(ushort);
		foreach (TileBulk tileBulk in this.tileBulks)
			success &= tileBulk.Write(s, ref count);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class C_TileClaimReq : IPacket
{
	public int unitOid;
	public int x;
	public int y;

	public ushort Protocol { get { return (ushort)PacketID.C_TileClaimReq; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.unitOid = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.x = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.y = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.C_TileClaimReq);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.unitOid);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.x);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.y);
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

