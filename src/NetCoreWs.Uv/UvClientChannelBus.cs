using NetCoreUv;
using NetCoreWs.Channels;

namespace NetCoreWs.Uv
{
    public class UvClientChannelBus
        : ChannelBus<UvClientChannelBusParameters, UvTcpClientSocketChannel, UvTcpClientSocketChannelParameters>
    {
        private readonly UvLoopHandle _uvLoop;
        private UvTcpClientSocketChannel _uvTcpClientSocketChannel;
        
        public UvClientChannelBus()
        {
            _uvLoop = new UvLoopHandle();
            _uvLoop.Init();
        }

        public void Open()
        {
            //uv_tcp_keepalive
            _uvTcpClientSocketChannel = CreateChannel(); 
            _uvTcpClientSocketChannel.InitUv(_uvLoop);

            // TODO: правило деметры
            _uvTcpClientSocketChannel.UvTcpHandle.Connect(
                ServerAddress.FromUrl(this.Parameters.Url),
                ConnectionCallback
            );
            
            _uvLoop.RunDefault();
        }

        private void ConnectionCallback(UvStreamHandle streamhandle, int status)
        {
            _uvTcpClientSocketChannel.StartRead();
        }
    }
}