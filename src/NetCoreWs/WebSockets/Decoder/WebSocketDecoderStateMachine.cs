using NetCoreWs.Buffers;

namespace NetCoreWs.WebSockets.Decoder
{
    public class WebSocketDecoderStateMachine
    {
        // ReSharper disable NotAccessedField.Local
        private readonly IWebSocketDecoderStep _readHeaderStep;
        private readonly IWebSocketDecoderStep _readExtendedLenStep;
        private readonly IWebSocketDecoderStep _readMaskingKeyStep;
        private readonly IWebSocketDecoderStep _readPayloadDataStep;
        // ReSharper restore NotAccessedField.Local

        private IWebSocketDecoderStep _currentStep;
        private WebSocketReadState _state;

        public WebSocketDecoderStateMachine()
        {
            var readHeaderStep = new WebSocketReadHeaderStep();
            var readExtendedLenStep = new WebSocketReadExtendedPayloadLengthStep();
            var readMaskingKeyStep = new WebSocketReadMaskingKeyStep();
            var readPayloadDataStep = new WebSocketReadPayloadDataStep();

            readHeaderStep.Init(readExtendedLenStep, readMaskingKeyStep, readPayloadDataStep);
            readExtendedLenStep.Init(readMaskingKeyStep, readPayloadDataStep);
            readMaskingKeyStep.Init(readPayloadDataStep);

            _readHeaderStep = readHeaderStep;
            _readExtendedLenStep = readExtendedLenStep;
            _readMaskingKeyStep = readMaskingKeyStep;
            _readPayloadDataStep = readPayloadDataStep;

            // TODO: пулинг byte[]
            _state = new WebSocketReadState(
                false /* fin */,
                0 /* opcode */,
                false /* mask */,
                0 /* payloadLen */,
                0 /* extendedPayloadLen */,
                new byte[4] /* maskingKeys */
            );

            _currentStep = _readHeaderStep;
        }

        public void Clear()
        {
            _currentStep.Clear();
            _currentStep = _readHeaderStep;
            _state.Clear();
        }

        public void Read(ByteBuf byteBuf, out WebSocketFrameInfo? frameInfo)
        {
            IWebSocketDecoderStep nextStep;
            do
            {
                _currentStep.Read(byteBuf, ref _state, out frameInfo, out nextStep);
                if (nextStep != null)
                {
                    _currentStep.Clear();
                    _currentStep = nextStep;
                }
            }
            // Прерываемся только если текущий шаг не может быть закончен (буфер закончился) или если есть фрейм.
            while (nextStep != null && frameInfo == null);

            // Если есть фрейм, то нужно сбросить состояние, чтобы при следующем чтении начинать с исходного состояния.
            if (frameInfo != null)
            {
                Clear();
            }
        }
    }
}