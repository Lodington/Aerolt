using WebSocketSharp.Net.WebSockets;

namespace Aerolt_External;

public interface IWebsocketCommand
{
    void Execute(WebSocketContext context);
}