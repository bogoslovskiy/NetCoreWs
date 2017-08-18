using System.Net;

namespace NetCoreWs.Sockets
{
    public class TcpServerSocketChannelBusParameters
    {
        public IPAddress IpAddress { get; set; }

        public int Port { get; set; }

        public int ListenBacklog { get; set; }
    }
}