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
        private Task _readingTask;

        public TcpSocketChannelBase()
        {
            _byteBufProvider = new SimpleByteBufProvider(4096);
        }

        public Task StartRead()
        {
            FireActivated();
            _readingTask = Task.Factory.StartNew(StartReading);
            return _readingTask;
        }
        
        private async void StartReading()
        {
            while (true)
            {
                byte[] buffer = _byteBufProvider.GetDefaultDataCore();
                
                int received = await Socket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                if (received > 0)
                {
                    var byteBuf = _byteBufProvider.Wrap(buffer, received);
                    FireReceive(byteBuf);
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

            simpleByteBuf.GetReadable(out byte[] data, out int len);
            
            Socket.Send(data, 0, len, SocketFlags.None);
        }
    }
}