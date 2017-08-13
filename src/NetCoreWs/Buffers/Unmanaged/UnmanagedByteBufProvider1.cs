using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NetCoreWs.Buffers.Unmanaged
{
    public class UnmanagedByteBufProvider1 : IByteBufProvider
    {
        private readonly int _bufDefaultSize;

        private readonly ConcurrentQueue<IntPtr> _memorySegments =
            new ConcurrentQueue<IntPtr>();
        private readonly ConcurrentQueue<UnmanagedByteBuf1> _wrappers =
            new ConcurrentQueue<UnmanagedByteBuf1>();

        public UnmanagedByteBufProvider1(int bufDefaultSize)
        {
            _bufDefaultSize = bufDefaultSize;
        }

        public ByteBuf GetBuffer()
        {
            IntPtr memPtr = GetMemorySegment();
            UnmanagedByteBuf1 byteBuf = GetWrapper();
            
            byteBuf.Attach(memPtr, _bufDefaultSize);
            
            return byteBuf;
        }

        public IntPtr GetUnwrappedMemSegPtr()
        {
            return GetMemorySegment();
        }

        public UnmanagedByteBuf1 WrapMemorySegment(IntPtr memSegPtr, int len)
        {
            var byteBuf = GetWrapper();
            
            byteBuf.Attach(memSegPtr, len);
            
            return byteBuf;
        }

        public void ReleaseMemSeg(IntPtr memSegPtr)
        {
            _memorySegments.Enqueue(memSegPtr);
        }

        public void ReleaseWrapper(UnmanagedByteBuf1 wrapper)
        {
            _wrappers.Enqueue(wrapper);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UnmanagedByteBuf1 GetWrapper()
        {
            if (_wrappers.TryDequeue(out UnmanagedByteBuf1 wrapper))
            {
                return wrapper;
            }
            
            return new UnmanagedByteBuf1(this);
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