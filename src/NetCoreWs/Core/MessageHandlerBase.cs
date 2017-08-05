using NetCoreWs.Buffers;
using NetCoreWs.Channels;

namespace NetCoreWs.Core
{
    abstract public class MessageHandlerBase
    {
        protected ChannelBase Channel;

        abstract protected void ChannelActivated();
        
        abstract protected void HandleMessage(ByteBuf byteBuf);
        
        public void Init(ChannelBase channel)
        {
            Channel = channel;
        }
        
        public void ByteMessageReceived(ByteBuf byteBuf)
        {
            HandleMessage(byteBuf);
        }

        protected void SendByteMessage(ByteBuf byteBuf)
        {
            Channel.Send(byteBuf);
        }
    }
}