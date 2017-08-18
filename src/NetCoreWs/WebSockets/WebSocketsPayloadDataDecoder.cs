using System;
using NetCoreWs.Buffers;
using NetCoreWs.Codecs;
using NetCoreWs.WebSockets.Decoder;

namespace NetCoreWs.WebSockets
{
    public class WebSocketsPayloadDataDecoder : ByteToMessageDecoder<ByteBuf>
    {
        private readonly WebSocketDecoderStateMachine _decoderStateMachine = new WebSocketDecoderStateMachine();
        
        protected override ByteBuf DecodeOne(ByteBuf byteBuf)
        {
            WebSocketFrameInfo? frameInfo;
            _decoderStateMachine.Read(byteBuf, out frameInfo);

            ByteBuf outputByteBuf = null;

            if (frameInfo != null)
            {
                WebSocketFrameInfo info = (WebSocketFrameInfo) frameInfo;

                outputByteBuf = this.Pipeline.GetBuffer();

                for (int i = 0; i < info.PayloadDataLen; i++)
                {
                    if (info.Masked)
                    {
                        outputByteBuf.Write((byte)(byteBuf.ReadByte() ^ info.MaskBytes[i % 4]));
                    }
                    else
                    {
                        outputByteBuf.Write(byteBuf.ReadByte());
                    }
                }
                
                //Console.WriteLine(outputByteBuf.Dump(System.Text.Encoding.UTF8));
            }
            
            // Как минимум мы можем освободить прочитанную часть.
            // Буфер при этом не освободится полностью.
            byteBuf.ReleaseReaded();
            
            return outputByteBuf;
        }
    }
}