using System;
using NetCoreWs.Buffers;
using NetCoreWs.Core;

namespace NetCoreWs.Channels
{
    abstract public class ChannelBase<TChannelParameters> : ChannelBase
        where TChannelParameters : class, new()
    {
        private MessageHandlerBase _messageHandler;

        public TChannelParameters Parameters { get; private set; }
        
        public void Init(
            Action<TChannelParameters> initChannelParameters,
            MessageHandlerBase messageHandler)
        {
            Parameters = new TChannelParameters();
            initChannelParameters(Parameters);
            
            _messageHandler = messageHandler;
        }
        
        protected void Receive(ByteBuf byteBuf)
        {
            _messageHandler.ByteMessageReceived(byteBuf);
        }
    }
}