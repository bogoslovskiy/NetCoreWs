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
        private Func<IPipeline> _getPipeline;

        public TChannelBusParameters Parameters { get; private set; }
        
        public void Init(
            Action<TChannelBusParameters> initChannelBusParameters,
            Action<TChannelParameters> initChannelParameters, 
            Func<IPipeline> getPipeline)
        {
            this.Parameters = new TChannelBusParameters();
            initChannelBusParameters(this.Parameters);
            
            _initChannelParameters = initChannelParameters;
            _getPipeline = getPipeline;
        }

        protected TChannel CreateChannel()
        {
            var channel = new TChannel();
            channel.Init(_initChannelParameters);
            
            IPipeline pipeline = _getPipeline();
            pipeline.LinkChannel(channel);

            return channel;
        }
    }
}