using System;

namespace NetCoreWs.Buffers
{
    public class UnexpectedEndOfBufferException : Exception
    {
        public UnexpectedEndOfBufferException()
            : base("Unexpected end of byte buffer.")
        {
        }
    }
}