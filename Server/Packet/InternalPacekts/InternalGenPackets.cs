using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum InternalPacketID
{
    M_S_CreateRoom = 1,
    S_M_CreateRoom = 2
}




public class M_S_CreateRoom : IPacket
{
    public string player1;
    public string player2;

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
        success &= BitConverter.TryWriteBytes(s, count);
        if (success == false)
            return null;
        return SendBufferHelper.Close(count);
    }
}

public class S_M_CreateRoom : IPacket
{
    public string roomId;

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
        success &= BitConverter.TryWriteBytes(s, count);
        if (success == false)
            return null;
        return SendBufferHelper.Close(count);
    }
}

