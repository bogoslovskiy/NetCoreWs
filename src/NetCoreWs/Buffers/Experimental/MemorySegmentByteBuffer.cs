using System;
using System.Runtime.CompilerServices;
using System.Text;
using NetCoreWs.Buffers.Unmanaged;
using NetCoreWs.Utils;

namespace NetCoreWs.Buffers.Experimental
{
    public class MemorySegmentByteBuffer : ByteBuf, IUnmanagedByteBuf
    {
        private readonly LockFreeUnmanagedByteBufProvider _byteBufProvider;
        
        private bool _released;
        
        private IntPtr _memSegPtr;
        unsafe private byte* _dataPtr;
        private int _memSegSize;
        private IntPtr _lastMemSegPtr;
        
        private int _writeIndex;
        private int _nextWrited;
        private int _readIndex;
        
        public override bool Released => _released;

        public MemorySegmentByteBuffer(LockFreeUnmanagedByteBufProvider byteBufProvider)
        {
            _byteBufProvider = byteBufProvider;
        }
        
        // TODO:
        ~MemorySegmentByteBuffer()
        {
            // Пул объектов такого типа должен быть устроен таким образом,
            // чтобы не хранить ссылку на отданный объект.
            // Таким образом, если текущий объект забыли отдать в пул, не должно быть
            // ссылок на него, чтобы сборщик его почистил, а при финализации объект мог
            // отдать неуправляемый ресурс обратно в пул.
            
            // Если "какой-то" сторонний объект не отдал данный объект в пул удерживает ссылку на него, то
            // мы не можем контролировать такие утечки, они полностью на совести "стороннего" кода.
            
            // Возвращаем куски памяти из неуправляемой кучи в пул.
            //ReleaseMemorySegments();
            
            // Сам объект в пул вернуть не можем, т.к. он уничтожается сборщиком.
        }
        
