using System;
using System.Runtime.CompilerServices;
using NetCoreWs.Utils;

namespace NetCoreWs.Buffers.Unmanaged
{
    public class UnmanagedByteBuf : ByteBuf
    {
        private readonly UnmanagedByteBufProvider _provider;
        
        private IntPtr _memSegPtr;
        private int _memSegSize;
        unsafe private byte* _memSegDataPtr;

        private int _readed;
        private int _writed;
        
        public UnmanagedByteBuf(UnmanagedByteBufProvider provider)
        {
            _provider = provider;
        }
        
//        _memSegSize = len;
//        
//        _memSegPtr = memPtr;
//        unsafe
//        {
//            _memSegDataPtr = (byte*) (void*) _memSegPtr;
//        }
//        _readed = -1;
//        _writed = -1;
        
//        public UnmanagedByteBuf(UnmanagedByteBufAllocator allocator)
//        {
//            _allocator = allocator;
//        }

//        ~UnmanagedByteBuf()
//        {
//            // Пул объектов такого типа должен быть устроен таким образом,
//            // чтобы не хранить ссылку на отданный объект.
//            // Таким образом, если текущий объект забыли отдать в пул, не должно быть
//            // ссылок на него, чтобы сборщик его почистил, а при финализации объект мог
//            // отдать неуправляемый ресурс обратно в пул.
//            
//            // Если "какой-то" сторонний объект не отдал данный объект в пул удерживает ссылку на него, то
//            // мы не можем контролировать такие утечки, они полностью на совести "стороннего" кода.
//            
//            // Возвращаем куски памяти из неуправляемой кучи в пул.
//            ReleaseMemorySegments();
//            
//            // Сам объект в пул вернуть не можем, т.к. он уничтожается сборщиком.
//        }

        public void Attach(IntPtr memSegPtr, int len)
        {
            _memSegPtr = memSegPtr;
            unsafe
            {
                _memSegDataPtr = (byte*) (void*) memSegPtr;
            }
            _memSegSize = len;
            _readed = -1;
            _writed = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetReadable(out IntPtr dataPtr, out int length)
        {
            // Учитывая то, кто использует этот метод, тут никогда не будет других чтений и никогда не будет больше
            // одного сегмента.
            length = _writed + 1;
            dataPtr = _memSegPtr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetWrite(int write)
        {
            _writed = write - 1;
        }

//        // TODO: Проверки (например, что присоединяем буфер, который не начали читать).
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public override void Append(ByteBuf byteBuf)
//        {
//            UnmanagedByteBuf unmanagedByteBuf = (UnmanagedByteBuf) byteBuf;
//
//            IntPtr appendixCurrentMemSegPtr = unmanagedByteBuf._memSegPtr;
//            IntPtr appendixLastMemSegPtr = unmanagedByteBuf._lastMemSegPtr;
//
//            MemorySegment.SetNext(_lastMemSegPtr, appendixCurrentMemSegPtr);
//            MemorySegment.SetPrev(appendixCurrentMemSegPtr, _lastMemSegPtr);
//
//            _lastMemSegPtr = appendixLastMemSegPtr;
//            _globalWrited += unmanagedByteBuf._globalWrited;
//            
//            // Т.к. мы все забрали у присоединяемого буфера, то буфер как обертка больше не нужен.
//            // Освобождаем его.
//            unmanagedByteBuf.ReleaseCore();
//        }

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public override void Release()
//        {
//            // Разбор (можно называть это деконструкцией) объекта и возврат в пул его составляющих осуществляет
//            // сам объект. Пул должен принимать только те куски на возврат, что ему отдают.
//            
//            // Возвращаем куски памяти из неуправляемой кучи в пул.
//            ReleaseMemorySegments();
//
//            ReleaseCore();
//        }
//
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public override void ReleaseReaded()
//        {
//            // Вычисляем, начиная с какого сегмента можно освободить прочитанную цепочку сегментов.
//            // Тут возможны 2 варианта:
//            // - Весь буфер уже прочитан, тогда можно освободить все.
//            // - Буфер прочитан не полностью, тогда можно освободить все предыдущие, а текущий оставить.
//            
//            // Сценарий с освобождением всей цепочки требует более тщательной реализации и пока невозможен,
//            // поэтому всегда будем освобождать только начиная с предыдущего.
//            // Дело в том, что у буфера пока не может не быть какого-то сегмента, а при освобождении всех именно так
//            // и получится, но сам буфер может использоваться для аккумуляции дальше. Будет реализовано потом, если
//            // потребуется.
//
//            IntPtr memSegPtr = MemorySegment.GetPrev(_memSegPtr);
//            
//            // Отвязываем от текущего.
//            MemorySegment.SetPrev(_memSegPtr, IntPtr.Zero);
//            
//            ReleaseMemorySegmentsAt(memSegPtr);
//        }

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

            return _memSegDataPtr[_readed];
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

        public override int WritableBytes()
        {
            return _memSegSize - _writed - 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe public override void Write(byte @byte)
        {
            _writed++;

            _memSegDataPtr[_writed] = @byte;
        }

        unsafe public override string Dump(System.Text.Encoding encoding)
        {
            int readable = ReadableBytes();

            int index = _readed;

            byte[] bytes = new byte[readable];

            int i = 0;
            while(i < readable)
            {
				index++;
                bytes[i] = _memSegDataPtr[index];
                i++;
            }

            return encoding.GetString(bytes);
        }

        //        
        //        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //        private void ReleaseCore()
        //        {
        //            // Инициализируем поля пустыми значениями.
        //            Clear();
        //            
        //            // Теперь возвращем в пул сам объект.
        //            _allocator.Release(this);
        //        }
        //        
        //        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //        private void ReleaseMemorySegments()
        //        {
        //            // Для освобождения всей цепочки сегментов памяти указываем стартовый сегмент - последний.
        //            ReleaseMemorySegmentsAt(_lastMemSegPtr);
        //        }
        //        
        //        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //        private void ReleaseMemorySegmentsAt(IntPtr memSegPtr)
        //        {
        //            // Проходим от указанного сегмента по связям с предыдущими.
        //            while (memSegPtr != IntPtr.Zero)
        //            {
        //                IntPtr releaseMemSegPtr = memSegPtr;
        //                memSegPtr = MemorySegment.GetPrev(releaseMemSegPtr);
        //                
        //                // Чистим связи с другими сегментами.
        //                MemorySegment.SetPrev(releaseMemSegPtr, IntPtr.Zero);
        //                MemorySegment.SetNext(releaseMemSegPtr, IntPtr.Zero);
        //                
        //                _allocator.Release(releaseMemSegPtr);
        //            }
        //        }
        //
        //        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //        private void Clear()
        //        {
        //            _memSegPtr = IntPtr.Zero;
        //            _lastMemSegPtr = IntPtr.Zero;
        //            unsafe
        //            {
        //                _memSegDataPtr = (byte*) (void*) IntPtr.Zero;;
        //            }
        //            _memSegSize = 0;
        //            _memSegReadIndex = -1;
        //            _memSegWriteIndex = -1;
        //            _globalReaded = 0;
        //            _globalWrited = 0;
        //            
        //            // Обязательно устанавливаем флаг, указывающий на то, что буфер больше нельзя использовать.
        //            _released = true;
        //        }
    }
}