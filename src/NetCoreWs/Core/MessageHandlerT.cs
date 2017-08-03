using NetCoreWs.Buffers;

namespace NetCoreWs.Core
{
    abstract public class MessageHandler<TMsg> : MessageHandlerBase
    {
        abstract protected TMsg Decode(ByteBuf inByteBuf);

        abstract protected void Encode(TMsg message, ByteBuf outByteBuf);

        abstract protected void OnMessageReceived(TMsg message);

        public void SendMessage(TMsg message)
        {
            // TODO: 
            ByteBuf outByteBuf = null;
            
            Encode(message, outByteBuf);
            
            SendByteMessage(outByteBuf);
        }
        
        protected sealed override void HandleMessage(ByteBuf byteBuf)
        {
            TMsg message = Decode(byteBuf);
            
            OnMessageReceived(message);
        }
    }
}