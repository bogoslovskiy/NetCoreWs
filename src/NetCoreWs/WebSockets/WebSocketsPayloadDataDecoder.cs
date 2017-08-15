using System;
using NetCoreWs.Buffers;
using NetCoreWs.Codecs;
using NetCoreWs.WebSockets.Decoder;

namespace NetCoreWs.WebSockets
{
    public class WebSocketsPayloadDataDecoder : ByteToMessageDecoder<ByteBuf>
    {
        private readonly WebSocketDecoderStateMachine _decoderStateMachine = new WebSocketDecoderStateMachine();
     
        // Храним ссылку на буфер на время жизни декодера. Как только декодер будет передан в пул 
        // (если есть пуллинг декодеров) или будет финализирован сборщиком, буфер надо отдать в пул.
        private ByteBuf _byteBuf;
        
        ~WebSocketsPayloadDataDecoder()
        {
            // Если клиент отключится от канала, то декодер будет финализирован (пока нет пуллинга).
            // Буфер чтения данных при этом можно аккуратно освободить (вернуть в пул).
            _byteBuf.Release();
        }
        
        protected override ByteBuf DecodeOne(ByteBuf byteBuf)
        {
            // Сохраняем ссылку на буфер, чтобы иметь возможность полностью освободить его, при деконструкции декодера.
            _byteBuf = byteBuf;
            
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
            // TODO: NRE
            //byteBuf.ReleaseReaded();
            
            return outputByteBuf;
        }
    }
}