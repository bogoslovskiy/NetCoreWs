namespace NetCoreWs.Buffers
{
    public class SimpleByteBufNode
    {
        public byte[] Data;
        public int Used;
        public int Size;
        public SimpleByteBufNode Prev;
        public SimpleByteBufNode Next;
    }
}