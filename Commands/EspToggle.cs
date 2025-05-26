using WebSocketSharp.Net.WebSockets;

namespace Aerolt_External.Commands;

public class EspToggle : IWebsocketCommand
{
    public string CommandName => "toggleESP";
    public void Execute(string payload, WebSocketContext context)
    {
        Esp.ToggleEspOptions(payload);
    }
}