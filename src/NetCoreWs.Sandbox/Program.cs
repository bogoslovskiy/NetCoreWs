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
    public class LogByteBuf : DuplexMessageHandler<ByteBuf, ByteBuf>
    {
        public override void OnChannelActivated()
        {
            FireChannelActivated();
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
            
            message.Back(readableBytes);
            UpstreamMessageHandled(message);
        }

        protected override void HandleDownstreamMessage(ByteBuf message)
        {
            DownstreamMessageHandled(message);
        }
    }

    public class EchoHandler : SimplexUpstreamMessageHandler<ByteBuf>
    {
        public override void OnChannelActivated()
        {
            FireChannelActivated();
        }

        protected override void HandleUpstreamMessage(ByteBuf message)
        {
            ByteBuf outByteBuf = this.Pipeline.GetBuffer();

            byte[] prefix = System.Text.Encoding.UTF8.GetBytes("Your message: ");
            for (int i = 0; i < prefix.Length; i++)
            {
                outByteBuf.Write(prefix[i]);
            }

            int inReadable = message.ReadableBytes();
            for (int i = 0; i < inReadable; i++)
            {
                outByteBuf.Write(message.ReadByte());
            }
            
            DownstreamMessageHandled(outByteBuf);
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
                    x.Add(new EchoHandler());
                }
            );
            
            serverBootstrapper.Bootstrapp().StartListening();
        }
    }
}