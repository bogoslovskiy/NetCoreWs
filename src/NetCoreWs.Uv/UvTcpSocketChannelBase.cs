using System;
using System.Runtime.InteropServices;
using NetCoreUv;
using NetCoreWs.Buffers;
using NetCoreWs.Buffers.Experimental;
using NetCoreWs.Channels;

namespace NetCoreWs.Uv
{
    abstract public class UvTcpSocketChannelBase<TChannelParameters> : ChannelBase<TChannelParameters>
        where TChannelParameters : class, new()
    {
        protected readonly UvTcpHandle UvTcpHandle;
//        private readonly UvWriteRequestT<UnmanagedByteBuf> _writeRequest;
        
        // TODO:
        private readonly LockFreeUnmanagedByteBufProvider _byteBufProvider = 
            new LockFreeUnmanagedByteBufProvider(65536, 4096, 1000);

        protected UvTcpSocketChannelBase()
        {
            UvTcpHandle = new UvTcpHandle();
//            _writeRequest = new UvWriteRequestT<UnmanagedByteBuf>();
        }

        public override IByteBufProvider GetByteBufProvider()
        {
            return _byteBufProvider;
        }
        
        public override void Send(ByteBuf byteBuf)
        {
            var unmanagedByteBuf = (IUnmanagedByteBuf) byteBuf;
            
            unmanagedByteBuf.GetReadable(out IntPtr ptr, out int len);

            var buf = new UvNative.uv_buf_t(ptr, len, PlatformApis.IsWindows);

            UvTcpHandle.TryWrite(buf);
            
//            // TODO: обрабатывать статус с ошибкой.
//            UvWriteRequestT<ByteBuf> writeRequest = new UvWriteRequestT<ByteBuf>();
//            writeRequest.Init(WriteCallback);
//            int writeResult = writeRequest.Write(UvTcpHandle, buf, byteBuf);
        }
        
        public void StartRead()
        {
            FireActivated();
            UvTcpHandle.ReadStart(AllocCallback, ReadCallback);
        }

        public void StopRead()
        {
            UvTcpHandle.ReadStop();
        }

        internal void InitUv(UvLoopHandle uvLoop)
        {
            UvTcpHandle.Init(uvLoop);
//            _writeRequest.Init(WriteCallback);
        }
        
//        private void WriteCallback(UnmanagedByteBuf byteBuf)
//        {
//            // TODO: обрабатывать статус с ошибкой.
//            
//            byteBuf.Release();
//        }

        private void AllocCallback(
            UvStreamHandle streamHandle,
            int suggestedsize,
            out UvNative.uv_buf_t buf)
        {
            // Тут мы можем просто взять поинтер, без буфера. Все равно поинтер не потеряется и будет передан
            // в ReadCallback, где будет завернут в буфер.
            try
            {
                IntPtr dataPtr = _byteBufProvider.GetDefaultDataIntPtr(out int size);
                buf = new UvNative.uv_buf_t(dataPtr, size, PlatformApis.IsWindows);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                streamHandle.ReadStop();
                streamHandle.Close();
                
                buf = new UvNative.uv_buf_t();
            }
        }
        
        private void ReadCallback(UvStreamHandle streamHandle, int status, ref UvNative.uv_buf_t buf)
        {
            if (status > 0)
            {
                try
                {
                    MemorySegmentByteBuffer byteBuf = _byteBufProvider.WrapMemorySegment(buf.Memory, buf.Len);
                    byteBuf.SetWrite(status);

                    this.Receive(byteBuf);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    streamHandle.ReadStop();
                    streamHandle.Close();
                }
            }
            else if (status == 0)
            {
                // TODO:
                Console.WriteLine("ReadCallback. No data to read.");
            }
            else
            {
                string error = string.Format(
                    "Error #{0}. {1} {2}",
                    status,
                    Marshal.PtrToStringAnsi(UvNative.uv_err_name(status)),
                    Marshal.PtrToStringAnsi(UvNative.uv_strerror(status))
                );
                // TODO:
                Console.WriteLine("ReadCallback. {0}.", error);
            }
        }
    }
}