using System;
using System.Collections.Generic;
using NetCoreWs.Buffers;
using NetCoreWs.Core;

namespace NetCoreWs.Codecs
{
    abstract public class ByteToMessageDecoder<TMessage> : DuplexMessageHandler<ByteBuf, TMessage>
    {
        private ByteBuf _cumulatedByteBuf;
        
        [ThreadStatic]
        static private List<TMessage> _output;
        
        private const int OutputBatchSize = 200;
        
        abstract protected TMessage DecodeOne(ByteBuf byteBuf);
        
        public override void OnChannelActivated()
        {
            FireChannelActivated();
        }

        protected override void HandleUpstreamMessage(ByteBuf inputMessage)
        {
            // Объединяем буферы, если предыдущий буфер не был прочитан до конца.
            if (_cumulatedByteBuf != null)
            {
                _cumulatedByteBuf.Append(inputMessage);
                inputMessage = _cumulatedByteBuf;
                _cumulatedByteBuf = null;
            }

            _output = _output ?? new List<TMessage>(OutputBatchSize);
            _output.Clear();
            
            // Пока декодер возвращает объект и в буфере есть данные для чтения, есть возможность декодировать следующий
            // объект.
            // Если же декодер не вернул объект или буфер опустошен, то декодирование можно прервать до поступления
            // следующей порции данных для обработки в новом буфере.
            TMessage outputMessage;
            do
            {
                outputMessage = DecodeOne(inputMessage);
                if (outputMessage != null)
                {
                    if (_output.Count == OutputBatchSize - 1)
                    {
                        foreach (TMessage message in _output)
                        {
                            UpstreamMessageHandled(message);
                        }
                        
                        _output.Clear();
                    }
                    
                    _output.Add(outputMessage);
                }
            }
            while (outputMessage != null && inputMessage.ReadableBytes() > 0);
            
            foreach (TMessage message in _output)
            {
                UpstreamMessageHandled(message);
            }
            
            // Если буфер не освобожден и в нем есть данные для чтения, 
            // буфер должен объединиться со следующим буфером.
            if (!inputMessage.Released && inputMessage.ReadableBytes() > 0)
            {
                _cumulatedByteBuf = inputMessage;
            }
            
            if (!inputMessage.Released && inputMessage.ReadableBytes() == 0)
            {
                inputMessage.Release();
            }
        }

        protected override void HandleDownstreamMessage(TMessage message)
        {
            DownstreamMessageHandled(message);
        }
    }
}