using System;
using NetCoreWs.Buffers;
using NetCoreWs.Core;

namespace NetCoreWs.WebSockets.Handshake
{
    public class WebSocketsClientHandshakeHandler : SimplexUpstreamMessageHandler<ByteBuf>
    {
        static private byte[] _handshakeRequestBytes = System.Text.Encoding.ASCII.GetBytes(
            "GET / HTTP/1.1\r\nHost: localhost:5052\r\nConnection: Upgrade\r\nUpgrade: websocket\r\nSec-WebSocket-Version: 13\r\nSec-WebSocket-Key: 7mGOCG9DmlxOo0Tx/uXt9Q==\r\n\r\n"
        );

        static private byte[] _handshakeResponseBytes = System.Text.Encoding.ASCII.GetBytes(
            "HTTP/1.1 101 Switching Protocols\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Version: 13\r\nSec-WebSocket-Accept: ImnT28RIT4b46ZKtNOJG8IBD6a8=\r\n\r\n"
        );
        
        private bool _handshakeRequested;
        
        public override void OnChannelActivated()
        {
            ByteBuf handshakeRequestByteBuf = this.Pipeline.GetBuffer();

            for (int i = 0; i < _handshakeRequestBytes.Length; i++)
            {
                handshakeRequestByteBuf.Write(_handshakeRequestBytes[i]);
            }

            _handshakeRequested = true;
            // TODO: не очень понятное название метода для таких случаев
            DownstreamMessageHandled(handshakeRequestByteBuf);
        }

        protected override void HandleUpstreamMessage(ByteBuf message)
        {
            if (!_handshakeRequested)
            {
                throw new InvalidOperationException();
            }

            // TODO: дальше надо не просто кидать исключение, а логгировать проблему и закрывать канал.
            
            int preciseHandshakeResponseSize = _handshakeResponseBytes.Length;
            int checkHandshakeResponseSize = message.ReadableBytes();
            if (preciseHandshakeResponseSize != checkHandshakeResponseSize)
            {
                throw new Exception();
            }

            for (int i = 0; i < preciseHandshakeResponseSize; i++)
            {
                byte preciseByte = _handshakeResponseBytes[i];
                byte checkByte = message.ReadByte();

                if (preciseByte != checkByte)
                {
                    throw new Exception();
                }
            }
            
            this.Pipeline.DeactivateHandler(this);

            FireChannelActivated();
        }
    }
}