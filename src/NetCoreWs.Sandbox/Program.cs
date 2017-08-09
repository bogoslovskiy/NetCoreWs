using System;
using System.Threading.Tasks;
using NetCoreWs.WebSockets;
using ServerBootsrtapper = NetCoreWs.Core.Bootstrapper<
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
            responseFrame.BinaryData = System.Text.Encoding.UTF8.GetBytes(response);
            responseFrame.DataLen = responseFrame.BinaryData.Length;
            
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
            Task serverTask = Task.Factory.StartNew(() => StartServer());
            
            Console.ReadLine();
        }

        static void StartServer()
        {
            ServerBootsrtapper bootstrapper = ServerBootsrtapper
                .UseChannel(
                    x =>
                    {
                        x.Url = "http://127.0.0.1:5052";
                        x.ListenBacklog = 100;
                    },
                    x => { }
                );
            ServerBootsrtapper.UseHandler<WebSocketsHandler>(bootstrapper);
            
            bootstrapper.Bootstrapp().StartListening();
        }
    }
}