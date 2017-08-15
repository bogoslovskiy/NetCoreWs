using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using NetCoreWs.Buffers.Unmanaged;

namespace NetCoreWs.Buffers.Experimental
{
    public class LockFreeUnmanagedByteBufProvider : IByteBufProvider
    {
        static private readonly int _memSegHeaderSize = MemorySegment.HeaderSize;
        private readonly int _defaultBufSize;

        // TODO: реализовать вменяемый пулинг
        // Чтобы минимизировать кэшмисы в процессоре, нужно чтобы сегменты памяти были максимально близко друг к другу.
        private readonly ConcurrentQueue<IntPtr> _defaultMemorySegments = new ConcurrentQueue<IntPtr>();
        private readonly ConcurrentQueue<MemorySegmentByteBuffer> _byteBufs = new ConcurrentQueue<MemorySegmentByteBuffer>();

        public LockFreeUnmanagedByteBufProvider(int defaultSize)
        {
            _defaultBufSize = defaultSize;
        }

        public ByteBuf GetBuffer()
        {
            int size;
            IntPtr dataPtr = GetDefaultDataIntPtr(out size);
            MemorySegmentByteBuffer byteBuf = (MemorySegmentByteBuffer)WrapMemorySegment(dataPtr, 0 /* filledSize */);

            return byteBuf;
        }

        public MemorySegmentByteBuffer WrapMemorySegment(IntPtr dataPtr, int filledSize)
        {
            IntPtr memSegPtr = MemorySegment.GetMemSegPtrByDataPtr(dataPtr);

            MemorySegmentByteBuffer byteBuf = GetByteBufCore();
            byteBuf.Attach(memSegPtr);
            byteBuf.SetWrite(filledSize);

            return byteBuf;
        }

        public IntPtr GetDefaultDataIntPtr(out int size)
        {
            size = _defaultBufSize - _memSegHeaderSize;
            IntPtr memSegPtr = GetDefaultIntPtrCore();
            return MemorySegment.GetDataPtr(memSegPtr);
        }

        internal void Release(IntPtr memSegPtr)
        {
            _defaultMemorySegments.Enqueue(memSegPtr);
        }
        
        internal void Release(MemorySegmentByteBuffer unmanagedByteBuf)
        {
            _byteBufs.Enqueue(unmanagedByteBuf);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private MemorySegmentByteBuffer GetByteBufCore()
        {
            MemorySegmentByteBuffer byteBuf;
            if (!_byteBufs.TryDequeue(out byteBuf))
            {
                byteBuf = new MemorySegmentByteBuffer(this);
            }

            return byteBuf;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IntPtr GetDefaultIntPtrCore()
        {
            IntPtr intPtr;
            if (!_defaultMemorySegments.TryDequeue(out intPtr))
            {
                intPtr = AllocDefault();
            }

            MemorySegment.SetNext(intPtr, IntPtr.Zero);
            MemorySegment.SetPrev(intPtr, IntPtr.Zero);
            MemorySegment.SetSize(intPtr, _defaultBufSize - _memSegHeaderSize);
            MemorySegment.SetUsed(intPtr, _defaultBufSize - _memSegHeaderSize);

            return intPtr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IntPtr AllocDefault()
        {
            return Marshal.AllocCoTaskMem(_defaultBufSize);
        }
    }
}