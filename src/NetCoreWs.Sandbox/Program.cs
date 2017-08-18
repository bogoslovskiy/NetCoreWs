using System;
using System.Diagnostics;
using System.Net;
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
using ServerBootsrtapper2 = NetCoreWs.Core.Bootstrapper<
    NetCoreWs.Sockets.TcpServerSocketChannelBus,
    NetCoreWs.Sockets.TcpServerSocketChannelBusParameters,
    NetCoreWs.Sockets.TcpServerSocketChannel,
    NetCoreWs.Sockets.TcpServerSocketChannelParameters>;
using ClientBootsrtapper2 = NetCoreWs.Core.Bootstrapper<
    NetCoreWs.Sockets.ClientSocketChannelBus,
    NetCoreWs.Sockets.ClientSocketChannelBusParameters,
    NetCoreWs.Sockets.TcpClientSocketChannel,
    NetCoreWs.Sockets.TcpClientSocketChannelParameters>;

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
    
    public class MeasureHandler : DuplexMessageHandler<ByteBuf, ByteBuf>
    {
        private volatile int _count;
        private int _oldCount;
        private Timer _timer;
        
        public override void OnChannelActivated()
        {
            FireChannelActivated();
            
            _timer = new Timer(TimerCb, null, new TimeSpan(0, 0, 0, 0), new TimeSpan(0, 0, 0, 1));
        }

        protected override void HandleUpstreamMessage(ByteBuf message)
        {
            _count++;
            message.Release();
//            Console.WriteLine(message.Dump(System.Text.Encoding.UTF8));

//            ByteBuf outByteBuf = this.Pipeline.GetBuffer();
//
//            byte[] prefix = System.Text.Encoding.UTF8.GetBytes("Your message: ");
//            for (int i = 0; i < prefix.Length; i++)
//            {
//                outByteBuf.Write(prefix[i]);
//            }
//
//            int inReadable = message.ReadableBytes();
//            for (int i = 0; i < inReadable; i++)
//            {
//                outByteBuf.Write(message.ReadByte());
//            }
//            
            //DownstreamMessageHandled(message);
        }

        protected override void HandleDownstreamMessage(ByteBuf message)
        {
            DownstreamMessageHandled(message);
        }

        private void TimerCb(object state)
        {
            Console.WriteLine($"{_count-_oldCount} / {_count}");

            _oldCount = _count;
        }
    }

    public class SendMessageHandler : SimplexUpstreamMessageHandler<ByteBuf>
    {
        private Timer _timer;
        
        public override void OnChannelActivated()
        {
            FireChannelActivated();

            _timer = new Timer(TimerCb, null, new TimeSpan(0, 0, 0, 0), new TimeSpan(1, 0, 0, 0));
        }

        protected override void HandleUpstreamMessage(ByteBuf message)
        {
        }
        
        private void TimerCb(object state)
        {
            var sw = Stopwatch.StartNew();
            int count = 1000000;
            for (int i = 0; i < count; i++)
            {
                //Task writeTask = Task.Factory.StartNew(S);
                S();
            }
            sw.Stop();
            
            Console.WriteLine($"{count}: {sw.ElapsedMilliseconds} ms");
        }

        private int i;
        
        private void S()
        {
            byte[] msgBytes = System.Text.Encoding.ASCII.GetBytes(
                $"Данное решение позволяет расширить store по «вертикали». " +
                $"Но бывают случаи, когда данного разделения может быть недостаточно. " +
                $"Например, один из уровней несет в себе составную логику, которую тоже было бы неплохо " +
                $"разделить (или как говорил один из известных людей: «Ухлубить!»). " +
                $"Но такого подхода нет в API Redux. И поиск решения данного вопроса так же ничего не дал " +
                $"(может плохо искал). Поэтому я разработал свой подход " +
                $"расширения по «горизонтали» Redux Store. {i++}"
            );
            
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
            try
            {
                Task serverTask = Task.Factory.StartNew(StartServer2);
                Thread.Sleep(1000);
                //Task clientTask1 = Task.Factory.StartNew(StartClient);
                Task clientTask2 = Task.Factory.StartNew(StartClient2);
            
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                //throw;
            }
        }

        static void StartServer2()
        {
            var serverBootstrapper = new ServerBootsrtapper2();
            serverBootstrapper.InitChannel(
                x =>
                {
                    x.IpAddress = new IPAddress(new byte[] {127, 0, 0, 1});
                    x.Port = 5052;
                    x.ListenBacklog = 100;
                },
                x => { }
            );
            serverBootstrapper.InitPipeline(
                x =>
                {
                    x.Add(new WebSocketsServerHandshakeHandler());
                    x.Add(new WebSocketsPayloadDataDecoder());
                    x.Add(new WebSocketsPayloadDataEncoder());
                    //x.Add(new LogByteBuf("Server"));
                    //x.Add(new EchoHandler());
                    x.Add(new MeasureHandler());
                    //x.Add(new EchoHandler());
                }
            );
            
            serverBootstrapper.Bootstrapp().Listen();
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
                    x.Add(new WebSocketsPayloadDataDecoder());
                    x.Add(new WebSocketsPayloadDataEncoder());
                    //x.Add(new LogByteBuf("Server"));
                    //x.Add(new EchoHandler());
                    x.Add(new MeasureHandler());
                    //x.Add(new EchoHandler());
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
                    x.Add(new WebSocketsPayloadDataDecoder());
                    x.Add(new WebSocketsPayloadDataEncoder());
                    //x.Add(new LogByteBuf("Client"));
                    x.Add(new SendMessageHandler());
                }
            );
            
            clientBootstrapper.Bootstrapp().Open();
        }
        
        static void StartClient2()
        {
            var clientBootstrapper = new ClientBootsrtapper2();
            clientBootstrapper.InitChannel(
                x =>
                {
                    x.IpAddress = new IPAddress(new byte[] {127, 0, 0, 1});
                    x.Port = 5052;
                },
                x => { }
            );
            clientBootstrapper.InitPipeline(
                x =>
                {
                    x.Add(new WebSocketsClientHandshakeHandler());
                    x.Add(new WebSocketsPayloadDataDecoder());
                    x.Add(new WebSocketsPayloadDataEncoder());
                    //x.Add(new LogByteBuf("Client"));
                    x.Add(new SendMessageHandler());
                }
            );
            
            clientBootstrapper.Bootstrapp().Open();
        }
    }
}