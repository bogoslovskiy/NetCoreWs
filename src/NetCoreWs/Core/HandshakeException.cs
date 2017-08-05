using System;

namespace NetCoreWs.Core
{
    public class HandshakeException : Exception
    {
        public HandshakeException(string message)
            : base(message)
        {
        }
    }
}