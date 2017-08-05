using NetCoreWs.Buffers;

namespace NetCoreWs.Channels
{
    abstract public class ChannelBase
    {
        abstract public ChannelType GetChannelType();

        abstract public IByteBufProvider GetByteBufProvider();
        
        abstract protected void SendCore(ByteBuf byteBuf);

        public void Send(ByteBuf byteBuf)
        {
            SendCore(byteBuf);
        }
    }
}