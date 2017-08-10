using System;
using System.Collections.Generic;
using NetCoreWs.Buffers;
using NetCoreWs.Channels;

namespace NetCoreWs.Core
{
    // TODO: internal + builder
    public class Pipeline : IPipeline
    {
        private ChannelBase _channel;
        private IByteBufProvider _channelByteBufProvider;
        private readonly List<MessageHandlerBase> _handlers;
        private MessageHandlerBase _firstHandler;
        
        public Pipeline()
        {
            _handlers = new List<MessageHandlerBase>();
        }

        public void LinkChannel(ChannelBase channel)
        {
            if (_firstHandler == null)
            {
                throw new InvalidOperationException("First add handlers.");
            }

            _channel = channel;
            _channelByteBufProvider = channel.GetByteBufProvider();
            _channel.Activated = OnChannelActivated;
            _channel.Receive = ReceiveFromChannel;
        }

        public void Add(MessageHandlerBase handler)
        {
            handler.IsActive = true;
            handler.Pipeline = this;
            
            if (_firstHandler == null)
            {
                LinkFirstHandler(handler);
            }
            else
            {
                MessageHandlerBase lastHandler = _handlers.Count > 0
                    ? _handlers[_handlers.Count - 1]
                    : null;
                
                LinkHandlers(lastHandler /* prev */, handler /* next */);
            }
            
            _handlers.Add(handler);
        }

        public ByteBuf GetBuffer()
        {
            return _channelByteBufProvider.GetBuffer();
        }
        
        public void DeactivateHandler(MessageHandlerBase handler)
        {
            handler.IsActive = false;
            
            int handlerIndex = _handlers.IndexOf(handler);

            MessageHandlerBase prevHandler = FindPrevFirstActiveHandler(handlerIndex);
            MessageHandlerBase nextHandler = FindNextFirstActiveHandler(handlerIndex);

            if (prevHandler == null && nextHandler == null)
            {
                throw new Exception("No handlers remain.");
            }
            
            LinkHandlers(prevHandler, nextHandler);

            if (prevHandler == null)
            {
                // Текущий хэндлер был первым. Линкуем следующий хэндлер как первый.
                LinkFirstHandler(nextHandler);
            }
        }

        private void OnChannelActivated()
        {
            try
            {
                _firstHandler.OnChannelActivated();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // TODO: close channel + log reason
                throw;
            }
        }
        
        private void ReceiveFromChannel(ByteBuf byteBuf)
        {
            try
            {
                _firstHandler.HandleUpstreamMessage(byteBuf);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // TODO: close channel with some message to second side
                throw;
            }
        }

        private void SendToChannel(object message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var byteBuf = message as ByteBuf;
            if (byteBuf != null)
            {
                _channel.Send(byteBuf);
                return;
            }

            throw new InvalidOperationException("Message must be a ByteBuf.");
        }

        private MessageHandlerBase FindPrevFirstActiveHandler(int startIndex)
        {
            while (startIndex > 0)
            {
                startIndex--;
                
                MessageHandlerBase handler = _handlers[startIndex];
                if (handler.IsActive)
                {
                    return handler;
                }
            }

            return null;
        }
        
        private MessageHandlerBase FindNextFirstActiveHandler(int startIndex)
        {
            while (startIndex < _handlers.Count - 1)
            {
                startIndex++;

                MessageHandlerBase handler = _handlers[startIndex];
                if (handler.IsActive)
                {
                    return handler;
                }
            }

            return null;
        }
        
        private void LinkFirstHandler(MessageHandlerBase firstHandler)
        {
            _firstHandler = firstHandler;
            _firstHandler.PrevHandler = SendToChannel;
        }
        
        private void LinkHandlers(MessageHandlerBase prev, MessageHandlerBase next)
        {
            if (prev != null)
            {
                if (next != null)
                {
                    prev.NextHandler = next.HandleUpstreamMessage;
                    prev.ChannelActivated = next.OnChannelActivated;
                }
                else
                {
                    prev.NextHandler = null;
                    prev.ChannelActivated = null;
                }
            }

            if (next != null)
            {
                if (prev != null)
                {
                    next.PrevHandler = prev.HandleDownstreamMessage;
                }
                else
                {
                    next.PrevHandler = null;
                }
            }
        }
    }
}