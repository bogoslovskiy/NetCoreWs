using System;
using NetCoreWs.Core;

namespace NetCoreWs.WebSockets
{
    abstract public partial class WebSocketsMessageHandler : MessageHandler<WebSocketFrame>
    {
        [ThreadStatic] static private byte[] _maskBytes;
        
        private readonly bool _useMask;
        
        protected WebSocketsMessageHandler(bool useMask)
        {
            _useMask = useMask;
        }
    }
}