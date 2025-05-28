using WebSocketSharp.Net.WebSockets;

namespace Aerolt_External.Commands;

public class EspToggle : IWebsocketCommand
{
    
    public void Execute(WebSocketContext context)
    {
        Esp.ToggleEspOptions(this);
    }
}