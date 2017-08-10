using System;
using NetCoreWs.Buffers;

namespace NetCoreWs.Channels
{
    abstract public class ChannelBase
    {
        private volatile Action _activated;
        private volatile Action<ByteBuf> _receive;
        
        public Action Activated
        {
            get => _activated;
            set => _activated = value;
        }
        
        public Action<ByteBuf> Receive
        {
            get => _receive;
            set => _receive = value;
        }

        abstract public IByteBufProvider GetByteBufProvider();
        
        abstract public void Send(ByteBuf byteBuf);

        protected void FireActivated()
        {
            Action activated = _activated;
            if (activated != null)
            {
                activated();
            }
        }

        protected void FireReceive(ByteBuf byteBuf)
        {
            Action<ByteBuf> receive = _receive;
            if (receive != null)
            {
                receive(byteBuf);
            }
        }        
    }
}