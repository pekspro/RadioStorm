namespace Pekspro.RadioStorm.Utilities;

public sealed class ReverseBinaryWriter
{
    public BinaryWriter BinaryWriter { get; }

    public ReverseBinaryWriter(BinaryWriter binaryWriter)
    {
        BinaryWriter = binaryWriter;
    }

    public void WriteInt64(long value)
    {
        BinaryWriter.Write(BinaryPrimitives.ReverseEndianness(value));
    }

    public void WriteInt32(int value)
    {
        BinaryWriter.Write(BinaryPrimitives.ReverseEndianness(value));
    }

    public void WriteInt16(short value)
    {
        BinaryWriter.Write(BinaryPrimitives.ReverseEndianness(value));
    }

    public void WriteUInt64(ulong value)
    {
        BinaryWriter.Write(BinaryPrimitives.ReverseEndianness(value));
    }

    public void WriteUInt32(uint value)
    {
        BinaryWriter.Write(BinaryPrimitives.ReverseEndianness(value));
    }

    public void WriteUInt16(ushort value)
    {
        BinaryWriter.Write(BinaryPrimitives.ReverseEndianness(value));
    }

    public void WriteByte(byte value)
    {
        BinaryWriter.Write(value);
    }

    public void WriteBoolean(bool value)
    {
        BinaryWriter.Write(value);
    }

    public void WriteDateTime(DateTimeOffset value)
    {
        var epoch1601 = new DateTimeOffset(1601, 1, 1, 0, 0, 0, TimeSpan.Zero);
        long ticks = (value - epoch1601).Ticks;
        WriteInt64(ticks);
    }
}
