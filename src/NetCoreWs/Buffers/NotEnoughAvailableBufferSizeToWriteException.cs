using System;

namespace NetCoreWs.Buffers
{
    public class NotEnoughAvailableBufferSizeToWriteException : Exception
    {
        public NotEnoughAvailableBufferSizeToWriteException(int bufferWritable, int dataLen)
            : base(
                $"Not enough available buffer size to write data. Buffer writable size = {bufferWritable}. " +
                $"Data lenght = {dataLen}."
            )
        {
        }
    }
}