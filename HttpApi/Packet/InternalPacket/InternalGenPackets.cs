using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum InternalPacketID
{
    M_S_CreateRoom = 1,
    S_M_CreateRoom = 2,

    S_M_GameResult = 3,
}


public interface IPacket
{
    ushort Protocol { get; }
    void Read(ArraySegment<byte> segment);
    ArraySegment<byte> Write();
}


public class M_S_CreateRoom : IPacket
{
	public string player1;
	public string player2;
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

	public ushort Protocol { get { return (ushort)InternalPacketID.M_S_CreateRoom; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		ushort player1Len = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		this.player1 = Encoding.Unicode.GetString(s.Slice(count, player1Len));
		count += player1Len;
		ushort player2Len = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		this.player2 = Encoding.Unicode.GetString(s.Slice(count, player2Len));
		count += player2Len;
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
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)InternalPacketID.M_S_CreateRoom);
		count += sizeof(ushort);
		ushort player1Len = (ushort)Encoding.Unicode.GetBytes(this.player1, 0, this.player1.Length, segment.Array, segment.Offset + count + sizeof(ushort));
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), player1Len);
		count += sizeof(ushort);
		count += player1Len;
		ushort player2Len = (ushort)Encoding.Unicode.GetBytes(this.player2, 0, this.player2.Length, segment.Array, segment.Offset + count + sizeof(ushort));
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), player2Len);
		count += sizeof(ushort);
		count += player2Len;
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



public class S_M_CreateRoom : IPacket
{
    public string roomId;
    public string player1;
    public string player2;

    public ushort Protocol { get { return (ushort)InternalPacketID.S_M_CreateRoom; } }

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
        ushort player1Len = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
        count += sizeof(ushort);
        this.player1 = Encoding.Unicode.GetString(s.Slice(count, player1Len));
        count += player1Len;
        ushort player2Len = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
        count += sizeof(ushort);
        this.player2 = Encoding.Unicode.GetString(s.Slice(count, player2Len));
        count += player2Len;
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        ushort count = 0;
        bool success = true;

        Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)InternalPacketID.S_M_CreateRoom);
        count += sizeof(ushort);
        ushort roomIdLen = (ushort)Encoding.Unicode.GetBytes(this.roomId, 0, this.roomId.Length, segment.Array, segment.Offset + count + sizeof(ushort));
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), roomIdLen);
        count += sizeof(ushort);
        count += roomIdLen;
        ushort player1Len = (ushort)Encoding.Unicode.GetBytes(this.player1, 0, this.player1.Length, segment.Array, segment.Offset + count + sizeof(ushort));
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), player1Len);
        count += sizeof(ushort);
        count += player1Len;
        ushort player2Len = (ushort)Encoding.Unicode.GetBytes(this.player2, 0, this.player2.Length, segment.Array, segment.Offset + count + sizeof(ushort));
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), player2Len);
        count += sizeof(ushort);
        count += player2Len;
        success &= BitConverter.TryWriteBytes(s, count);
        if (success == false)
            return null;
        return SendBufferHelper.Close(count);
    }
}

public class S_M_GameResult : IPacket
{
    public string roomId;
    public string winnerId;
    public string loserId;

    public ushort Protocol { get { return (ushort)InternalPacketID.S_M_GameResult; } }

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
        ushort winnerIdLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
        count += sizeof(ushort);
        this.winnerId = Encoding.Unicode.GetString(s.Slice(count, winnerIdLen));
        count += winnerIdLen;
        ushort loserIdLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
        count += sizeof(ushort);
        this.loserId = Encoding.Unicode.GetString(s.Slice(count, loserIdLen));
        count += loserIdLen;
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        ushort count = 0;
        bool success = true;

        Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)InternalPacketID.S_M_GameResult);
        count += sizeof(ushort);
        ushort roomIdLen = (ushort)Encoding.Unicode.GetBytes(this.roomId, 0, this.roomId.Length, segment.Array, segment.Offset + count + sizeof(ushort));
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), roomIdLen);
        count += sizeof(ushort);
        count += roomIdLen;
        ushort winnerIdLen = (ushort)Encoding.Unicode.GetBytes(this.winnerId, 0, this.winnerId.Length, segment.Array, segment.Offset + count + sizeof(ushort));
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), winnerIdLen);
        count += sizeof(ushort);
        count += winnerIdLen;
        ushort loserIdLen = (ushort)Encoding.Unicode.GetBytes(this.loserId, 0, this.loserId.Length, segment.Array, segment.Offset + count + sizeof(ushort));
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), loserIdLen);
        count += sizeof(ushort);
        count += loserIdLen;
        success &= BitConverter.TryWriteBytes(s, count);
        if (success == false)
            return null;
        return SendBufferHelper.Close(count);
    }
}
