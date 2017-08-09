using NetCoreUv;
using NetCoreWs.Channels;

namespace NetCoreWs.Uv
{
    public class UvClientChannelBus
        : ChannelBus<UvClientChannelBusParameters, UvTcpClientSocketChannel, UvTcpClientSocketChannelParameters>
    {
        private readonly UvLoopHandle _uvLoop;
        private readonly UvTcpHandle _clientUvTcpHandle;
        
        public UvClientChannelBus()
        {
            _uvLoop = new UvLoopHandle();
            _uvLoop.Init();
            
            _clientUvTcpHandle = new UvTcpHandle();
            _clientUvTcpHandle.Init(_uvLoop);
        }

        public void Open()
        {
            _uvLoop.RunDefault();
        }
    }
}