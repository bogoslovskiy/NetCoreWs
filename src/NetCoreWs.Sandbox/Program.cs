using System;
using System.Threading;
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
            byte[] data = new byte[message.DataLen];
            for (int i = 0; i < message.DataLen; i++)
            {
                data[i] = message.BinaryData[i];
            }

            string messageStr = System.Text.Encoding.UTF8.GetString(data);
            Console.WriteLine($"WebSockets frame received '{messageStr}'");

            string response = $"Your message is '{messageStr}'";
            
            WebSocketFrame responseFrame = new WebSocketFrame();
            responseFrame.Type = WebSocketFrameType.Text;
            responseFrame.IsFinal = true;
            responseFrame.DataLen = response.Length;
            responseFrame.BinaryData = System.Text.Encoding.UTF8.GetBytes(response);
            
            SendMessage(responseFrame);
        }

        protected override void SetMask(byte[] maskBytes)
        {
            maskBytes[0] = 123;
            maskBytes[1] = 231;
            maskBytes[2] = 77;
            maskBytes[3] = 149;
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