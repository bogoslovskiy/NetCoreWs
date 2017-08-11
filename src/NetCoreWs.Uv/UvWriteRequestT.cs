using System;
using NetCoreUv;

namespace NetCoreWs.Uv
{
    public class UvWriteRequestT<T> : UvWriteRequest
    {
        private T _context;
        private Action<T> _writeCallback;

        public void Init(Action<T> writeCallback)
        {
            _writeCallback = writeCallback;
            
            base.Init();
        }

        public int Write(UvTcpHandle tcpHandle, UvNative.uv_buf_t buf, T context)
        {
            _context = context;

            return base.Write(tcpHandle, buf);
        }

        protected override void OnWrited()
        {
            _writeCallback(_context);
        }
    }
}