using NetCoreWs.Buffers;

namespace NetCoreWs.WebSockets
{
    public class WebSocketFrame
    {
        public WebSocketFrameType Type { get; set; }

        public bool IsFinal { get; set; }

        public ByteBuf ByteBuf { get; set; }
    }
}