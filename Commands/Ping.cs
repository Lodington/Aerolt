using Newtonsoft.Json;
using WebSocketSharp.Net.WebSockets;

namespace Aerolt_External.Commands;

public class Ping : IWebsocketCommand
{
    public void Execute(WebSocketContext context)
    {
        var response = new { type = "Pong" };
        var json = JsonConvert.SerializeObject(response);
        context.WebSocket.Send(json);
    }
}