using Aerolt_External.Messages;
using WebSocketSharp.Net.WebSockets;

namespace Aerolt_External.Commands;

public class SpawnPortal : IWebsocketCommand
{
    public string portalType;
    public void Execute(WebSocketContext context)
    {
        new PortalSpawnMessage(portalType).SendToServer();
    }
}