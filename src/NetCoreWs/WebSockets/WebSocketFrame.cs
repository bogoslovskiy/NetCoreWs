namespace NetCoreWs.WebSockets
{
    public class WebSocketFrame
    {
        public WebSocketFrameType Type { get; set; }

        public bool IsFinal { get; set; }

        public byte[] BinaryData { get; set; }

        public int DataLen { get; set; }
    }
}