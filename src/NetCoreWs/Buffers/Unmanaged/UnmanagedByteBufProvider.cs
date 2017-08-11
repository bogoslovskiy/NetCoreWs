using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NetCoreWs.Buffers.Unmanaged
{
    public class UnmanagedByteBufProvider : IByteBufProvider
    {
        private readonly int _bufDefaultSize;

        private readonly ConcurrentQueue<IntPtr> _memorySegments =
            new ConcurrentQueue<IntPtr>();
        private readonly ConcurrentQueue<UnmanagedByteBuf> _wrappers =
            new ConcurrentQueue<UnmanagedByteBuf>();

        public UnmanagedByteBufProvider(int bufDefaultSize)
        {
            _bufDefaultSize = bufDefaultSize;
        }

        public ByteBuf GetBuffer()
        {
            IntPtr memPtr = GetMemorySegment();
            UnmanagedByteBuf byteBuf = GetWrapper();
            
            byteBuf.Attach(memPtr, _bufDefaultSize);
            
            return byteBuf;
        }

        public IntPtr GetUnwrappedMemSegPtr()
        {
            return GetMemorySegment();
        }

        public UnmanagedByteBuf WrapMemorySegment(IntPtr memSegPtr, int len)
        {
            var byteBuf = GetWrapper();
            
            byteBuf.Attach(memSegPtr, len);
            
            return byteBuf;
        }

        public void ReleaseMemSeg(IntPtr memSegPtr)
        {
            _memorySegments.Enqueue(memSegPtr);
        }

        public void ReleaseWrapper(UnmanagedByteBuf wrapper)
        {
            _wrappers.Enqueue(wrapper);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UnmanagedByteBuf GetWrapper()
        {
            if (_wrappers.TryDequeue(out UnmanagedByteBuf wrapper))
            {
                return wrapper;
            }
            
            return new UnmanagedByteBuf(this);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IntPtr GetMemorySegment()
        {
            if (_memorySegments.TryDequeue(out IntPtr memorySegmentPointer))
            {
                return memorySegmentPointer;
            }
            
            return Marshal.AllocCoTaskMem(_bufDefaultSize);
        }
    }
}