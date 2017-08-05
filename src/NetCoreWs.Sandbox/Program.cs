using System;
using NetCoreWs.Buffers;
using NetCoreWs.Core;
using NetCoreWs.Uv;
using NetCoreWs.WebSockets;
using Bootsrtapper = NetCoreWs.Core.Bootstrapper<
    NetCoreWs.Uv.UvServerChannelBus,
    NetCoreWs.Uv.UvServerChannelBusParameters,
    NetCoreWs.Uv.UvTcpServerSocketChannel,
    NetCoreWs.Uv.UvTcpServerSocketChannelParameters>;

namespace NetCoreWs.Sandbox
{
    public class WebSocketsHandler : WebSocketsMessageHandler
    {
        public WebSocketsHandler() 
            : base(false /* useMask */)
        {
        }

        protected override void ClientReceiveHandshake(ByteBuf inByteBuf)
        {
            throw new NotImplementedException();
        }

        protected override void ClientSendHandshake(ByteBuf outByteBuf)
        {
            throw new NotImplementedException();
        }

        protected override void OnMessageReceived(WebSocketFrame message)
        {
            Console.WriteLine($"WebSockets frame received {message.ByteBuf.Dump()}");
        }

        protected override void SetMask(byte[] maskBytes)
        {
            throw new NotImplementedException();
        }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            Bootsrtapper bootstrapper = Bootsrtapper
                .UseChannel(
                    x =>
                    {
                        x.Url = "http://127.0.0.1:5052";
                        x.ListenBacklog = 100;
                    },
                    x => { }
                );
            Bootsrtapper.UseHandler<WebSocketsHandler>(bootstrapper);
            
            bootstrapper.Bootstrapp().StartListening();
        }
    }
}