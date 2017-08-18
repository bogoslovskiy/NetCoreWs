using System.Net;
using System.Net.Sockets;

namespace NetCoreWs.Sockets
{
    public class TcpClientSocketChannel : TcpSocketChannelBase<TcpClientSocketChannelParameters>
    {
        public void Connect(IPAddress ipAddress, int port)
        {
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Socket.Connect(ipAddress, port);
        }
    }
}