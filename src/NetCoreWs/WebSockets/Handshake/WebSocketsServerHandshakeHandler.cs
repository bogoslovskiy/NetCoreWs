using System;
using NetCoreWs.Buffers;
using NetCoreWs.Core;

namespace NetCoreWs.WebSockets.Handshake
{
    public class WebSocketsServerHandshakeHandler : SimplexUpstreamMessageHandler<ByteBuf>
    {
        static private readonly int ConnectionHeaderLen = HttpHeaderConstants.ConnectionLower.Length;
        static private readonly int UpgradeHeaderLen = HttpHeaderConstants.UpgradeLower.Length;
        static private readonly int SecWebSocketVersionHeaderLen = HttpHeaderConstants.SecWebsocketVersionLower.Length;
        static private readonly int SecWebSocketKeyHeaderLen = HttpHeaderConstants.SecWebsocketKeyLower.Length;
        
        private const int ConnectionHeaderMask = 1;
        private const int ConnectionHeaderValueMask = 1 << 1;
        private const int UpgradeHeaderMask = 1 << 2;
        private const int UpgradeHeaderValueMask = 1 << 3;
        private const int SecWebSocketVersionHeaderMask = 1 << 4;
        private const int SecWebSocketVersionHeaderValueMask = 1 << 5;
        private const int SecWebSocketKeyHeaderMask = 1 << 6;
        private const int SecWebSocketKeyHeaderValueMask = 1 << 7;
        
        private byte _headerNameValueMatchBits;
        
        private byte[] _key = new byte[92];
        private int _keyLen;

        public override void OnChannelActivated()
        {
        }
        
        protected override void HandleUpstreamMessage(ByteBuf message)
        {
            _headerNameValueMatchBits = 0;
            
            SkipToCrLf(message);

            // Освобождаем буфер.
            message.Release();
            
            bool handshaked = _headerNameValueMatchBits == byte.MaxValue;
            if (handshaked)
            {
                ByteBuf outByteBuf = this.Pipeline.GetBuffer();
                SwitchingProtocolResponse.Get(outByteBuf, _key, _keyLen);

                this.Pipeline.DeactivateHandler(this);
                
                DownstreamMessageHandled(outByteBuf);

                FireChannelActivated();
            }
            else
            {
                // TODO: анализ рукопожатия + bad response.
                throw new Exception();
            }
        }

        private void SkipToCrLf(ByteBuf message)
        {
            int skipped = message.SkipTo(
                HttpHeaderConstants.CR /* stopByte1 */,
                HttpHeaderConstants.LF /* stopByte2 */,
                true /* include */
            );

            if (skipped < 0)
            {
                // TODO: 
                throw new HandshakeException("");
            }

            CrLf(message);
        }

        private void CrLf(ByteBuf message)
        {
            // Далее как минимум должны следовать либо 2 байта CRLF, либо следующий заголовок со значением,
            // где тоже должно быть гораздо больше байт, с учетом того,
            // что название и значение разделено 2 байтами ": ".
            if (message.ReadableBytes() < 2)
            {
                return;
            }

            // Читаем следующие 2 байта.
            byte nextByte1 = message.ReadByte();
            byte nextByte2 = message.ReadByte();

            // Если CRLF - значит это второй CRLF, который по стандарту означает начало тела HTTP.
            // Но в нашем случае, тело нас не интересует, мы прочитали заголовки.
            if (nextByte1 == HttpHeaderConstants.CR && nextByte2 == HttpHeaderConstants.LF)
            {
                return;
            }

            // TODO: нормальные исключения
            if (nextByte1 == HttpHeaderConstants.CR && nextByte2 != HttpHeaderConstants.LF)
            {
                // TODO: 
                throw new HandshakeException("");
            }

            if (nextByte1 != HttpHeaderConstants.CR && nextByte2 == HttpHeaderConstants.LF)
            {
                // TODO: 
                throw new HandshakeException("");
            }

            // Если далее идут не CRLF, значит идет следующий заголовок.
            // Возвращаем чтение буфера на 2 байта обратно.
            message.Back(2);

            MatchHeaderName(message);
        }

