using NetCoreWs.Buffers;

namespace NetCoreWs.Core
{
    abstract public class MessageChannelBase
    {
        private MessageHandlerBase _messageHandler;
        
        abstract protected void SendCore(ByteBuf byteBuf);
        
        public void Init(MessageHandlerBase messageHandler)
        {
            _messageHandler = messageHandler;
        }

        public void Send(ByteBuf byteBuf)
        {
            SendCore(byteBuf);
        }
        
        protected void Receive(ByteBuf byteBuf)
        {
            _messageHandler.ByteMessageReceived(byteBuf);
        }
    }
}