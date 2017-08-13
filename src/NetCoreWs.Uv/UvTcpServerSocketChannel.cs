using NetCoreUv;

namespace NetCoreWs.Uv
{
    public class UvTcpServerSocketChannel : UvTcpSocketChannelBase<UvTcpServerSocketChannelParameters>
    {
        public void Accept(UvStreamHandle streamHandle)
        {
            streamHandle.Accept(UvTcpHandle);
        }
    }
}