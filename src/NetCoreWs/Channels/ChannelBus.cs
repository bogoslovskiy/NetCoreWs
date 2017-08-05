using System;
using NetCoreWs.Core;

namespace NetCoreWs.Channels
{
    abstract public class ChannelBus<TChannelBusParameters, TChannel, TChannelParameters>
        where TChannel : ChannelBase<TChannelParameters>, new()
        where TChannelBusParameters : class, new()
        where TChannelParameters : class, new()
    {
        private Action<TChannelParameters> _initChannelParameters;
        private Func<MessageHandlerBase> _getHandler;

        public TChannelBusParameters Parameters { get; private set; }
        
        public void Init(
            Action<TChannelBusParameters> initChannelBusParameters,
            Action<TChannelParameters> initChannelParameters, 
            Func<MessageHandlerBase> getHandler)
        {
            Parameters = new TChannelBusParameters();
            initChannelBusParameters(Parameters);
            
            _initChannelParameters = initChannelParameters;
            _getHandler = getHandler;
        }

        protected TChannel CreateChannel()
        {
            var channel = new TChannel();
            var messageHandler = _getHandler();
            channel.Init(_initChannelParameters, messageHandler);
            messageHandler.Init(channel);

            return channel;
        }
    }
}