using NetCoreWs.Buffers;

namespace NetCoreWs.WebSockets.Decoder
{
    class WebSocketReadMaskingKeyStep : IWebSocketDecoderStep
    {
        private IWebSocketDecoderStep _readPayloadDataStep;

        public void Init(IWebSocketDecoderStep readPayloadDataStep)
        {
            _readPayloadDataStep = readPayloadDataStep;
        }

        public void Clear()
        {
        }

        public void Read(
            ByteBuf byteBuf,
            ref WebSocketReadState state,
            out WebSocketFrameInfo? frameInfo,
            out IWebSocketDecoderStep nextStep)
        {
            frameInfo = null;
            nextStep = null;

            if (byteBuf.ReadableBytes() < 4)
            {
                return;
            }

            state.MaskingKey[0] = byteBuf.ReadByte();
            state.MaskingKey[1] = byteBuf.ReadByte();
            state.MaskingKey[2] = byteBuf.ReadByte();
            state.MaskingKey[3] = byteBuf.ReadByte();

            nextStep = _readPayloadDataStep;
        }
    }
}