using WebSocketSharp.Net.WebSockets;

namespace Aerolt_External;

public interface IWebsocketCommand
{
    string CommandName { get; }
    void Execute(string payload, WebSocketContext context);
}