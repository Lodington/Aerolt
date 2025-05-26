using Aerolt_External.Messages;
using WebSocketSharp.Net.WebSockets;

namespace Aerolt_External.Commands;

public class InstaTeleporter : IWebsocketCommand
{
    public void Execute(WebSocketContext context)
    {
       new TeleporterChargeMessage().SendToServer();
       //TODO send message to chat?
    }
}