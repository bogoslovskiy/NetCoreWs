using System;
using System.Runtime.InteropServices;
using System.Threading;
using NetCoreUv;
using NetCoreWs.Buffers;
using NetCoreWs.Buffers.Unmanaged;
using NetCoreWs.Channels;

namespace NetCoreWs.Uv
{
    abstract public class UvTcpSocketChannelBase<TChannelParameters> : ChannelBase<TChannelParameters>
        where TChannelParameters : class, new()
    {
        protected readonly UvTcpHandle UvTcpHandle;
        private readonly UvWriteRequestT<UnmanagedByteBuf> _writeRequest;
        private long _writeLock;
        
        // TODO:
        private readonly UnmanagedByteBufAllocator _byteBufProvider = new UnmanagedByteBufAllocator(4096);

        protected UvTcpSocketChannelBase()
        {
            UvTcpHandle = new UvTcpHandle();
            _writeRequest = new UvWriteRequestT<UnmanagedByteBuf>();
        }

        public override IByteBufProvider GetByteBufProvider()
        {
            return _byteBufProvider;
        }
        
        public override void Send(ByteBuf byteBuf)
        {
            var unmanagedByteBuf = (UnmanagedByteBuf) byteBuf;
            
            unmanagedByteBuf.GetReadable(out IntPtr ptr, out int len);

            //Console.WriteLine(unmanagedByteBuf.Dump(System.Text.Encoding.ASCII));

            var buf = new UvNative.uv_buf_t(ptr, len, PlatformApis.IsWindows);

            if (Interlocked.CompareExchange(ref _writeLock, 1, 0) != 0)
            {
                var spinWait = new SpinWait();
                while (Interlocked.CompareExchange(ref _writeLock, 1, 0) != 0)
                {
                    spinWait.SpinOnce();
                }
            }
            
            // TODO: обрабатывать статус с ошибкой.
            int writeResult = _writeRequest.Write(UvTcpHandle, buf, unmanagedByteBuf);
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
            _writeRequest.Init(WriteCallback);
        }
        
        private void WriteCallback(UnmanagedByteBuf byteBuf)
        {
            // TODO: обрабатывать статус с ошибкой.
            
            byteBuf.Release();

            Interlocked.Exchange(ref _writeLock, 0);
        }

        private void AllocCallback(
            UvStreamHandle streamHandle,
            int suggestedsize,
            out UvNative.uv_buf_t buf)
        {
            // Тут мы можем просто взять поинтер, без буфера. Все равно поинтер не потеряется и будет передан
            // в ReadCallback, где будет завернут в буфер.
            
            // TODO:
            var byteBuf = (UnmanagedByteBuf)_byteBufProvider.GetBuffer();
            int writable = byteBuf.WritableBytes();
            IntPtr memPtr;
            int readable;
            byteBuf.GetReadable(out memPtr, out readable);
            buf = new UvNative.uv_buf_t(memPtr, writable, PlatformApis.IsWindows);
        }
        
        private void ReadCallback(UvStreamHandle streamHandle, int status, ref UvNative.uv_buf_t buf)
        {
            if (status > 0)
            {
                UnmanagedByteBuf byteBuf = _byteBufProvider.WrapMemorySegment(buf.Memory, buf.Len);
                byteBuf.SetWrite(status);

                try
                {
                    this.Receive(byteBuf);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
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