using NetCoreWs.Buffers;
using NetCoreWs.Channels;

namespace NetCoreWs.Core
{
    public interface IPipeline
    {
        void LinkChannel(ChannelBase channel);
        
        void Add(MessageHandlerBase handler);
        
        ByteBuf GetBuffer();

        void DeactivateHandler(MessageHandlerBase handler);
    }
}