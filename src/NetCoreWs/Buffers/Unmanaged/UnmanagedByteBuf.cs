using System;
using System.Runtime.CompilerServices;
using NetCoreWs.Utils;

namespace NetCoreWs.Buffers.Unmanaged
{
    public class UnmanagedByteBuf : ByteBuf
    {
        private readonly UnmanagedByteBufProvider _provider;

        private IntPtr _memorySegmentPointer;
        private int _memorySegmentSize;
        unsafe private byte* _memorySegmentBytePointer;

        private int _readed;
        private int _writed;

        public UnmanagedByteBuf(UnmanagedByteBufProvider provider)
        {
            _provider = provider;
        }

        ~UnmanagedByteBuf()
        {
            // Пул объектов такого типа должен быть устроен таким образом,
            // чтобы не хранить ссылку на отданный объект.
            // Таким образом, если текущий объект забыли отдать в пул, не должно быть
            // ссылок на него, чтобы сборщик его почистил, а при финализации объект мог
            // отдать неуправляемый ресурс обратно в пул.

            // Если "какой-то" сторонний объект не отдал данный объект в пул удерживает ссылку на него, то
            // мы не можем контролировать такие утечки, они полностью на совести "стороннего" кода.

            // Возвращаем куски памяти из неуправляемой кучи в пул.
            ReleaseMemorySegment();

            // Сам объект в пул вернуть не можем, т.к. он уничтожается сборщиком.
        }

        public void Attach(IntPtr memSegPtr, int len)
        {
            _memorySegmentPointer = memSegPtr;
            unsafe
            {
                _memorySegmentBytePointer = (byte*) (void*) memSegPtr;
            }
            _memorySegmentSize = len;
            _readed = -1;
            _writed = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetReadable(out IntPtr dataPtr, out int length)
        {
            // Учитывая то, кто использует этот метод, тут никогда не будет других чтений и никогда не будет больше
            // одного сегмента.
            length = _writed + 1;
            dataPtr = _memorySegmentPointer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetWrite(int write)
        {
            _writed = write - 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Release()
        {
            // Разбор (можно называть это деконструкцией) объекта и возврат в пул его составляющих осуществляет
            // сам объект. Пул должен принимать только те куски на возврат, что ему отдают.
            
            // Возвращаем куски памяти из неуправляемой кучи в пул.
            ReleaseMemorySegment();

            ReleaseCore();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int ReadableBytes()
        {
            return _writed - _readed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Back(int offset)
        {
            _readed -= offset;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe public override byte ReadByte()
        {
            _readed++;

            return _memorySegmentBytePointer[_readed];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override short ReadShort()
        {
            byte b1 = ReadByte();
            byte b2 = ReadByte();

            return ByteConverters.GetShort(b1, b2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ushort ReadUShort()
        {
            byte b1 = ReadByte();
            byte b2 = ReadByte();

            return ByteConverters.GetUShort(b1, b2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int ReadInt()
        {
            byte b1 = ReadByte();
            byte b2 = ReadByte();
            byte b3 = ReadByte();
            byte b4 = ReadByte();

            return ByteConverters.GetInt(b1, b2, b3, b4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override uint ReadUInt()
        {
            byte b1 = ReadByte();
            byte b2 = ReadByte();
            byte b3 = ReadByte();
            byte b4 = ReadByte();

            return ByteConverters.GetUInt(b1, b2, b3, b4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override long ReadLong()
        {
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
            bool stopByteMatched = false;

            int allReaded = 0;
            int readed = 0;

            while (ReadableBytes() > 0)
            {
                byte currentByte = ReadByte();
                allReaded++;

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
                Back(1);
                return readed;
            }

            Back(allReaded);

            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int ReadToOrRollback(byte stopByte1, byte stopByte2, byte[] output, int startIndex, int len)
        {
            bool stopByte1Matched = false;
            bool stopBytesMatched = false;

            int allReaded = 0;
            int readed = 0;

            while (ReadableBytes() > 0)
            {
                byte currentByte = ReadByte();
                allReaded++;

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
                Back(2);

                return readed;
            }

            Back(allReaded);

            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int SkipTo(byte stopByte, bool include)
        {
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
                    Back(1);
                }

                return skipped;
            }

            Back(skipped);

            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int SkipTo(byte stopByte1, byte stopByte2, bool include)
        {
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
                    Back(2);
                }

                return skipped;
            }

            Back(skipped);

            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int WritableBytes()
        {
            return _memorySegmentSize - _writed - 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe public override void Write(byte @byte)
        {
            _writed++;

            _memorySegmentBytePointer[_writed] = @byte;
        }

        unsafe public override string Dump(System.Text.Encoding encoding)
        {
            int readable = ReadableBytes();

            int index = _readed;

            byte[] bytes = new byte[readable];

            int i = 0;
            while (i < readable)
            {
                index++;
                bytes[i] = _memorySegmentBytePointer[index];
                i++;
            }

            return encoding.GetString(bytes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReleaseCore()
        {
            // Инициализируем поля пустыми значениями.
            Clear();

            // Теперь возвращем в пул сам объект.
            _provider.ReleaseWrapper(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReleaseMemorySegment()
        {
            _provider.ReleaseMemSeg(_memorySegmentPointer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Clear()
        {
            _memorySegmentPointer = IntPtr.Zero;
            unsafe
            {
                _memorySegmentBytePointer = (byte*) (void*) IntPtr.Zero;
            }
            _memorySegmentSize = 0;
            _readed = 0;
            _writed = 0;
        }
    }
}