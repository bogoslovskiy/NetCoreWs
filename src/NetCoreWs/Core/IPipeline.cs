using NetCoreWs.Buffers;
using NetCoreWs.Channels;

namespace NetCoreWs.Core
{
    public interface IPipeline
    {
        ChannelBase Channel { get; }
        
        IByteBufProvider ChannelByteBufProvider { get; }

        void DeactivateHandler(MessageHandlerBase handler);
    }
}