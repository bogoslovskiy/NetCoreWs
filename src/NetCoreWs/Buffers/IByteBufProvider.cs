namespace NetCoreWs.Buffers
{
    public interface IByteBufProvider
    {
        ByteBuf GetBuffer();
    }
}