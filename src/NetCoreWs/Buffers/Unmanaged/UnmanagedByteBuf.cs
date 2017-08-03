using System;

namespace NetCoreWs.Buffers.Unmanaged
{
    public class UnmanagedByteBuf : ByteBuf
    {
        private IntPtr _memPtr;
        private int _memLen;
        unsafe private byte* _memDataPtr;
        private int _readed;
        private int _writed;
        
        public override int ReadableBytes()
        {
            throw new System.NotImplementedException();
        }

        public override byte ReadByte()
        {
            throw new System.NotImplementedException();
        }

        public override short ReadShort()
        {
            throw new System.NotImplementedException();
        }

        public override ushort ReadUShort()
        {
            throw new System.NotImplementedException();
        }

        public override int ReadInt()
        {
            throw new System.NotImplementedException();
        }

        public override uint ReadUInt()
        {
            throw new System.NotImplementedException();
        }

        public override long ReadLong()
        {
            throw new System.NotImplementedException();
        }

        public override ulong ReadULong()
        {
            throw new System.NotImplementedException();
        }

        public override ByteBuf SliceFromCurrentReadPosition(int len)
        {
            throw new System.NotImplementedException();
        }

        public override int WritableBytes()
        {
            throw new System.NotImplementedException();
        }

        public override void Write(byte value)
        {
            throw new System.NotImplementedException();
        }
    }
}