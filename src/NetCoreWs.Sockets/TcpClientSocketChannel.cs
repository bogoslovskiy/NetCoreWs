namespace NetCoreWs.Sockets
{
    public class TcpClientSocketChannel : TcpSocketChannelBase<TcpClientSocketChannelParameters>
    {
        public void Connect(string host, int port)
        {
            Socket.Connect(host, port);
        }
    }
}