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
            
            var byteBuf = new UnmanagedByteBuf(memPtr, _bufDefaultSize);
            return byteBuf;
        }
    }
}