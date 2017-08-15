﻿using System;
using NetCoreWs.Buffers;
using NetCoreWs.Core;
using NetCoreWs.WebSockets.Decoder;

namespace NetCoreWs.WebSockets
{
    public class WebSocketsPayloadDataHandler0 : DuplexMessageHandler<ByteBuf, ByteBuf>
    {
        [ThreadStatic]
        static private byte[] _maskBytes;
        
        public override void OnChannelActivated()
        {
            FireChannelActivated();
        }
        
        protected override void HandleUpstreamMessage(ByteBuf message)
        {
            _maskBytes = _maskBytes ?? new byte[4];

            Codec.Decode(
                message,
                _maskBytes,
                out WebSocketFrameType frameType,
                out bool fin,
                out bool masked,
                out int payloadLen
            );
                
            // TODO: оптимизировать (возможно передавать тот же буфер)
            ByteBuf payloadDataByteBuf = this.Pipeline.GetBuffer();

            for (int i = 0; i < payloadLen; i++)
            {
                byte @byte = message.ReadByte();

                if (masked)
                {
                    @byte ^= _maskBytes[i % 4];
                }
                
                payloadDataByteBuf.Write(@byte);
            }

            if (message.ReadableBytes() > 0)
            {
                throw new Exception($"В буфере еще остались данные. {message.ReadableBytes()}");
            }
                
                
            // Освобождаем буфер.
            message.Release();
            
            UpstreamMessageHandled(payloadDataByteBuf);
        }

        protected override void HandleDownstreamMessage(ByteBuf message)
        {
            int payloadLen = message.ReadableBytes();
            
            ByteBuf outByteBuf = this.Pipeline.GetBuffer();

            Codec.Encode(
                outByteBuf,
                message,
                null /* maskBytes */,
                WebSocketFrameType.Text,
                true /* fin */,
                false /* masked */,
                payloadLen
            );
            
            // Освобождаем буфер.
            message.Release();
            
            DownstreamMessageHandled(outByteBuf);
        }
    }
}