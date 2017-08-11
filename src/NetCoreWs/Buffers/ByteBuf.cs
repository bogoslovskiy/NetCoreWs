﻿namespace NetCoreWs.Buffers
{
    abstract public class ByteBuf
    {
        abstract public void Release();

        abstract public int ReadableBytes();

        abstract public void Back(int offset);

        abstract public byte ReadByte();

        abstract public short ReadShort();

        abstract public ushort ReadUShort();

        abstract public int ReadInt();

        abstract public uint ReadUInt();

        abstract public long ReadLong();

        abstract public ulong ReadULong();

        abstract public int ReadToOrRollback(
            byte stopByte,
            byte[] output,
            int startIndex,
            int len);

        abstract public int ReadToOrRollback(
            byte stopByte1,
            byte stopByte2,
            byte[] output,
            int startIndex,
            int len);

        abstract public int SkipTo(byte stopByte, bool include);

        abstract public int SkipTo(byte stopByte1, byte stopByte2, bool include);

        abstract public int WritableBytes();

        abstract public void Write(byte value);

        abstract public string Dump(System.Text.Encoding encoding);

        // TODO: write*
    }
}