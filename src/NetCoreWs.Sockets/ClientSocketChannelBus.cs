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
            
            _channel.Connect(this.Parameters.Host, this.Parameters.Port);

            Task startReadTask = _channel.StartRead();
            Task.WaitAll(startReadTask);
        }
    }
}