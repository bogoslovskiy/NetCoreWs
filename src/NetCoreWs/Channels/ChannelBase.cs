using System;
using NetCoreWs.Buffers;

namespace NetCoreWs.Channels
{
    abstract public class ChannelBase
    {
        abstract public IByteBufProvider GetByteBufProvider();
        
        abstract public void Send(ByteBuf byteBuf);

        public Action<ByteBuf> Receive;
    }
}