using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NetCoreWs.Utils;

namespace NetCoreWs.Buffers.Unmanaged
{
    // TODO: добавить и прописать нормальные типы исключений
    public class UnmanagedByteBuf : ByteBuf
    {
        private struct State
        {
            public IntPtr MemSegPtr;
            public int MemSegSize;
            unsafe public byte* MemSegDataPtr;
            public int GlobalReaded;
            public int GlobalWrited;

            unsafe public State(
                IntPtr memSegPtr,
                int memSegSize,
                byte* memSegDataPtr,
                int globalReadIndex,
                int globalWriteIndex)
            {
                MemSegPtr = memSegPtr;
                MemSegSize = memSegSize;
                MemSegDataPtr = memSegDataPtr;
                GlobalReaded = globalReadIndex;
                GlobalWrited = globalWriteIndex;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int RemainBytes()
            {
                return GlobalWrited - GlobalReaded;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Back(int offset)
            {
                GlobalReaded -= offset;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            unsafe public byte ReadByte()
            {
                GlobalReaded++;

                return MemSegDataPtr[GlobalReaded];
            }
        }

        // TODO: заменить на интерфейс? 
        //private readonly UnmanagedByteBufAllocator _allocator;

        private IntPtr _memSegPtr;
        private int _memSegSize;
        unsafe private byte* _memSegDataPtr;

        private int _globalReaded;
        private int _globalWrited;
        
        public UnmanagedByteBuf(IntPtr memPtr, int len)
        {
            _memSegSize = len;
            _memSegPtr = memPtr;
            unsafe
            {
                _memSegDataPtr = (byte*) (void*) _memSegPtr;
            }
            _globalReaded = -1;
            _globalWrited = -1;
        }
        
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

        public void Attach(IntPtr memSeg)
        {
            _memSegPtr = memSeg;
            //_lastMemSegPtr = memSeg;
            unsafe
            {
                _memSegDataPtr = (byte*) (void*) memSeg;
            }
//            _memSegSize = MemorySegment.GetUsed(memSeg);
            _globalReaded = -1;
            _globalWrited = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetReadable(out IntPtr dataPtr, out int length)
        {
            // Учитывая то, кто использует этот метод, тут никогда не будет других чтений и никогда не будет больше
            // одного сегмента.
            length = _globalWrited + 1;
            dataPtr = _memSegPtr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetWrite(int write)
        {
            _globalWrited = write - 1;
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
            return _globalWrited - _globalReaded;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Back(int offset)
        {
            _globalReaded -= offset;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe public override byte ReadByte()
        {
            _globalReaded++;

            return _memSegDataPtr[_globalReaded];
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

        public override ByteBuf SliceFromCurrentReadPosition(int len)
        {
            // TODO: !!!!
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int ReadToOrRollback(byte stopByte, byte[] output, int startIndex, int len)
        {
            bool stopByteMatched = false;

            int readed = 0;

            State state = GetState();

            while (state.RemainBytes() > 0)
            {
                byte currentByte = state.ReadByte();

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
                state.Back(1);
                SetState(state);
                return readed;
            }

            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int ReadToOrRollback(byte stopByte1, byte stopByte2, byte[] output, int startIndex, int len)
        {
            bool stopByte1Matched = false;
            bool stopBytesMatched = false;

            int readed = 0;

            State state = GetState();

            while (state.RemainBytes() > 0)
            {
                byte currentByte = state.ReadByte();

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
                state.Back(2);
                SetState(state);
                return readed;
            }

            return -1;
        }

        public override int SkipTo(byte stopByte, bool include)
        {
            int skipped = 0;

            bool stopByteMatched = false;

            State state = GetState();

            while (state.RemainBytes() > 0)
            {
                skipped++;
                byte currentByte = state.ReadByte();

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
                    state.Back(1);
                }

                SetState(state);
                return skipped;
            }

            return -1;
        }

        public override int SkipTo(byte stopByte1, byte stopByte2, bool include)
        {
            int skipped = 0;

            bool stopByte1Matched = false;
            bool stopBytesMatched = false;

            State state = GetState();

            while (state.RemainBytes() > 0)
            {
                skipped++;
                byte currentByte = state.ReadByte();

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
                    state.Back(2);
                }

                SetState(state);
                return skipped;
            }

            return -1;
        }

        public override int WritableBytes()
        {
            // TODO: Запись пока что невозможна в цепочку сегментов.
            return _memSegSize - _globalWrited - 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe public override void Write(byte @byte)
        {
            _globalWrited++;

            _memSegDataPtr[_globalWrited] = @byte;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe private State GetState()
        {
            return new State(
                _memSegPtr,
                _memSegSize,
                _memSegDataPtr,
                _globalReaded,
                _globalWrited
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe private void SetState(State state)
        {
            _memSegPtr = state.MemSegPtr;
            _memSegSize = state.MemSegSize;
            _memSegDataPtr = state.MemSegDataPtr;
            _globalReaded = state.GlobalReaded;
            _globalWrited = state.GlobalWrited;
        }

        unsafe public override string Dump()
        {
            int readable = ReadableBytes();

            int index = _globalReaded;

            byte[] bytes = new byte[readable];

            int i = 0;
            while(i < readable)
            {
				index++;
                bytes[i] = _memSegDataPtr[index];
                i++;
            }

            return System.Text.Encoding.ASCII.GetString(bytes);
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