namespace NetCoreWs.WebSockets.Decoder
{
    public struct WebSocketFrameInfo
    {
        public WebSocketFrameType Type { get; }

        public bool IsFinal { get; }
        
        public bool Masked { get; }
        
        public byte[] MaskBytes { get; }

        public int PayloadDataLen { get; }

        public WebSocketFrameInfo(
            WebSocketFrameType type, 
            bool isFinal, 
            bool masked, 
            byte[] maskBytes, 
            int payloadDataLen)
        {
            this.Type = type;
            this.IsFinal = isFinal;
            this.Masked = masked;
            this.MaskBytes = maskBytes;
            this.PayloadDataLen = payloadDataLen;
        }
    }
}