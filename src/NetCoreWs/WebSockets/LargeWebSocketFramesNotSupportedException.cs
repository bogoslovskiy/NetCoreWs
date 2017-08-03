using System;

namespace NetCoreWs.WebSockets
{
    public class LargeWebSocketFramesNotSupportedException : Exception
    {
        public LargeWebSocketFramesNotSupportedException(int payloadDataLen)
            : base($"Large WebSockets frames not supported. Payload data lenght = {payloadDataLen}.")
        {
        }
    }
}