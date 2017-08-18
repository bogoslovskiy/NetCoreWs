using System.Net.Sockets;

namespace NetCoreWs.Sockets
{
    public class TcpServerSocketChannel : TcpSocketChannelBase<TcpServerSocketChannelParameters>
    {
        public void Accept(Socket socket)
        {
            Socket = socket;
        }
    }
}