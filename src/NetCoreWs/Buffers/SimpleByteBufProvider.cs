using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace NetCoreWs.Buffers
{
    public class SimpleByteBufProvider : IByteBufProvider
    {
        private readonly int _segmentSize;

        public SimpleByteBufProvider(int segmentSize)
        {
            _segmentSize = segmentSize;
        }

        public ByteBuf GetBuffer()
        {
            byte[] data = GetDefaultDataCore();
            SimpleByteBuf byteBuf = GetByteBufCore();
            
            byteBuf.Attach(data, _segmentSize, 0);

            return byteBuf;
        }

        public ByteBuf Wrap(byte[] data, int used)
        {
            SimpleByteBuf byteBuf = GetByteBufCore();
            
            byteBuf.Attach(data, _segmentSize, used);

            return byteBuf;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private SimpleByteBuf GetByteBufCore()
        {
            return new SimpleByteBuf(this);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] GetDefaultDataCore()
        {
            byte[] data = new byte[_segmentSize];
            return data;
        }
    }
}