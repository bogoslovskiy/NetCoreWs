using System;

namespace NetCoreWs.Core
{
    abstract public class SimplexUpstreamMessageHandler<TInputMsg> : MessageHandlerBase
    {
        abstract protected void HandleUpstreamMessage(TInputMsg message);
        
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
            throw new InvalidOperationException(
                "Simplex upstream message handler does not support downstream message handling."
            );
        }
    }
}