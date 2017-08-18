using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Threading.Tasks;
using NetCoreWs.Channels;

namespace NetCoreWs.Sockets
{
    public class TcpServerSocketChannelBus
        : ChannelBus<TcpServerSocketChannelBusParameters, TcpServerSocketChannel, TcpServerSocketChannelParameters>
    {
        //private Socket _listenSocket;

        private TcpListener _server;
        
        private List<TcpServerSocketChannel> _clientChannels = new List<TcpServerSocketChannel>();
        
        public void Listen()
        {
            _server = new TcpListener(this.Parameters.IpAddress, this.Parameters.Port);
            
//            _listenSocket = new Socket(
//                AddressFamily.InterNetwork,
//                SocketType.Stream,
//                ProtocolType.Tcp
//            );
//
//            _listenSocket.Bind(new IPEndPoint(this.Parameters.IpAddress, this.Parameters.Port)); 
//
//            _listenSocket.Listen(this.Parameters.ListenBacklog);

            _server.Start();
            
            Listening().GetAwaiter().GetResult();
        }

        private async Task Listening()
        {
            while (true)
            {
                Console.WriteLine("Wait for a connection.");

                Socket clientSocket = await _server.AcceptSocketAsync();
                
                TcpServerSocketChannel channel = CreateChannel();
                channel.Accept(clientSocket);
                
                _clientChannels.Add(channel);

                channel.StartRead();
            }
        }
    }
}