        public void Attach(IntPtr memSegPtr)
        {
            _memSegPtr = memSegPtr;
            _memSegSize = MemorySegment.GetSize(_memSegPtr);
            unsafe
            {
                _dataPtr = (byte*) (void*) MemorySegment.GetDataPtr(_memSegPtr);
            }
            _lastMemSegPtr = memSegPtr;

            _writeIndex = MemorySegment.GetUsed(memSegPtr) - 1;
            _nextWrited = 0;
            _readIndex = -1;
            _released = false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetWrite(int write)
        {
            MemorySegment.SetUsed(_memSegPtr, write);
            _writeIndex = write - 1;
        }
        
        public void GetReadable(out IntPtr dataPtr, out int length)
        {
            dataPtr = MemorySegment.GetDataPtr(_memSegPtr);
            length = _writeIndex + 1;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Append(ByteBuf byteBuf)
        {
            var memSegByteBuf = (MemorySegmentByteBuffer) byteBuf;

            IntPtr next = memSegByteBuf._memSegPtr;

            MemorySegment.SetNext(_lastMemSegPtr, next);
            MemorySegment.SetPrev(next, _lastMemSegPtr);

            _lastMemSegPtr = memSegByteBuf._lastMemSegPtr;

            _nextWrited += memSegByteBuf._nextWrited + MemorySegment.GetUsed(next);

            _byteBufProvider.Release(memSegByteBuf);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Release()
        {
            ReleaseReaded();

            IntPtr next = _memSegPtr;
            while (next != IntPtr.Zero)
            {
                IntPtr toRelease = next;
                next = MemorySegment.GetNext(next);
                
                MemorySegment.SetPrev(toRelease, IntPtr.Zero);
                MemorySegment.SetNext(toRelease, IntPtr.Zero);
                
                MemorySegment.ClearBeforeRelease(toRelease);
                _byteBufProvider.Release(toRelease);
            }
            
            _byteBufProvider.Release(this);
            _released = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void ReleaseReaded()
        {
            IntPtr prev = MemorySegment.GetPrev(_memSegPtr);
            
            // Отвязываем от текущего.
            MemorySegment.SetPrev(_memSegPtr, IntPtr.Zero);
            
            while (prev != IntPtr.Zero)
            {
                IntPtr toRelease = prev;
                prev = MemorySegment.GetPrev(prev);
                
                MemorySegment.SetPrev(toRelease, IntPtr.Zero);
                MemorySegment.SetNext(toRelease, IntPtr.Zero);
                
                MemorySegment.ClearBeforeRelease(toRelease);
                _byteBufProvider.Release(toRelease);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int ReadableBytes()
        {
            ThrowIfReleased();
            
            return _writeIndex - _readIndex + _nextWrited;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Back(int offset)
        {
            ThrowIfReleased();
            
            SeekBackward(offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe public override byte ReadByte()
        {
            ThrowIfReleased();
            
			if (_readIndex < _writeIndex)
			{
			    _readIndex++;
			    return _dataPtr[_readIndex];
			}
            
            SeekForward(1);

            return _dataPtr[_readIndex];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override short ReadShort()
        {
            ThrowIfReleased();
            
            byte b1 = ReadByte();
            byte b2 = ReadByte();

            return ByteConverters.GetShort(b1, b2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ushort ReadUShort()
        {
            ThrowIfReleased();
            
            byte b1 = ReadByte();
            byte b2 = ReadByte();

            return ByteConverters.GetUShort(b1, b2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int ReadInt()
        {
            ThrowIfReleased();
            
            byte b1 = ReadByte();
            byte b2 = ReadByte();
            byte b3 = ReadByte();
            byte b4 = ReadByte();

            return ByteConverters.GetInt(b1, b2, b3, b4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override uint ReadUInt()
        {
            ThrowIfReleased();
            
            byte b1 = ReadByte();
            byte b2 = ReadByte();
            byte b3 = ReadByte();
            byte b4 = ReadByte();

            return ByteConverters.GetUInt(b1, b2, b3, b4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override long ReadLong()
        {
            ThrowIfReleased();
            
            byte b1 = ReadByte();
            byte b2 = ReadByte();
            byte b3 = ReadByte();
            byte b4 = ReadByte();
            byte b5 = ReadByte();
            byte b6 = ReadByte();
            byte b7 = ReadByte();
            byte b8 = ReadByte();

            return ByteConverters.GetLong(b1, b2, b3, b4, b5, b6, b7, b8);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ulong ReadULong()
        {
            ThrowIfReleased();
            
            byte b1 = ReadByte();
            byte b2 = ReadByte();
            byte b3 = ReadByte();
            byte b4 = ReadByte();
            byte b5 = ReadByte();
            byte b6 = ReadByte();
            byte b7 = ReadByte();
            byte b8 = ReadByte();

            return ByteConverters.GetULong(b1, b2, b3, b4, b5, b6, b7, b8);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int ReadToOrRollback(byte stopByte, byte[] output, int startIndex, int len)
        {
            ThrowIfReleased();
            
            bool stopByteMatched = false;

            int readed = 0;
            int allReaded = 0;
            
            while (ReadableBytes() > 0)
            {
                allReaded++;
                byte currentByte = ReadByte();

                if (currentByte == stopByte)
                {
                    stopByteMatched = true;
                    break;
                }

                output[startIndex] = currentByte;
                readed++;
                startIndex++;

                if (startIndex == len)
                {
                    throw new Exception();
                }
            }

            if (stopByteMatched)
            {
                SeekBackward(1);
                
                return readed;
            }

            SeekBackward(allReaded);
            
            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int ReadToOrRollback(byte stopByte1, byte stopByte2, byte[] output, int startIndex, int len)
        {
            ThrowIfReleased();
            
            bool stopByte1Matched = false;
            bool stopBytesMatched = false;

            int readed = 0;
            int allReaded = 0;

            while (ReadableBytes() > 0)
            {
                allReaded++;
                byte currentByte = ReadByte();
                
                if (currentByte == stopByte2)
                {
                    if (stopByte1Matched)
                    {
                        stopBytesMatched = true;
                        break;
                    }
                }
                if (currentByte == stopByte1)
                {
                    stopByte1Matched = true;
                    continue;
                }

                stopByte1Matched = false;

                output[startIndex] = currentByte;
                readed++;
                startIndex++;

                if (startIndex == len)
                {
                    throw new Exception();
                }
            }

            if (stopBytesMatched)
            {
                SeekBackward(2);
                
                return readed;
            }

            SeekBackward(allReaded);
            
            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int SkipTo(byte stopByte, bool include)
        {
            ThrowIfReleased();
            
            int skipped = 0;

            bool stopByteMatched = false;

            while (ReadableBytes() > 0)
            {
                skipped++;
                byte currentByte = ReadByte();

                if (currentByte == stopByte)
                {
                    stopByteMatched = true;
                    break;
                }
            }

            if (stopByteMatched)
            {
                if (!include)
                {
                    skipped -= 1;
                    SeekBackward(1);
                }

                return skipped;
            }

            SeekBackward(skipped);
            
            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int SkipTo(byte stopByte1, byte stopByte2, bool include)
        {
            ThrowIfReleased();
            
            int skipped = 0;

            bool stopByte1Matched = false;
            bool stopBytesMatched = false;

            while (ReadableBytes() > 0)
            {
                skipped++;
                byte currentByte = ReadByte();

                if (currentByte == stopByte2)
                {
                    if (stopByte1Matched)
                    {
                        stopBytesMatched = true;
                        break;
                    }
                }
                if (currentByte == stopByte1)
                {
                    stopByte1Matched = true;
                    continue;
                }

                stopByte1Matched = false;
            }

            if (stopBytesMatched)
            {
                if (!include)
                {
                    skipped -= 2;
                    SeekBackward(2);
                }

                return skipped;
            }
            
            SeekBackward(skipped);
            
            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int WritableBytes()
        {
            ThrowIfReleased();
            
            // TODO: Запись пока что невозможна в цепочку сегментов.
            return _memSegSize - _writeIndex - 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe public override void Write(byte @byte)
        {
            ThrowIfReleased();
            
            // TODO: Запись пока что невозможна в цепочку сегментов.
            if (_memSegSize - 1 == _writeIndex)
            {
                throw new InvalidOperationException();
            }
            
            _writeIndex++;
            _dataPtr[_writeIndex] = @byte;
        }

        unsafe public override string Dump(Encoding encoding)
        {
            int readable = ReadableBytes();

            int index = _readIndex;

            byte[] bytes = new byte[readable];

            int i = 0;
            while (i < readable)
            {
                index++;
                bytes[i] = _dataPtr[index];
                i++;
            }

            return encoding.GetString(bytes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowIfReleased()
        {
            if (_released)
            {
                throw new Exception("Buffer has been released.");
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SeekForward(int offset)
        {
            IntPtr memSegPtr = _memSegPtr;
            
            int writeIndex = _writeIndex;
            int nextWrited = _nextWrited;
            int readIndex = _readIndex + offset;

            while (readIndex > writeIndex)
            {
                memSegPtr = MemorySegment.GetNext(memSegPtr);
                if (memSegPtr == IntPtr.Zero)
                {
                    throw new UnexpectedEndOfBufferException();
                }
                int used = MemorySegment.GetUsed(memSegPtr);

                readIndex -= writeIndex + 1;

                writeIndex = used - 1;
                nextWrited -= used;
            }

            _memSegPtr = memSegPtr;
            _memSegSize = MemorySegment.GetSize(_memSegPtr);
            unsafe
            {
                _dataPtr = (byte*) (void*) MemorySegment.GetDataPtr(_memSegPtr);
            }
            _writeIndex = writeIndex;
            _nextWrited = nextWrited;
            _readIndex = readIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SeekBackward(int offset)
        {
            IntPtr memSegPtr = _memSegPtr;
            
            int writeIndex = _writeIndex;
            int nextWrited = _nextWrited;
            int readIndex = _readIndex - offset;

            while (readIndex < -1)
            {
                nextWrited += MemorySegment.GetUsed(memSegPtr);
                
                memSegPtr = MemorySegment.GetPrev(memSegPtr);
                if (memSegPtr == IntPtr.Zero)
                {
                    throw new UnexpectedEndOfBufferException();
                }
                writeIndex = MemorySegment.GetUsed(memSegPtr) - 1;
                readIndex += writeIndex;
            }

            _memSegPtr = memSegPtr;
            _memSegSize = MemorySegment.GetSize(_memSegPtr);
            unsafe
            {
                _dataPtr = (byte*) (void*) MemorySegment.GetDataPtr(_memSegPtr);
            }
            _writeIndex = writeIndex;
            _nextWrited = nextWrited;
            _readIndex = readIndex;
        }
    }
}