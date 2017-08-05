using System;
using NetCoreWs.Channels;

namespace NetCoreWs.Core
{
    public class Bootstrapper<TChannelBus, TChannelBusParameters, TChannel, TChannelParameters>
        where TChannelBus : ChannelBus<TChannelBusParameters, TChannel, TChannelParameters>, new()
        where TChannelBusParameters : class, new()
        where TChannel : ChannelBase<TChannelParameters>, new()
        where TChannelParameters : class, new()
    {
        private Action<TChannelBusParameters> _initChannelBusParameters;
        private Action<TChannelParameters> _initChannelParameters;
        private Func<MessageHandlerBase> _getHandler;
        
        static public Bootstrapper<TChannelBus, TChannelBusParameters, TChannel, TChannelParameters> UseChannel(
            Action<TChannelBusParameters> initChannelBusParameters,
            Action<TChannelParameters> initChannelParameters)
        {
            var bootstrapper = new Bootstrapper<TChannelBus, TChannelBusParameters, TChannel, TChannelParameters>();
            bootstrapper._initChannelBusParameters = initChannelBusParameters;
            bootstrapper._initChannelParameters = initChannelParameters;

            return bootstrapper;
        }

        static public Bootstrapper<TChannelBus, TChannelBusParameters, TChannel, TChannelParameters> UseHandler<TMessageHandler>(
            Bootstrapper<TChannelBus, TChannelBusParameters, TChannel, TChannelParameters> bootstrapper)
            where TMessageHandler : MessageHandlerBase, new()
        {
            bootstrapper._getHandler = () => new TMessageHandler();
            return bootstrapper;
        }

        public TChannelBus Bootstrapp()
        {
            var channelBus = new TChannelBus();
            channelBus.Init(_initChannelBusParameters, _initChannelParameters, _getHandler);
            _initChannelBusParameters(channelBus.Parameters);

            return channelBus;
        }
    }
}