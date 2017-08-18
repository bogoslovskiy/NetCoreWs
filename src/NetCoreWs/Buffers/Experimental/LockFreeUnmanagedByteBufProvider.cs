using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using NetCoreWs.Buffers.Unmanaged;

namespace NetCoreWs.Buffers.Experimental
{
//    public class LockFreeUnmanagedByteBufProvider : IByteBufProvider
//    {
//        class MemoryPoolNode
//        {
//            public IntPtr MemorySegmentPtr;
//            
//            public MemoryPoolNode Prev;
//            
//            public MemoryPoolNode Next;
//        }
//        
//        static private readonly int _memSegHeaderSize = MemorySegment.HeaderSize;
//
//        private readonly int _pageSize;
//        private readonly int _segmentSize;
//        private readonly int _initialPagesCount;
//
//        private volatile MemoryPoolNode _tailFree;
//        private volatile MemoryPoolNode _headFree;
//
//        private volatile MemoryPoolNode _tailPooled;
//        private volatile MemoryPoolNode _headPooled;
//
//        public LockFreeUnmanagedByteBufProvider(int segmentSize)
//        {
//            _segmentSize = segmentSize;
//        }
//
//
//
//        private IntPtr PopPooled()
//        {
//            var spin = new SpinWait();
//
//            MemoryPoolNode nodePooled;
//            
//            while (true)
//            {
//                nodePooled = _tailPooled;
//                
//                if (nodePooled == null)
//                {
//                    spin.SpinOnce();
//                    continue;
//                }
//
//                if (Interlocked.CompareExchange(ref _tailPooled, nodePooled.Next, nodePooled) == nodePooled)
//                {
//                    break;
//                }
//                
//                spin.SpinOnce();
//            }
//
//            IntPtr memSegPtr = nodePooled.MemorySegmentPtr;
//
//            nodePooled.MemorySegmentPtr = IntPtr.Zero;
//            PushFree(nodePooled);
//            
//            return memSegPtr;
//        }
//
//        private void PushFree(MemoryPoolNode node)
//        {
//            var spin = new SpinWait();
//
//            MemoryPoolNode headPooled;
//            
//            while (true)
//            {
//                head = m_head;
//                node.Next = head;
//                if (Interlocked.CompareExchange(ref m_head, node, head) == head) break;
//                spin.SpinOnce();
//            }
//        }
//        
//        
//        
//        
//        
//
//        public ByteBuf GetBuffer()
//        {
//            int size;
//            IntPtr dataPtr = GetDefaultDataIntPtr(out size);
//            MemorySegmentByteBuffer byteBuf = (MemorySegmentByteBuffer)WrapMemorySegment(dataPtr, 0 /* filledSize */);
//
//            return byteBuf;
//        }
//
//        public MemorySegmentByteBuffer WrapMemorySegment(IntPtr dataPtr, int filledSize)
//        {
//            IntPtr memSegPtr = MemorySegment.GetMemSegPtrByDataPtr(dataPtr);
//
//            MemorySegmentByteBuffer byteBuf = GetByteBufCore();
//            byteBuf.Attach(memSegPtr);
//            byteBuf.SetWrite(filledSize);
//
//            return byteBuf;
//        }
//
//        public IntPtr GetDefaultDataIntPtr(out int size)
//        {
//            size = _segmentSize - _memSegHeaderSize;
//            IntPtr memSegPtr = GetDefaultIntPtrCore();
//            return MemorySegment.GetDataPtr(memSegPtr);
//        }
//
//        internal void Release(IntPtr memSegPtr)
//        {
//            MemorySegment.SetNext(memSegPtr, IntPtr.Zero);
//            MemorySegment.SetPrev(memSegPtr, IntPtr.Zero);
//            MemorySegment.SetUsed(memSegPtr, 0);
//            
//            _defaultMemorySegments.Enqueue(memSegPtr);
//        }
//        
//        internal void Release(MemorySegmentByteBuffer unmanagedByteBuf)
//        {
//            _byteBufs.Enqueue(unmanagedByteBuf);
//        }
//
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        private MemorySegmentByteBuffer GetByteBufCore()
//        {
//            MemorySegmentByteBuffer byteBuf;
//            if (!_byteBufs.TryDequeue(out byteBuf))
//            {
//                byteBuf = new MemorySegmentByteBuffer(this);
//            }
//
//            return byteBuf;
//        }
//
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        private IntPtr GetDefaultIntPtrCore()
//        {
//            IntPtr intPtr;
//            if (!_defaultMemorySegments.TryDequeue(out intPtr))
//            {
//                intPtr = AllocDefault();
//            }
//
//            MemorySegment.SetNext(intPtr, IntPtr.Zero);
//            MemorySegment.SetPrev(intPtr, IntPtr.Zero);
//            MemorySegment.SetSize(intPtr, _segmentSize - _memSegHeaderSize);
//            MemorySegment.SetUsed(intPtr, _segmentSize - _memSegHeaderSize);
//
//            return intPtr;
//        }
//
//        private int _allocs;
//        
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        private IntPtr AllocDefault()
//        {
//            _allocs++;
//            Console.WriteLine($"Allocs:{_allocs}");
//            return Marshal.AllocCoTaskMem(_segmentSize);
//        }
//    }
}