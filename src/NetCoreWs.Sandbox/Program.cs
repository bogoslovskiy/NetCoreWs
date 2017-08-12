using System;
using System.Diagnostics;
using System.Threading;
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
using ClientBootsrtapper = NetCoreWs.Core.Bootstrapper<
    NetCoreWs.Uv.UvClientChannelBus,
    NetCoreWs.Uv.UvClientChannelBusParameters,
    NetCoreWs.Uv.UvTcpClientSocketChannel,
    NetCoreWs.Uv.UvTcpClientSocketChannelParameters>;

namespace NetCoreWs.Sandbox
{
    public class LogByteBuf : DuplexMessageHandler<ByteBuf, ByteBuf>
    {
        private string _context;
        
        public LogByteBuf(string context)
        {
            _context = context;
        }
        
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
            Console.WriteLine($"{_context}. Message received '{messageStr}'");
            
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

    public class SendMessageHandler : SimplexUpstreamMessageHandler<ByteBuf>
    {
        private Timer _timer;
        
        public override void OnChannelActivated()
        {
            FireChannelActivated();

            _timer = new Timer(TimerCb, null, new TimeSpan(0, 0, 0, 3), new TimeSpan(1, 0, 0, 3));
            
        }

        protected override void HandleUpstreamMessage(ByteBuf message)
        {
        }
        
        private void TimerCb(object state)
        {
            var sw = Stopwatch.StartNew();
            int count = 1000;
            for (int i = 0; i < count; i++)
            {
                S();
                Thread.Sleep(1);
            }
            sw.Stop();
            
            Console.WriteLine($"{count}: {sw.ElapsedMilliseconds} ms");
        }

        private void S()
        {
            byte[] msgBytes = System.Text.Encoding.ASCII.GetBytes($"My time is: {DateTimeOffset.Now}");
            
            ByteBuf outByteBuf = this.Pipeline.GetBuffer();
            
            for (int i = 0; i < msgBytes.Length; i++)
            {
                outByteBuf.Write(msgBytes[i]);
            }
            
            DownstreamMessageHandled(outByteBuf);
        }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            Task serverTask = Task.Factory.StartNew(StartServer);
            Thread.Sleep(2000);
            Task clientTask = Task.Factory.StartNew(StartClient);
            
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
                    x.Add(new LogByteBuf("Server"));
                    x.Add(new EchoHandler());
                }
            );
            
            serverBootstrapper.Bootstrapp().StartListening();
        }
        
        static void StartClient()
        {
            var clientBootstrapper = new ClientBootsrtapper();
            clientBootstrapper.InitChannel(
                x =>
                {
                    x.Url = "http://127.0.0.1:5052";
                },
                x => { }
            );
            clientBootstrapper.InitPipeline(
                x =>
                {
                    x.Add(new WebSocketsClientHandshakeHandler());
                    x.Add(new WebSocketsPayloadDataHandler());
                    x.Add(new LogByteBuf("Client"));
                    x.Add(new SendMessageHandler());
                }
            );
            
            clientBootstrapper.Bootstrapp().Open();
        }
    }
}