using NetCoreUv;

namespace NetCoreWs.Uv
{
    public class UvTcpClientSocketChannel: UvTcpSocketChannelBase<UvTcpClientSocketChannelParameters>
    {
        public void KeepAlive(uint delay)
        {
            UvTcpHandle.KeepAlive(delay);
        }
        
        public void Connect(ServerAddress address, UvStreamHandle.ConnectionCallback connectionCallback)
        {
            UvTcpHandle.Connect(address, connectionCallback);
        }
    }
}