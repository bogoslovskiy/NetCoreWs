using System;
using System.Threading.Tasks;
using NetCoreWs.Buffers;
using NetCoreWs.Core;

namespace NetCoreWs.WebSockets
{
    abstract public partial class WebSocketsMessageHandler
    {
        static private readonly int ConnectionHeaderLen = HttpHeaderConstants.ConnectionLower.Length;
        static private readonly int UpgradeHeaderLen = HttpHeaderConstants.UpgradeLower.Length;
        static private readonly int SecWebSocketVersionHeaderLen = HttpHeaderConstants.SecWebsocketVersionLower.Length;
        static private readonly int SecWebSocketKeyHeaderLen = HttpHeaderConstants.SecWebsocketKeyLower.Length;
        
        private bool _connectionHeaderMatched;
        private bool _connectionHeaderValueMatched;
        private bool _upgradeHeaderMatched;
        private bool _upgradeHeaderValueMatched;
        private bool _secWebSocketVersionHeaderMatched;
        private bool _secWebSocketVersionHeaderValueMatched;
        private bool _secWebSocketKeyHeaderMatched;
        private bool _secWebSocketKeyHeaderValueMatched;
        private byte[] _key = new byte[92];
        private int _keyLen;
        
        protected override void ServerReceiveHandshake(ByteBuf inByteBuf)
        {
            SkipToCrLf(inByteBuf);

            bool handshaked = HandshakeMatched();

            if (handshaked)
            {
                ByteBuf outByteBuf = Channel.GetByteBufProvider().GetBuffer();
                SwitchingProtocolResponse.Get(outByteBuf, _key, _keyLen);

                SendByteMessage(outByteBuf);

                Handshaked = true;
            }
            else
            {
                // TODO: анализ рукопожатия + bad response.
                throw new Exception();
            }
        }
        
        private bool HandshakeMatched()
        {
            return
                _connectionHeaderMatched &
                _connectionHeaderValueMatched &
                _upgradeHeaderMatched &
                _upgradeHeaderValueMatched &
                _secWebSocketVersionHeaderMatched &
                _secWebSocketVersionHeaderValueMatched &
                _secWebSocketKeyHeaderMatched &
                _secWebSocketKeyHeaderValueMatched;
        }

        private void SkipToCrLf(ByteBuf inByteBuf)
        {
            int skipped = inByteBuf.SkipTo(
                HttpHeaderConstants.CR /* stopByte1 */,
                HttpHeaderConstants.LF /* stopByte2 */,
                true /* include */
            );

            if (skipped < 0)
            {
                // TODO: 
                throw new HandshakeException("");
            }

            CrLf(inByteBuf);
        }

        private void CrLf(ByteBuf inByteBuf)
        {
            // Далее как минимум должны следовать либо 2 байта CRLF, либо следующий заголовок со значением,
            // где тоже должно быть гораздо больше байт, с учетом того,
            // что название и значение разделено 2 байтами ": ".
            if (inByteBuf.ReadableBytes() < 2)
            {
                return;
            }

            // Читаем следующие 2 байта.
            byte nextByte1 = inByteBuf.ReadByte();
            byte nextByte2 = inByteBuf.ReadByte();

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
            inByteBuf.Back(2);

            MatchHeaderName(inByteBuf);
        }

        private void MatchHeaderName(ByteBuf inByteBuf)
        {
            bool colonAndWhitespace = false;
            bool crlf = false;
            bool allNotMatched = false;

            bool firstByte = false;
            bool lastByteIsColon = false;
            bool lastByteIsCR = false;
            int index = 0;

            bool _localConnectionHeaderMatched = false;
            bool _localUpgradeHeaderMatched = false;
            bool _localSecWebSocketVersionHeaderMatched = false;
            bool _localSecWebSocketKeyHeaderMatched = false;

            bool skipConnectionHeader = false;
            bool skipUpgradeHeader = false;
            bool skipSecWebSocketVersionHeader = false;
            bool skipSecWebSocketKeyHeader = false;

            while (inByteBuf.ReadableBytes() > 0)
            {
                if (!firstByte)
                {
                    firstByte = true;

                    _localConnectionHeaderMatched = true;
                    _localUpgradeHeaderMatched = true;
                    _localSecWebSocketVersionHeaderMatched = true;
                    _localSecWebSocketKeyHeaderMatched = true;
                }

                byte nextByte = inByteBuf.ReadByte();

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
                    if (lastByteIsCR)
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
                    lastByteIsCR = true;
                    continue;
                }
                else
                {
                    lastByteIsCR = false;
                    crlf = false;
                }

                #endregion

                #region Headers matching

                if (!skipConnectionHeader)
                {
                    _localConnectionHeaderMatched &=
                        ConnectionHeaderLen > index &&
                        (nextByte == HttpHeaderConstants.ConnectionLower[index] ||
                         nextByte == HttpHeaderConstants.ConnectionUpper[index]);
                    if (!_localConnectionHeaderMatched)
                    {
                        skipConnectionHeader = true;
                    }
                }

                if (!skipUpgradeHeader)
                {
                    _localUpgradeHeaderMatched &=
                        UpgradeHeaderLen > index &&
                        (nextByte == HttpHeaderConstants.UpgradeLower[index] ||
                         nextByte == HttpHeaderConstants.UpgradeUpper[index]);
                    if (!_localUpgradeHeaderMatched)
                    {
                        skipUpgradeHeader = true;
                    }
                }

                if (!skipSecWebSocketVersionHeader)
                {
                    _localSecWebSocketVersionHeaderMatched &=
                        SecWebSocketVersionHeaderLen > index &&
                        (nextByte == HttpHeaderConstants.SecWebsocketVersionLower[index] ||
                         nextByte == HttpHeaderConstants.SecWebsocketVersionUpper[index]);
                    if (!_localSecWebSocketVersionHeaderMatched)
                    {
                        skipSecWebSocketVersionHeader = true;
                    }
                }

                if (!skipSecWebSocketKeyHeader)
                {
                    _localSecWebSocketKeyHeaderMatched &=
                        SecWebSocketKeyHeaderLen > index &&
                        (nextByte == HttpHeaderConstants.SecWebsocketKeyLower[index] ||
                         nextByte == HttpHeaderConstants.SecWebsocketKeyUpper[index]);
                    if (!_localSecWebSocketKeyHeaderMatched)
                    {
                        skipSecWebSocketKeyHeader = true;
                    }
                }

                #endregion

                allNotMatched =
                    !_localConnectionHeaderMatched &&
                    !_localUpgradeHeaderMatched &&
                    !_localSecWebSocketVersionHeaderMatched &&
                    !_localSecWebSocketKeyHeaderMatched;
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
                SkipToCrLf(inByteBuf);
                return;
            }

