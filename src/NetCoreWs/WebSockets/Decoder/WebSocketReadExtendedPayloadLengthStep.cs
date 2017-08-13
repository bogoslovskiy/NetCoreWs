using System;
using NetCoreWs.Buffers;

namespace NetCoreWs.WebSockets.Decoder
{
    class WebSocketReadExtendedPayloadLengthStep : IWebSocketDecoderStep
    {
        private IWebSocketDecoderStep _readMaskingKeyStep;
        private IWebSocketDecoderStep _readPayloadDataStep;

        public void Init(IWebSocketDecoderStep readMaskingKeyStep, IWebSocketDecoderStep readPayloadDataStep)
        {
            _readMaskingKeyStep = readMaskingKeyStep;
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

            if (state.PayloadLen == 126)
            {
                if (byteBuf.ReadableBytes() < 2)
                {
                    return;
                }

                state.ExtendedPayloadLen = byteBuf.ReadUShort();
            }
            else if (state.PayloadLen == 127)
            {
                throw new NotSupportedException();
            }

            nextStep = state.Mask 
                ? _readMaskingKeyStep 
                : _readPayloadDataStep;
        }
    }
}