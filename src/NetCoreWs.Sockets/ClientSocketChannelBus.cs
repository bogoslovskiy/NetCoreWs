using System.Threading.Tasks;
using NetCoreWs.Channels;

namespace NetCoreWs.Sockets
{
    public class ClientSocketChannelBus 
        : ChannelBus<ClientSocketChannelBusParameters, TcpClientSocketChannel, TcpClientSocketChannelParameters>
    {
        private TcpClientSocketChannel _channel;
        
        public void Open()
        {
            _channel = CreateChannel();
            _channel.Connect(this.Parameters.IpAddress, this.Parameters.Port);
            
            Task readingTask = _channel.StartRead();
            Task.WaitAll(readingTask);
        }
    }
}