using NetCoreWs.Buffers;

namespace NetCoreWs.Core
{
    abstract public class MessageHandlerBase
    {
        private MessageChannelBase _messageChannel;

        abstract protected void HandleMessage(ByteBuf byteBuf);
        
        public void Init(MessageChannelBase messageChannel)
        {
            _messageChannel = messageChannel;
        }
        
        public void ByteMessageReceived(ByteBuf byteBuf)
        {
            HandleMessage(byteBuf);
        }

        protected void SendByteMessage(ByteBuf byteBuf)
        {
            _messageChannel.Send(byteBuf);
        }
    }
}