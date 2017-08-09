using System.Collections.Generic;
using NetCoreWs.Buffers;
using NetCoreWs.Channels;

namespace NetCoreWs.Core
{
    public class Pipeline : IPipeline
    {
        private List<MessageHandlerBase> _handlers;
        
        public ChannelBase Channel { get; private set; }

        public IByteBufProvider ChannelByteBufProvider => this.Channel.GetByteBufProvider();

        public Pipeline(ChannelBase channel)
        {
            this.Channel = channel;
            _handlers = new List<MessageHandlerBase>();
        }
        
        public void DeactivateHandler(MessageHandlerBase handler)
        {
            throw new System.NotImplementedException();
        }
    }
}