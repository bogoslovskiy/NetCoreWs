using System;

namespace NetCoreWs.Channels
{
    abstract public class ChannelBase<TChannelParameters> : ChannelBase
        where TChannelParameters : class, new()
    {
        public TChannelParameters Parameters { get; private set; }
        
        public void Init(Action<TChannelParameters> initChannelParameters)
        {
            this.Parameters = new TChannelParameters();
            initChannelParameters(this.Parameters);
        }
    }
}