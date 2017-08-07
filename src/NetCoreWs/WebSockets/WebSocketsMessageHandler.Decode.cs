using NetCoreWs.Buffers;

namespace NetCoreWs.WebSockets
{
    abstract public partial class WebSocketsMessageHandler
    {
        protected override WebSocketFrame Decode(ByteBuf inByteBuf)
        {
            if (inByteBuf.ReadableBytes() < 2)
            {
                throw new UnexpectedEndOfBufferException();
            }

            byte headerByte1 = inByteBuf.ReadByte();
            byte headerByte2 = inByteBuf.ReadByte();

            bool fin = (headerByte1 & WebSocketUtils.MaskFin) == WebSocketUtils.MaskFin;
            byte opCode = (byte) (headerByte1 & WebSocketUtils.MaskOpCode);
            bool useMask = (headerByte2 & WebSocketUtils.MaskMask) == WebSocketUtils.MaskMask;
            int payloadLen = (byte) (headerByte2 & WebSocketUtils.MaskPayloadLen);
            
            if (payloadLen > 125)
            {
                payloadLen = ReadExtPayloadLen(inByteBuf, payloadLen);
            }
            
            if (useMask)
            {
                _maskBytes = _maskBytes ?? new byte[4];
                ReadMask(inByteBuf, _maskBytes);
            }
            
            if (inByteBuf.ReadableBytes() < payloadLen)
            {
                throw new UnexpectedEndOfBufferException();
            }

            // TODO: provider
            byte[] binaryData = new byte[4096];
            
            for (int i = 0; i < payloadLen; i++)
            {
                if (useMask)
                {
                    binaryData[i] = (byte)(inByteBuf.ReadByte() ^ _maskBytes[i % 4]);
                }
                else
                {
                    binaryData[i] = inByteBuf.ReadByte();
                }
            }
            
            var frame = new WebSocketFrame();
            frame.IsFinal = fin;
            frame.Type = WebSocketUtils.GetFrameType(opCode);
            frame.BinaryData = binaryData;
            frame.DataLen = payloadLen;
            
            // TODO: release input byte buf

            return frame;
        }

        private int ReadExtPayloadLen(ByteBuf inByteBuf, int payloadLen)
        {
            if (payloadLen == 126)
            {
                if (inByteBuf.ReadableBytes() < 2)
                {
                    throw new UnexpectedEndOfBufferException();
                }

                return inByteBuf.ReadUShort();
            }
            
            throw new UnexpectedEndOfBufferException();
        }

        private void ReadMask(ByteBuf inByteBuf, byte[] maskBytes)
        {
            if (inByteBuf.ReadableBytes() < 4)
            {
                throw new UnexpectedEndOfBufferException();
            }

            maskBytes[0] = inByteBuf.ReadByte();
            maskBytes[1] = inByteBuf.ReadByte();
            maskBytes[2] = inByteBuf.ReadByte();
            maskBytes[3] = inByteBuf.ReadByte();
        }
    }
}