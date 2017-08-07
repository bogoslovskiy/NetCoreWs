using System;
using System.Runtime.InteropServices;

namespace NetCoreWs.Buffers.Unmanaged
{
    public class UnmanagedByteBufProvider : IByteBufProvider
    {
        private readonly int _bufDefaultSize;

        public UnmanagedByteBufProvider(int bufDefaultSize)
        {
            _bufDefaultSize = bufDefaultSize;
        }

        public ByteBuf GetBuffer()
        {
            IntPtr memPtr = Marshal.AllocCoTaskMem(_bufDefaultSize);
            
            var byteBuf = new UnmanagedByteBuf(this);
            byteBuf.Attach(memPtr, _bufDefaultSize);
            return byteBuf;
        }

        public IntPtr GetUnwrappedMemSegPtr()
        {
            IntPtr memPtr = Marshal.AllocCoTaskMem(_bufDefaultSize);
            return memPtr;
        }

        public UnmanagedByteBuf WrapMemorySegment(IntPtr memSegPtr, int len)
        {
            var byteBuf = new UnmanagedByteBuf(this);
            byteBuf.Attach(memSegPtr, len);
            return byteBuf;
        }

        public void ReleaseMemSeg(IntPtr memSegPtr)
        {
            throw new NotImplementedException();
        }

        public void ReleaseWrapper(UnmanagedByteBuf byteBuf)
        {
            throw new NotImplementedException();
        }
    }
}