            if (colonAndWhitespace)
            {
                if (_localConnectionHeaderMatched)
                {
                    _connectionHeaderMatched = true;
                    
                    MatchHeaderValue(
                        inByteBuf,
                        HttpHeaderConstants.UpgradeLower,
                        HttpHeaderConstants.UpgradeUpper,
                        out bool macthed,
                        out bool notMatched,
                        out bool valueEndWithCrlf
                    );

                    if (macthed)
                    {
                        _connectionHeaderValueMatched = true;
                        if (valueEndWithCrlf)
                        {
                            CrLf(inByteBuf);
                        }
                        else
                        {
                            SkipToCrLf(inByteBuf);
                        }
                    }

                    if (notMatched)
                    {
                        _connectionHeaderValueMatched = false;
                    }
                }
                else if (_localUpgradeHeaderMatched)
                {
                    _upgradeHeaderMatched = true;
                    
                    MatchHeaderValue(
                        inByteBuf,
                        HttpHeaderConstants.WebsocketLower,
                        HttpHeaderConstants.WebsocketUpper,
                        out bool macthed,
                        out bool notMatched,
                        out bool valueEndWithCrlf
                    );

                    if (macthed)
                    {
                        _upgradeHeaderValueMatched = true;
                        if (valueEndWithCrlf)
                        {
                            CrLf(inByteBuf);
                        }
                        else
                        {
                            SkipToCrLf(inByteBuf);
                        }
                    }

                    if (notMatched)
                    {
                        _upgradeHeaderValueMatched = false;
                    }
                }
                else if (_localSecWebSocketVersionHeaderMatched)
                {
                    _secWebSocketVersionHeaderMatched = true;
                    
                    MatchHeaderValue(
                        inByteBuf,
                        HttpHeaderConstants.Version13,
                        HttpHeaderConstants.Version13,
                        out bool macthed,
                        out bool notMatched,
                        out bool valueEndWithCrlf
                    );

                    if (macthed)
                    {
                        _secWebSocketVersionHeaderValueMatched = true;
                        if (valueEndWithCrlf)
                        {
                            CrLf(inByteBuf);
                        }
                        else
                        {
                            SkipToCrLf(inByteBuf);
                        }
                    }

                    if (notMatched)
                    {
                        _secWebSocketVersionHeaderValueMatched = false;
                    }
                }
                else if (_localSecWebSocketKeyHeaderMatched)
                {
                    _secWebSocketKeyHeaderMatched = true;
                    SecWebSocketKey(inByteBuf);
                }
                else
                {
                    SkipToCrLf(inByteBuf);
                }
            }
        }

        private void MatchHeaderValue(
            ByteBuf inByteBuf, 
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
            // ReSharper disable once InconsistentNaming
            bool lastByteIsCR = false;

            bool headerValueMatched = false;
            bool headerValueMatchedCurrent = false;
            bool skipToNextCommaAndWhitespace = false;

            while (inByteBuf.ReadableBytes() > 0)
            {
                if (!firstByte)
                {
                    firstByte = true;
                    index = 0;
                    headerValueMatched = true;
                    headerValueMatchedCurrent = false;
                    skipToNextCommaAndWhitespace = false;
                }

                byte nextByte = inByteBuf.ReadByte();

                #region CRLF

                if (nextByte == HttpHeaderConstants.LF)
                {
                    if (lastByteIsCR)
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
                    lastByteIsCR = true;
                    continue;
                }

                lastByteIsCR = false;

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

        private void SecWebSocketKey(ByteBuf inByteBuf)
        {
            int read = inByteBuf.ReadToOrRollback(
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
            inByteBuf.ReadByte();
            inByteBuf.ReadByte();

            _secWebSocketKeyHeaderValueMatched = true;
            _keyLen = read;

            CrLf(inByteBuf);
        }
    }
}