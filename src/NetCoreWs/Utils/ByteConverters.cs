using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NetCoreWs.Utils
{
    static public class ByteConverters
    {
        [StructLayout(LayoutKind.Explicit)]
        public struct ByteUnion2
        {
            [FieldOffset(0)]
            public short Short;

            [FieldOffset(0)]
            public ushort UShort;

            [FieldOffset(0)]
            public char Char;

            [FieldOffset(0)]
            public byte B1;

            [FieldOffset(1)]
            public byte B2;
        }
        
        [StructLayout(LayoutKind.Explicit)]
        public struct ByteUnion4
        {
            [FieldOffset(0)]
            public int Int;

            [FieldOffset(0)]
            public uint UInt;

            [FieldOffset(0)]
            public float Float;

            [FieldOffset(0)]
            public byte B1;

            [FieldOffset(1)]
            public byte B2;

            [FieldOffset(2)]
            public byte B3;

            [FieldOffset(3)]
            public byte B4;
        }
        
        [StructLayout(LayoutKind.Explicit)]
        public struct ByteUnion8
        {
            [FieldOffset(0)]
            public long Long;

            [FieldOffset(0)]
            public ulong ULong;

            [FieldOffset(0)]
            public double Double;

            [FieldOffset(0)]
            public byte B1;

            [FieldOffset(1)]
            public byte B2;

            [FieldOffset(2)]
            public byte B3;

            [FieldOffset(3)]
            public byte B4;

            [FieldOffset(4)]
            public byte B5;

            [FieldOffset(5)]
            public byte B6;

            [FieldOffset(6)]
            public byte B7;

            [FieldOffset(7)]
            public byte B8;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public short GetShort(byte b1, byte b2)
        {
            short a = b1;
            a = (short)(a << 8 | b2);
            return a;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public ushort GetUShort(byte b1, byte b2)
        {
            ushort a = b1;
            a = (ushort)(a << 8 | b2);
            return a;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int GetInt(byte b1, byte b2, byte b3, byte b4)
        {
            int a = b1;
            a = a << 8 | b2;
            a = a << 8 | b3;
            a = a << 8 | b4;
            return a;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public uint GetUInt(byte b1, byte b2, byte b3, byte b4)
        {
            uint a = b1;
            a = a << 8 | b2;
            a = a << 8 | b3;
            a = a << 8 | b4;
            return a;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public long GetLong(byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7, byte b8)
        {
            long a = b1;
            a = a << 8 | b2;
            a = a << 8 | b3;
            a = a << 8 | b4;
            a = a << 8 | b5;
            a = a << 8 | b6;
            a = a << 8 | b7;
            a = a << 8 | b8;
            return a;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public ulong GetULong(byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7, byte b8)
        {
            ulong a = b1;
            a = a << 8 | b2;
            a = a << 8 | b3;
            a = a << 8 | b4;
            a = a << 8 | b5;
            a = a << 8 | b6;
            a = a << 8 | b7;
            a = a << 8 | b8;
            return a;
        }
    }
}