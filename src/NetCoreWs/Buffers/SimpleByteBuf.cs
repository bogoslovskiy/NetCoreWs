using System;
using System.Runtime.CompilerServices;
using System.Text;
using NetCoreWs.Utils;

namespace NetCoreWs.Buffers
{
    public class SimpleByteBuf : ByteBuf
    {
        private readonly SimpleByteBufProvider _byteBufProvider;
        
        private bool _released;
        
        private SimpleByteBufNode _node;
        private byte[] _data;
        private int _nodeSize;
        private SimpleByteBufNode _lastNode;
        
        private int _writeIndex;
        private int _nextWrited;
        private int _readIndex;
        
        public override bool Released => _released;

        public SimpleByteBuf(SimpleByteBufProvider byteBufProvider)
        {
            _byteBufProvider = byteBufProvider;
        }
        
        // TODO:
        ~SimpleByteBuf()
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
        
        public void Attach(byte[] data, int size, int used)
        {
            SimpleByteBufNode node = new SimpleByteBufNode() {Data = data, Size = size, Used = used};
            
            _node = node;
            _nodeSize = size;
            _data = data;
            _lastNode = node;

            _writeIndex = used - 1;
            _nextWrited = 0;
            _readIndex = -1;
            _released = false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetWrite(int write)
        {
            _node.Used = write;
            _writeIndex = write - 1;
        }
        
        public void GetReadable(out byte[] data, out int length)
        {
            data = _node.Data;
            length = _writeIndex + 1;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Append(ByteBuf byteBuf)
        {
            var simpleByteBuf = (SimpleByteBuf) byteBuf;

            SimpleByteBufNode next = simpleByteBuf._node;
            
            _lastNode.Next = next;
            next.Prev = _lastNode;

            _lastNode = simpleByteBuf._lastNode;

            _nextWrited += simpleByteBuf._nextWrited + next.Used;

            //_byteBufProvider.Release(simpleByteBuf);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Release()
        {
            return;
            ReleaseReaded();

            SimpleByteBufNode next = _node;
            while (next != null)
            {
                SimpleByteBufNode toRelease = next;
                next = next.Next;

                toRelease.Prev = null;
                toRelease.Next = null;
                
                //_byteBufProvider.Release(toRelease);
            }
            
            //_byteBufProvider.Release(this);
            _released = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void ReleaseReaded()
        {
            return;
            SimpleByteBufNode prev = _node.Prev;
            
            // Отвязываем от текущего.
            _node.Prev = null;
            
            while (prev != null)
            {
                SimpleByteBufNode toRelease = prev;
                prev = prev.Prev;

                toRelease.Prev = null;
                toRelease.Next = null;
                
                //_byteBufProvider.Release(toRelease);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int ReadableBytes()
        {
            return _writeIndex - _readIndex + _nextWrited;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Back(int offset)
        {
            SeekBackward(offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override byte ReadByte()
        {
			if (_readIndex < _writeIndex)
			{
			    _readIndex++;
			    return _data[_readIndex];
			}
            
            SeekForward(1);

            return _data[_readIndex];
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
            // TODO: Запись пока что невозможна в цепочку сегментов.
            return _nodeSize - _writeIndex - 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Write(byte @byte)
        {
            // TODO: Запись пока что невозможна в цепочку сегментов.
            if (_nodeSize - 1 == _writeIndex)
            {
                throw new InvalidOperationException();
            }
            
            _writeIndex++;
            _data[_writeIndex] = @byte;
        }

        public override string Dump(Encoding encoding)
        {
            int readable = ReadableBytes();

            int index = _readIndex;

            byte[] bytes = new byte[readable];

            int i = 0;
            while (i < readable)
            {
                index++;
                bytes[i] = _data[index];
                i++;
            }

            return encoding.GetString(bytes);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SeekForward(int offset)
        {
            SimpleByteBufNode node = _node;
            
            int writeIndex = _writeIndex;
            int nextWrited = _nextWrited;
            int readIndex = _readIndex + offset;

            while (readIndex > writeIndex)
            {
                node = node.Next;
                int used = node.Used;

                readIndex -= writeIndex + 1;

                writeIndex = used - 1;
                nextWrited -= used;
            }

            _node = node;
            _nodeSize = _node.Size;
            _data = _node.Data;
            _writeIndex = writeIndex;
            _nextWrited = nextWrited;
            _readIndex = readIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SeekBackward(int offset)
        {
            SimpleByteBufNode node = _node;
            
            int writeIndex = _writeIndex;
            int nextWrited = _nextWrited;
            int readIndex = _readIndex - offset;

            while (readIndex < -1)
            {
                nextWrited += node.Used;

                node = node.Prev;

                writeIndex = node.Used;
                readIndex += writeIndex;
            }

            _node = node;
            _nodeSize = _node.Size;
            _data = _node.Data;
            _writeIndex = writeIndex;
            _nextWrited = nextWrited;
            _readIndex = readIndex;
        }
    }
}