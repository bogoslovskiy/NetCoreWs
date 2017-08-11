using NetCoreWs.Buffers;
using NetCoreWs.Utils;

namespace NetCoreWs.WebSockets
{
    static class Codec
    {
        static public void Decode(
            ByteBuf byteBuf,
            byte[] maskBytes,
            out WebSocketFrameType type,
            out bool fin,
            out bool masked,
            out int payloadLen)
        {
            if (byteBuf.ReadableBytes() < 2)
            {
                throw new UnexpectedEndOfBufferException();
            }

            byte headerByte1 = byteBuf.ReadByte();
            byte headerByte2 = byteBuf.ReadByte();

            fin = (headerByte1 & Utils.MaskFin) == Utils.MaskFin;
            byte opCode = (byte) (headerByte1 & Utils.MaskOpCode);
            masked = (headerByte2 & Utils.MaskMask) == Utils.MaskMask;
            payloadLen = (byte) (headerByte2 & Utils.MaskPayloadLen);

            type = Utils.GetFrameType(opCode);
            
            if (payloadLen > 125)
            {
                payloadLen = ReadExtPayloadLen(byteBuf, payloadLen);
            }
            
            if (masked)
            {
                ReadMask(byteBuf, maskBytes);
            }
            
            if (byteBuf.ReadableBytes() < payloadLen)
            {
                throw new UnexpectedEndOfBufferException();
            }
        }

        static public void Encode(
            ByteBuf outByteBuf,
            ByteBuf inByteBuf,
            byte[] maskBytes,
            WebSocketFrameType type,
            bool fin,
            bool masked,
            int payloadLen)
        {
            if (payloadLen > 65535)
            {
                throw new LargeWebSocketFramesNotSupportedException(payloadLen);
            }
            
            int totalDataLen =
                2 /* mandatoryHeader */ +
                payloadLen /* payload */ +
                (masked ? 4 : 0) /* mask */ +
                (payloadLen <= 125 ? 0 : 2) /* ext payload */;

            if (totalDataLen > outByteBuf.WritableBytes())
            {
                throw new NotEnoughAvailableBufferSizeToWriteException(outByteBuf.WritableBytes(), totalDataLen);
            }
            
            byte opCode = Utils.GetFrameOpCode(type);
            if (fin)
            {
                opCode = (byte)(opCode | Utils.MaskFin);
            }

            outByteBuf.Write(opCode);

            byte payloadLenAndMask;

            if (payloadLen <= 125)
            {
                payloadLenAndMask = (byte) payloadLen;
            }
            else
            {
                payloadLenAndMask = 126;
            }

            byte payloadLenByte = payloadLenAndMask;

            if (masked)
            {
                payloadLenAndMask = (byte)(payloadLenAndMask | Utils.MaskMask);
            }

            outByteBuf.Write(payloadLenAndMask);

            if (payloadLenByte == 126)
            {
                ByteConverters.ByteUnion2 byteUnion2 = new ByteConverters.ByteUnion2();
                byteUnion2.UShort = (ushort)payloadLen;
                outByteBuf.Write(byteUnion2.B2);
                outByteBuf.Write(byteUnion2.B1);
            }

            if (masked)
            {
                outByteBuf.Write(maskBytes[3]);
                outByteBuf.Write(maskBytes[2]);
                outByteBuf.Write(maskBytes[1]);
                outByteBuf.Write(maskBytes[0]);
            }

            // TODO: оптимизация
            for (int i = 0; i < payloadLen; i++)
            {
                byte @byte = inByteBuf.ReadByte();
                
                if (masked)
                {
                    outByteBuf.Write((byte)(@byte ^ maskBytes[i % 4]));
                }
                else
                {
                    outByteBuf.Write(@byte);
                }
            }
        }
        
        static private int ReadExtPayloadLen(ByteBuf inByteBuf, int payloadLen)
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

        static private void ReadMask(ByteBuf inByteBuf, byte[] maskBytes)
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