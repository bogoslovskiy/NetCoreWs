namespace NetCoreWs.Buffers
{
    public class SimpleByteBufProvider : IByteBufProvider
    {
        public ByteBuf GetBuffer()
        {
            return new SimpleByteBuf(new byte[4096]);
        }
    }
}