using NetCoreWs.Buffers;

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

//        static public void Encode(
//            ByteBuf byteBuf,
//            byte[] maskBytes,
//            WebSocketFrameType type,
//            bool fin,
//            bool masked,
//            int payloadLen)
//        {
//            int payloadDataLen = message.DataLen;
//
//            if (payloadDataLen > 65535)
//            {
//                throw new LargeWebSocketFramesNotSupportedException(payloadDataLen);
//            }
//            
//            int totalDataLen =
//                2 /* mandatoryHeader */ +
//                payloadDataLen /* payload */ +
//                (_useMask ? 4 : 0) /* mask */ +
//                (payloadDataLen <= 125 ? 0 : 2) /* ext payload */;
//
//            if (totalDataLen > outByteBuf.WritableBytes())
//            {
//                throw new NotEnoughAvailableBufferSizeToWriteException(outByteBuf.WritableBytes(), totalDataLen);
//            }
//            
//            byte opCode = Utils.GetFrameOpCode(message.Type);
//            if (message.IsFinal)
//            {
//                opCode = (byte)(opCode | Utils.MaskFin);
//            }
//
//            outByteBuf.Write(opCode);
//
//            byte payloadLenAndMask;
//
//            if (payloadDataLen <= 125)
//            {
//                payloadLenAndMask = (byte) payloadDataLen;
//            }
//            else
//            {
//                payloadLenAndMask = 126;
//            }
//
//            byte payloadLenByte = payloadLenAndMask;
//
//            if (_useMask)
//            {
//                payloadLenAndMask = (byte)(payloadLenAndMask | Utils.MaskMask);
//                SetMask(_maskBytes);
//            }
//
//            outByteBuf.Write(payloadLenAndMask);
//
//            if (payloadLenByte == 126)
//            {
//                ByteConverters.ByteUnion2 byteUnion2 = new ByteConverters.ByteUnion2();
//                byteUnion2.UShort = (ushort)payloadDataLen;
//                outByteBuf.Write(byteUnion2.B2);
//                outByteBuf.Write(byteUnion2.B1);
//            }
//
//            if (_useMask)
//            {
//                outByteBuf.Write(_maskBytes[3]);
//                outByteBuf.Write(_maskBytes[2]);
//                outByteBuf.Write(_maskBytes[1]);
//                outByteBuf.Write(_maskBytes[0]);
//            }
//
//            // TODO: оптимизация
//            for (int i = 0; i < payloadDataLen; i++)
//            {
//                if (_useMask)
//                {
//                    outByteBuf.Write((byte)(message.BinaryData[i] ^ _maskBytes[i % 4]));
//                }
//                else
//                {
//                    outByteBuf.Write(message.BinaryData[i]);
//                }
//            }
//        }
        
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