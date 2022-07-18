namespace Pekspro.RadioStorm.Utilities;

public class ReverseBinaryReader
{
    public BinaryReader BinaryReader { get; }

    public ReverseBinaryReader(BinaryReader binaryReader)
    {
        BinaryReader = binaryReader;
    }

    public long ReadInt64()
    {
        return BinaryPrimitives.ReverseEndianness(BinaryReader.ReadInt64());
    }

    public int ReadInt32()
    {
        return BinaryPrimitives.ReverseEndianness(BinaryReader.ReadInt32());
    }

    public short ReadInt16()
    {
        return BinaryPrimitives.ReverseEndianness(BinaryReader.ReadInt16());
    }

    public ulong ReadUInt64()
    {
        return BinaryPrimitives.ReverseEndianness(BinaryReader.ReadUInt64());
    }

    public uint ReadUInt32()
    {
        return BinaryPrimitives.ReverseEndianness(BinaryReader.ReadUInt32());
    }

    public ushort ReadUInt16()
    {
        return BinaryPrimitives.ReverseEndianness(BinaryReader.ReadUInt16());
    }

    public byte ReadByte()
    {
        return BinaryReader.ReadByte();
    }

    public bool ReadBoolean()
    {
        return BinaryReader.ReadBoolean();
    }

    public DateTimeOffset ReadDateTime()
    {
        long ticks = ReadInt64();
        var date = new DateTimeOffset(1601, 1, 1, 0, 0, 0, TimeSpan.Zero);
        date = date.AddTicks(ticks).ToLocalTime();

        return date;
    }
}
