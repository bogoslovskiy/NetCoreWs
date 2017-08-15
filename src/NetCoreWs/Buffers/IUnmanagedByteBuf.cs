using System;

namespace NetCoreWs.Buffers
{
    public interface IUnmanagedByteBuf
    {
        void GetReadable(out IntPtr dataPtr, out int length);
    }
}