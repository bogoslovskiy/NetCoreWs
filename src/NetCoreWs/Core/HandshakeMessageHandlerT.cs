using System;
using NetCoreWs.Buffers;
using NetCoreWs.Channels;

namespace NetCoreWs.Core
{
    abstract public class HandshakeMessageHandler<TMsg> : MessageHandlerBase
    {
        private ChannelType _channelType;
        
        protected volatile bool Handshaked;

        abstract protected void ServerReceiveHandshake(ByteBuf inByteBuf);
        
        abstract protected void ClientReceiveHandshake(ByteBuf inByteBuf);
        
        abstract protected void ClientSendHandshake(ByteBuf outByteBuf);
        
        abstract protected TMsg Decode(ByteBuf inByteBuf);

        abstract protected void Encode(TMsg message, ByteBuf outByteBuf);

        abstract protected void OnMessageReceived(TMsg message);
        
        public void SendMessage(TMsg message)
        {
            if (Handshaked)
            {
                ByteBuf outByteBuf = Channel.GetByteBufProvider().GetBuffer();
            
                Encode(message, outByteBuf);
            
                SendByteMessage(outByteBuf);
                
                return;
            }
            
            throw new InvalidOperationException("Handshake before use.");
        }

        // TODO: реализовать вызов при реализации клиентского канала.
        protected sealed override void ChannelActivated()
        {
            _channelType = Channel.GetChannelType();
            
            if (_channelType == ChannelType.Client)
            {
                ByteBuf outByteBuf = Channel.GetByteBufProvider().GetBuffer();
                ClientSendHandshake(outByteBuf);
            }
        }

        protected sealed override void HandleMessage(ByteBuf inByteBuf)
        {
            if (!Handshaked)
            {
                if (_channelType == ChannelType.Client)
                {
                    ClientReceiveHandshake(inByteBuf);
                }
                else
                {
                    ServerReceiveHandshake(inByteBuf);
                }
            }
            else
            {
                TMsg message = Decode(inByteBuf);
            
                OnMessageReceived(message);
            }
        }
    }
}