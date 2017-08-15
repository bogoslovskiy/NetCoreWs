namespace NetCoreWs.WebSockets
{
    public enum WebSocketFrameType
    {
        Continuation,
        Text,
        Binary,
        Close,
        Ping,
        Pong
    }
}