using System;
using System.Threading.Tasks;
using NetCoreWs.Buffers;
using NetCoreWs.Core;
using NetCoreWs.WebSockets;
using NetCoreWs.WebSockets.Handshake;
using ServerBootsrtapper = NetCoreWs.Core.Bootstrapper<
    NetCoreWs.Uv.UvServerChannelBus,
    NetCoreWs.Uv.UvServerChannelBusParameters,
    NetCoreWs.Uv.UvTcpServerSocketChannel,
    NetCoreWs.Uv.UvTcpServerSocketChannelParameters>;

namespace NetCoreWs.Sandbox
{
    public class LogByteBuf : SimplexUpstreamMessageHandler<ByteBuf>
    {
        public override void OnChannelActivated()
        {
        }

        protected override void HandleUpstreamMessage(ByteBuf message)
        {
            int readableBytes = message.ReadableBytes();
            byte[] data = new byte[readableBytes];
            for (int i = 0; i < readableBytes; i++)
            {
                data[i] = message.ReadByte();
            }

            string messageStr = System.Text.Encoding.UTF8.GetString(data);
            Console.WriteLine($"Message received '{messageStr}'");
        }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            Task serverTask = Task.Factory.StartNew(() => StartServer());
            
            Console.ReadLine();
        }

        static void StartServer()
        {
            var serverBootstrapper = new ServerBootsrtapper();
            serverBootstrapper.InitChannel(
                x =>
                {
                    x.Url = "http://127.0.0.1:5052";
                    x.ListenBacklog = 100;
                },
                x => { }
            );
            serverBootstrapper.InitPipeline(
                x =>
                {
                    x.Add(new WebSocketsServerHandshakeHandler());
                    x.Add(new WebSocketsPayloadDataHandler());
                    x.Add(new LogByteBuf());
                }
            );
            
            serverBootstrapper.Bootstrapp().StartListening();
        }
    }
}