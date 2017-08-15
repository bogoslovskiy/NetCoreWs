using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using NetCoreWs.Buffers;
using NetCoreWs.Channels;

namespace NetCoreWs.Sockets
{
    public class TcpSocketChannelBase<TChannelParameters> : ChannelBase<TChannelParameters>
        where TChannelParameters : class, new()
    {
        protected Socket Socket;
        private SimpleByteBufProvider _byteBufProvider;

        public TcpSocketChannelBase()
        {
            _byteBufProvider = new SimpleByteBufProvider();
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public async Task StartRead()
        {
            byte[] buffer = new byte[4096];
            
            while (true)
            {
                int received = await Socket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                if (received > 0)
                {
                    FireReceive(new SimpleByteBuf(buffer));
                }
            }
        }
        
        public override IByteBufProvider GetByteBufProvider()
        {
            return _byteBufProvider;
        }

        public override void Send(ByteBuf byteBuf)
        {
            SimpleByteBuf simpleByteBuf = (SimpleByteBuf) byteBuf;

            byte[] buffer = simpleByteBuf.GetInternalBuffer(out int offset, out int size);

            Socket.Send(buffer, offset, size, SocketFlags.None);
        }
    }
}