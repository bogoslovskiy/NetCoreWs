using NetCoreWs.Buffers;
using NetCoreWs.Utils;

namespace NetCoreWs.WebSockets
{
    abstract public partial class WebSocketsMessageHandler
    {
        abstract protected void SetMask(byte[] maskBytes);
        
        protected override void Encode(WebSocketFrame message, ByteBuf outByteBuf)
        {
            int payloadDataLen = message.ByteBuf.ReadableBytes();

            if (payloadDataLen > 65535)
            {
                throw new LargeWebSocketFramesNotSupportedException(payloadDataLen);
            }
            
            int totalDataLen =
                2 /* mandatoryHeader */ +
                payloadDataLen /* payload */ +
                (_useMask ? 4 : 0) /* mask */ +
                (payloadDataLen <= 125 ? 0 : 2) /* ext payload */;

            if (totalDataLen > outByteBuf.WritableBytes())
            {
                throw new NotEnoughAvailableBufferSizeToWriteException(outByteBuf.WritableBytes(), totalDataLen);
            }
            
            byte opCode = WebSocketUtils.GetFrameOpCode(message.Type);
            if (message.IsFinal)
            {
                opCode = (byte)(opCode | WebSocketUtils.MaskFin);
            }

            outByteBuf.Write(opCode);

            byte payloadLenAndMask;

            if (payloadDataLen <= 125)
            {
                payloadLenAndMask = (byte) payloadDataLen;
            }
            else if (payloadDataLen <= 65536)
            {
                payloadLenAndMask = 126;
            }

            byte payloadLen = payloadLenAndMask;

            if (_useMask)
            {
                payloadLenAndMask = (byte)(payloadLenAndMask | WebSocketUtils.MaskMask);
                SetMask(_maskBytes);
            }

            outByteBuf.Write(payloadLenAndMask);

            if (payloadLen == 126)
            {
                ByteConverters.ByteUnion2 byteUnion2 = new ByteConverters.ByteUnion2();
                byteUnion2.UShort = (ushort)payloadDataLen;
                outByteBuf.Write(byteUnion2.B2);
                outByteBuf.Write(byteUnion2.B1);
            }

            if (_useMask)
            {
                outByteBuf.Write(_maskBytes[3]);
                outByteBuf.Write(_maskBytes[2]);
                outByteBuf.Write(_maskBytes[1]);
                outByteBuf.Write(_maskBytes[0]);
            }

            // TODO: оптимизация
            for (int i = 0; i < payloadDataLen; i++)
            {
                if (_useMask)
                {
                    outByteBuf.Write((byte)(message.ByteBuf.ReadByte() ^ _maskBytes[i % 4]));
                }
                else
                {
                    outByteBuf.Write(message.ByteBuf.ReadByte());
                }
            }
        }
    }
}