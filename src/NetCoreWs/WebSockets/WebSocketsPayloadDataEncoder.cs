using NetCoreWs.Buffers;
using NetCoreWs.Core;

namespace NetCoreWs.WebSockets
{
    public class WebSocketsPayloadDataEncoder : DuplexMessageHandler<ByteBuf, ByteBuf>
    {
        public override void OnChannelActivated()
        {
            FireChannelActivated();
        }

        protected override void HandleUpstreamMessage(ByteBuf message)
        {
            UpstreamMessageHandled(message);
        }

        protected override void HandleDownstreamMessage(ByteBuf message)
        {
            int payloadLen = message.ReadableBytes();
            
            ByteBuf outByteBuf = this.Pipeline.GetBuffer();

            Codec.Encode(
                outByteBuf,
                message,
                null /* maskBytes */,
                WebSocketFrameType.Text,
                true /* fin */,
                false /* masked */,
                payloadLen
            );
            
            // Освобождаем буфер.
            message.Release();
            
            DownstreamMessageHandled(outByteBuf);
        }
    }
}