using NetCoreWs.Buffers;

namespace NetCoreWs.WebSockets.Decoder
{
    class WebSocketReadHeaderStep : IWebSocketDecoderStep
    {
        private IWebSocketDecoderStep _readExtendedLenStep;
        private IWebSocketDecoderStep _readMaskingKeyStep;
        private IWebSocketDecoderStep _readPayloadDataStep;

        public void Init(
            IWebSocketDecoderStep readExtendedLenStep,
            IWebSocketDecoderStep readMaskingKeyStep,
            IWebSocketDecoderStep readPayloadDataStep)
        {
            _readExtendedLenStep = readExtendedLenStep;
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

            if (byteBuf.ReadableBytes() < 2)
            {
                return;
            }

            state.Clear();

            byte headerByte1 = byteBuf.ReadByte();
            byte headerByte2 = byteBuf.ReadByte();

            state.Fin = (headerByte1 & Utils.MaskFin) == Utils.MaskFin;
            state.OpCode = (byte) (headerByte1 & Utils.MaskOpCode);
            state.Mask = (headerByte2 & Utils.MaskMask) == Utils.MaskMask;
            state.PayloadLen = (byte) (headerByte2 & Utils.MaskPayloadLen);

            if (state.PayloadLen > 125)
            {
                nextStep = _readExtendedLenStep;
            }
            else if (state.Mask)
            {
                nextStep = _readMaskingKeyStep;
            }
            else
            {
                nextStep = _readPayloadDataStep;
            }
        }
    }
}