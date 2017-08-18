using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using NetCoreWs.Buffers.Unmanaged;
using NetCoreWs.WebSockets;

namespace NetCoreWs.Buffers.Experimental
{
    public class LockFreeUnmanagedByteBufProvider : IByteBufProvider
    {
        static private readonly int MemSegHeaderSize = MemorySegment.HeaderSize;

        private readonly int _pageSize;
        private readonly int _segmentSize;
        private readonly int _initialPagesCount;

        private readonly ConcurrentQueue<IntPtr> _memSegPtrs = new ConcurrentQueue<IntPtr>();
        private readonly IntPtr[] _pages;

        public LockFreeUnmanagedByteBufProvider(int pageSize, int segmentSize, int initialPagesCount)
        {
            _pageSize = pageSize;
            _segmentSize = segmentSize;
            _initialPagesCount = initialPagesCount;
            
            _pages = new IntPtr[initialPagesCount];
            
            Allocate();
        }

        ~LockFreeUnmanagedByteBufProvider()
        {
            foreach (IntPtr pagePtr in _pages)
            {
                Marshal.FreeCoTaskMem(pagePtr);
            }
        }
        
        public ByteBuf GetBuffer()
        {
            int size;
            IntPtr dataPtr = GetDefaultDataIntPtr(out size);
            MemorySegmentByteBuffer byteBuf = WrapMemorySegment(dataPtr, 0 /* filledSize */);

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
            size = _segmentSize - MemSegHeaderSize;
            IntPtr memSegPtr = GetDefaultIntPtrCore();
            return MemorySegment.GetDataPtr(memSegPtr);
        }

        internal void Release(IntPtr memSegPtr)
        {
            MemorySegment.SetNext(memSegPtr, IntPtr.Zero);
            MemorySegment.SetPrev(memSegPtr, IntPtr.Zero);
            MemorySegment.SetUsed(memSegPtr, 0);
            
            _memSegPtrs.Enqueue(memSegPtr);
        }
        
        internal void Release(MemorySegmentByteBuffer unmanagedByteBuf)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private MemorySegmentByteBuffer GetByteBufCore()
        {
            return new MemorySegmentByteBuffer(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IntPtr GetDefaultIntPtrCore()
        {
            IntPtr intPtr;

            var spin = new SpinWait();
            
            while (!_memSegPtrs.TryDequeue(out intPtr))
            {
                spin.SpinOnce();
            }

            MemorySegment.SetNext(intPtr, IntPtr.Zero);
            MemorySegment.SetPrev(intPtr, IntPtr.Zero);
            MemorySegment.SetSize(intPtr, _segmentSize - MemSegHeaderSize);
            MemorySegment.SetUsed(intPtr, _segmentSize - MemSegHeaderSize);

            return intPtr;
        }

        private void Allocate()
        {
            for (int i = 0; i < _initialPagesCount; i++)
            {
                _pages[i] = Marshal.AllocCoTaskMem(_pageSize);
                
                CreatePageSegmentsAndAddToPool(_pages[i]);
            }
        }

        private void CreatePageSegmentsAndAddToPool(IntPtr pagePtr)
        {
            int remainSize = _pageSize;

            int memSegOffset = 0;
            
            while (remainSize > _segmentSize)
            {
                IntPtr memSegPtr = IntPtr.Add(pagePtr, memSegOffset);

                _memSegPtrs.Enqueue(memSegPtr);
                
                memSegOffset += _segmentSize;
                remainSize -= _segmentSize;
            }
        }
    }
}