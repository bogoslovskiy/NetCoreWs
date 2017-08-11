using System;

namespace NetCoreWs.Core
{
    abstract public class DuplexMessageHandler<TInputMsg, TOutputMsg> : MessageHandlerBase
    {
        abstract protected void HandleUpstreamMessage(TInputMsg message);
        
        abstract protected void HandleDownstreamMessage(TOutputMsg message);
        
        public sealed override void HandleUpstreamMessage(object message)
        {
            if (message != null)
            {
                if (message is TInputMsg)
                {
                    HandleUpstreamMessage((TInputMsg)message);
                    return;
                }
                
                throw new InvalidOperationException($"Upstream message must be instance of {typeof(TInputMsg)}");
            }
            
            throw new ArgumentNullException(nameof(message));
        }

        public sealed override void HandleDownstreamMessage(object message)
        {
            if (message != null)
            {
                if (message is TOutputMsg)
                {
                    HandleDownstreamMessage((TOutputMsg)message);
                    return;
                }
                
                throw new InvalidOperationException($"Downstream message must be instance of {typeof(TOutputMsg)}");
            }
            
            throw new ArgumentNullException(nameof(message));
        }
    }
}