        private void MatchHeaderName(ByteBuf message)
        {
            bool colonAndWhitespace = false;
            bool crlf = false;
            bool allNotMatched = false;

            bool firstByte = false;
            bool lastByteIsColon = false;
            bool lastByteIsCr = false;
            int index = 0;

            bool localConnectionHeaderMatched = false;
            bool localUpgradeHeaderMatched = false;
            bool localSecWebSocketVersionHeaderMatched = false;
            bool localSecWebSocketKeyHeaderMatched = false;

            bool skipConnectionHeader = false;
            bool skipUpgradeHeader = false;
            bool skipSecWebSocketVersionHeader = false;
            bool skipSecWebSocketKeyHeader = false;

            while (message.ReadableBytes() > 0)
            {
                if (!firstByte)
                {
                    firstByte = true;

                    localConnectionHeaderMatched = true;
                    localUpgradeHeaderMatched = true;
                    localSecWebSocketVersionHeaderMatched = true;
                    localSecWebSocketKeyHeaderMatched = true;
                }

                byte nextByte = message.ReadByte();

                #region ": "

                if (nextByte == HttpHeaderConstants.Whitespace)
                {
                    if (lastByteIsColon)
                    {
                        colonAndWhitespace = true;
                        break;
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                if (nextByte == HttpHeaderConstants.Colon)
                {
                    lastByteIsColon = true;
                    continue;
                }
                else
                {
                    lastByteIsColon = false;
                    colonAndWhitespace = false;
                }

                #endregion

                #region CRLF

                if (nextByte == HttpHeaderConstants.LF)
                {
                    if (lastByteIsCr)
                    {
                        crlf = true;
                        break;
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                if (nextByte == HttpHeaderConstants.CR)
                {
                    lastByteIsCr = true;
                    continue;
                }
                else
                {
                    lastByteIsCr = false;
                    crlf = false;
                }

                #endregion

                #region Headers matching

                if (!skipConnectionHeader)
                {
                    localConnectionHeaderMatched &=
                        ConnectionHeaderLen > index &&
                        (nextByte == HttpHeaderConstants.ConnectionLower[index] ||
                         nextByte == HttpHeaderConstants.ConnectionUpper[index]);
                    if (!localConnectionHeaderMatched)
                    {
                        skipConnectionHeader = true;
                    }
                }

                if (!skipUpgradeHeader)
                {
                    localUpgradeHeaderMatched &=
                        UpgradeHeaderLen > index &&
                        (nextByte == HttpHeaderConstants.UpgradeLower[index] ||
                         nextByte == HttpHeaderConstants.UpgradeUpper[index]);
                    if (!localUpgradeHeaderMatched)
                    {
                        skipUpgradeHeader = true;
                    }
                }

                if (!skipSecWebSocketVersionHeader)
                {
                    localSecWebSocketVersionHeaderMatched &=
                        SecWebSocketVersionHeaderLen > index &&
                        (nextByte == HttpHeaderConstants.SecWebsocketVersionLower[index] ||
                         nextByte == HttpHeaderConstants.SecWebsocketVersionUpper[index]);
                    if (!localSecWebSocketVersionHeaderMatched)
                    {
                        skipSecWebSocketVersionHeader = true;
                    }
                }

                if (!skipSecWebSocketKeyHeader)
                {
                    localSecWebSocketKeyHeaderMatched &=
                        SecWebSocketKeyHeaderLen > index &&
                        (nextByte == HttpHeaderConstants.SecWebsocketKeyLower[index] ||
                         nextByte == HttpHeaderConstants.SecWebsocketKeyUpper[index]);
                    if (!localSecWebSocketKeyHeaderMatched)
                    {
                        skipSecWebSocketKeyHeader = true;
                    }
                }

                #endregion

                allNotMatched =
                    !localConnectionHeaderMatched &&
                    !localUpgradeHeaderMatched &&
                    !localSecWebSocketVersionHeaderMatched &&
                    !localSecWebSocketKeyHeaderMatched;
                if (allNotMatched)
                {
                    break;
                }

                index++;
            }

            if (crlf)
            {
                throw new Exception();
            }

            if (allNotMatched)
            {
                SkipToCrLf(message);
                return;
            }

            if (colonAndWhitespace)
            {
                if (localConnectionHeaderMatched)
                {
                    _headerNameValueMatchBits = (byte)(_headerNameValueMatchBits | ConnectionHeaderMask);
                    
                    MatchHeaderValue(
                        message,
                        HttpHeaderConstants.UpgradeLower,
                        HttpHeaderConstants.UpgradeUpper,
                        out bool macthed,
                        out bool notMatched,
                        out bool valueEndWithCrlf
                    );

                    if (macthed)
                    {
                        _headerNameValueMatchBits = (byte)(_headerNameValueMatchBits | ConnectionHeaderValueMask);
                        
                        if (valueEndWithCrlf)
                        {
                            CrLf(message);
                        }
                        else
                        {
                            SkipToCrLf(message);
                        }
                    }

                    if (notMatched)
                    {
                        _headerNameValueMatchBits = (byte)(_headerNameValueMatchBits & ~ConnectionHeaderValueMask);
                    }
                }
                else if (localUpgradeHeaderMatched)
                {
                    _headerNameValueMatchBits = (byte)(_headerNameValueMatchBits | UpgradeHeaderMask);
                    
                    MatchHeaderValue(
                        message,
                        HttpHeaderConstants.WebsocketLower,
                        HttpHeaderConstants.WebsocketUpper,
                        out bool macthed,
                        out bool notMatched,
                        out bool valueEndWithCrlf
                    );

                    if (macthed)
                    {
                        _headerNameValueMatchBits = (byte)(_headerNameValueMatchBits | UpgradeHeaderValueMask);
                        if (valueEndWithCrlf)
                        {
                            CrLf(message);
                        }
                        else
                        {
                            SkipToCrLf(message);
                        }
                    }

                    if (notMatched)
                    {
                        _headerNameValueMatchBits = (byte)(_headerNameValueMatchBits & ~UpgradeHeaderValueMask);
                    }
                }
                else if (localSecWebSocketVersionHeaderMatched)
                {
                    _headerNameValueMatchBits = (byte)(_headerNameValueMatchBits | SecWebSocketVersionHeaderMask);
                    
                    MatchHeaderValue(
                        message,
                        HttpHeaderConstants.Version13,
                        HttpHeaderConstants.Version13,
                        out bool macthed,
                        out bool notMatched,
                        out bool valueEndWithCrlf
                    );

                    if (macthed)
                    {
                        _headerNameValueMatchBits = 
                            (byte)(_headerNameValueMatchBits | SecWebSocketVersionHeaderValueMask);
                        if (valueEndWithCrlf)
                        {
                            CrLf(message);
                        }
                        else
                        {
                            SkipToCrLf(message);
                        }
                    }

                    if (notMatched)
                    {
                        _headerNameValueMatchBits = 
                            (byte)(_headerNameValueMatchBits & ~SecWebSocketVersionHeaderValueMask);
                    }
                }
                else if (localSecWebSocketKeyHeaderMatched)
                {
                    _headerNameValueMatchBits = (byte)(_headerNameValueMatchBits | SecWebSocketKeyHeaderMask);
                    SecWebSocketKey(message);
                }
                else
                {
                    SkipToCrLf(message);
                }
            }
        }

        private void MatchHeaderValue(
            ByteBuf message, 
            byte[] headerValueLower, 
            byte[] headerValueUpper,
            out bool matched,
            out bool notMatched,
            out bool crlf)
        {
            crlf = false;
            matched = false;
            notMatched = false;

            int headerValueLen = headerValueLower.Length;

            int index = 0;
            bool firstByte = false;
            bool lastByteIsCr = false;

            bool headerValueMatched = false;
            bool headerValueMatchedCurrent = false;
            bool skipToNextCommaAndWhitespace = false;

            while (message.ReadableBytes() > 0)
            {
                if (!firstByte)
                {
                    firstByte = true;
                    index = 0;
                    headerValueMatched = true;
                    headerValueMatchedCurrent = false;
                    skipToNextCommaAndWhitespace = false;
                }

                byte nextByte = message.ReadByte();

                #region CRLF

                if (nextByte == HttpHeaderConstants.LF)
                {
                    if (lastByteIsCr)
                    {
                        crlf = true;
                        skipToNextCommaAndWhitespace = false;
                        break;
                    }

                    // TODO:
                    throw new Exception();
                }
                if (nextByte == HttpHeaderConstants.CR)
                {
                    lastByteIsCr = true;
                    continue;
                }

                lastByteIsCr = false;

                #endregion

                if (nextByte == HttpHeaderConstants.Comma || nextByte == HttpHeaderConstants.Whitespace)
                {
                    if (headerValueMatchedCurrent)
                    {
                        matched = true;
                        skipToNextCommaAndWhitespace = false;
                        break;
                    }

                    firstByte = false;
                    continue;
                }

                if (!skipToNextCommaAndWhitespace)
                {
                    headerValueMatched &=
                        headerValueLen > index &&
                        (nextByte == headerValueLower[index] ||
                         nextByte == headerValueUpper[index]);
                    if (!headerValueMatched)
                    {
                        headerValueMatchedCurrent = false;
                        skipToNextCommaAndWhitespace = true;
                    }
                    else
                    {
                        if (index == headerValueLen - 1)
                        {
                            headerValueMatchedCurrent = true;
                        }
                    }
                }

                index++;
            }

            matched |= crlf && headerValueMatchedCurrent;

            if (!matched && !headerValueMatched && !skipToNextCommaAndWhitespace)
            {
                notMatched = true;
            }
        }

        private void SecWebSocketKey(ByteBuf message)
        {
            int read = message.ReadToOrRollback(
                HttpHeaderConstants.CR,
                HttpHeaderConstants.LF,
                _key /* output */,
                0 /* startIndex */,
                _key.Length /* len */
            );

            if (read < 0)
            {
                // TODO: 
                throw new Exception();
            }

            // Если буфер смог дочитать до CRLF, значит в нем точно есть еще как минимум 2 байта CRLF.
            // Читаем их, чтобы сдвинуть.
            message.ReadByte();
            message.ReadByte();

            _headerNameValueMatchBits = (byte)(_headerNameValueMatchBits | SecWebSocketKeyHeaderValueMask);
            _keyLen = read;

            CrLf(message);
        }
    }
}