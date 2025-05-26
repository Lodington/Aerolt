using Newtonsoft.Json;
using WebSocketSharp.Net.WebSockets;

namespace Aerolt_External.Commands;

public class PingCommand : IWebsocketCommand
{
    public string CommandName => "ping";
    public void Execute(string payload, WebSocketContext context)
    {
        var response = new { type = "Pong" };
        var json = JsonConvert.SerializeObject(response);
        context.WebSocket.Send(json);
    }
}