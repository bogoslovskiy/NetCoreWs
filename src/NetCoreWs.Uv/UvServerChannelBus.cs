using System.Collections.Generic;
using NetCoreUv;
using NetCoreWs.Channels;

namespace NetCoreWs.Uv
{
    public class UvServerChannelBus 
        : ChannelBus<UvServerChannelBusParameters, UvTcpServerSocketChannel, UvTcpServerSocketChannelParameters>
    {
        private readonly UvLoopHandle _uvLoop;
        private readonly UvTcpHandle _listenUvTcpHandle;
        private readonly List<UvTcpServerSocketChannel> _uvTcpServerSocketChannels = 
            new List<UvTcpServerSocketChannel>();
        
        public UvServerChannelBus()
        {
            _uvLoop = new UvLoopHandle();
            _uvLoop.Init();
            
            _listenUvTcpHandle = new UvTcpHandle();
            _listenUvTcpHandle.Init(_uvLoop);
        }

        public void StartListening()
        {
            _listenUvTcpHandle.Bind(ServerAddress.FromUrl(Parameters.Url));
            _listenUvTcpHandle.Listen(Parameters.ListenBacklog /* backLog */, ConnectionCallback);

            _uvLoop.RunDefault();
        }
        
        private void ConnectionCallback(UvStreamHandle streamHandle, int status)
        {
            UvTcpServerSocketChannel channel = CreateChannel();
            channel.InitUv(_uvLoop);

            streamHandle.Accept(channel.UvTcpHandle);
            _uvTcpServerSocketChannels.Add(channel);

            channel.StartRead();
            
            channel.ActivateChannel();
        }
    }
}