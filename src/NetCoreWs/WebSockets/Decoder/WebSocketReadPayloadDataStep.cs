using NetCoreWs.Buffers;

namespace NetCoreWs.WebSockets.Decoder
{
    class WebSocketReadPayloadDataStep : IWebSocketDecoderStep
    {
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

            int factPayloadLen = state.ExtendedPayloadLen > 0
                ? (int) state.ExtendedPayloadLen
                : state.PayloadLen;
            
            if (byteBuf.ReadableBytes() < factPayloadLen)
            {
                return;
            }
            
            byte[] mask = new byte[4];
            state.MaskingKey.CopyTo(mask, 0);

            frameInfo = new WebSocketFrameInfo(
                Utils.GetFrameType(state.OpCode),
                state.Fin,
                state.Mask,
                mask,
                factPayloadLen
            );
        }
    }
}