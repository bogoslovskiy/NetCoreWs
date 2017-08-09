using System;

namespace NetCoreWs.Core
{
    abstract public class MessageHandlerBase
    {
        private volatile Action<object> _prevHandler;
        private volatile Action<object> _nextHandler;

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

        public IPipeline Pipeline { get; set; }

        abstract public void HandleUpstreamMessage(object message);
        
        abstract public void HandleDownstreamMessage(object message);

        protected void UpstreamMessageHandled(object message)
        {
            _nextHandler(message);
        }
        
        protected void DownstreamMessageHandled(object message)
        {
            _prevHandler(message);
        }
    }
}