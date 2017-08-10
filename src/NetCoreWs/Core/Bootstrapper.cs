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
        private Action<IPipeline> _pipelineInitializer;

        public void InitChannel(
            Action<TChannelBusParameters> initChannelBusParameters,
            Action<TChannelParameters> initChannelParameters)
        {
            _initChannelBusParameters = initChannelBusParameters;
            _initChannelParameters = initChannelParameters;
        }

        public void InitPipeline(Action<IPipeline> pipelineInitializer)
        {
            _pipelineInitializer = pipelineInitializer;
        }

        public TChannelBus Bootstrapp()
        {
            var channelBus = new TChannelBus();
            channelBus.Init(_initChannelBusParameters, _initChannelParameters, CreatePipeline);

            return channelBus;
        }

        private IPipeline CreatePipeline()
        {
            IPipeline pipeline = new Pipeline();

            _pipelineInitializer(pipeline);
            
            return pipeline;
        }
    }
}