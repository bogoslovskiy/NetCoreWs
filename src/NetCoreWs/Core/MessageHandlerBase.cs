using System;

namespace NetCoreWs.Core
{
    abstract public class MessageHandlerBase
    {
        private volatile Action<object> _prevHandler;
        private volatile Action<object> _nextHandler;
        private volatile Action _channelActivated;

        public Action<object> PrevHandler
        {
            get => _prevHandler;
            set => _prevHandler = value;
        }

        public Action<object> NextHandler
        {
            get => _nextHandler;
            set => _nextHandler = value;
        }
        
        public Action ChannelActivated
        {
            get => _channelActivated;
            set => _channelActivated = value;
        }

        public bool IsActive { get; set; }

        public IPipeline Pipeline { get; set; }

        abstract public void OnChannelActivated();
        
        abstract public void HandleUpstreamMessage(object message);
        
        abstract public void HandleDownstreamMessage(object message);

        protected void UpstreamMessageHandled(object message)
        {
            Action<object> nextHandler = _nextHandler;
            if (nextHandler != null)
            {
                nextHandler(message);
            }
        }
        
        protected void DownstreamMessageHandled(object message)
        {
            Action<object> prevHandler = _prevHandler;
            if (prevHandler != null)
            {
                prevHandler(message);
            }
        }

        protected void FireChannelActivated()
        {
            Action channelActivated = _channelActivated;
            if (channelActivated != null)
            {
                channelActivated();
            }
        }
    }
}