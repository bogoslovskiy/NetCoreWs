using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NetCoreWs.Buffers.Unmanaged
{
    // TODO: Выделять один большой сегмент памяти и из него раздавать нужные сегменты
    public class UnmanagedByteBufAllocator : IByteBufProvider
    {
        static private readonly int _memSegHeaderSize = MemorySegment.HeaderSize;
        private readonly int _defaultBufSize;

        // TODO: реализовать вменяемый пулинг
        // Чтобы минимизировать кэшмисы в процессоре, нужно чтобы сегменты памяти были максимально близко друг к другу.
        private readonly ConcurrentQueue<IntPtr> _defaultMemorySegments = new ConcurrentQueue<IntPtr>();
        private readonly ConcurrentQueue<ByteBuf> _byteBufs = new ConcurrentQueue<ByteBuf>();

        public UnmanagedByteBufAllocator(int defaultSize)
        {
            _defaultBufSize = defaultSize;
        }

        public ByteBuf GetBuffer()
        {
            int size;
            IntPtr dataPtr = GetDefaultDataIntPtr(out size);
            UnmanagedByteBuf byteBuf = (UnmanagedByteBuf)WrapMemorySegment(dataPtr, 0 /* filledSize */);

            return byteBuf;
        }

        public UnmanagedByteBuf WrapMemorySegment(IntPtr dataPtr, int filledSize)
        {
            IntPtr memSegPtr = MemorySegment.GetMemSegPtrByDataPtr(dataPtr);

            UnmanagedByteBuf byteBuf = GetByteBufCore();
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
        
        internal void Release(UnmanagedByteBuf unmanagedByteBuf)
        {
            _byteBufs.Enqueue(unmanagedByteBuf);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UnmanagedByteBuf GetByteBufCore()
        {
            ByteBuf byteBuf;
            if (!_byteBufs.TryDequeue(out byteBuf))
            {
                byteBuf = new UnmanagedByteBuf(this);
            }

            return (UnmanagedByteBuf) byteBuf;
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