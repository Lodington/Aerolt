using Aerolt_External.Messages;
using WebSocketSharp.Net.WebSockets;

namespace Aerolt_External.Commands;

public class SpawnPortalCommand : IWebsocketCommand
{
    public string CommandName => "spawnPortal";
    public void Execute(string payload, WebSocketContext context)
    {
        new PortalSpawnMessage(payload).SendToServer();
    }
}