using NetCoreWs.Buffers;

namespace NetCoreWs.WebSockets.Decoder
{
    public interface IWebSocketDecoderStep
    {
        void Clear();
        
        void Read(
            ByteBuf byteBuf,
            ref WebSocketReadState state,
            out WebSocketFrameInfo? frameInfo,
            out IWebSocketDecoderStep nextStep);
    }
}