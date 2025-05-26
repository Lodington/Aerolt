using Aerolt_External.Messages;
using WebSocketSharp.Net.WebSockets;

namespace Aerolt_External.Commands;

public class InstaTeleporterCommand : IWebsocketCommand
{
    public string CommandName => "instaTeleporter";
    public void Execute(string payload, WebSocketContext context)
    {
       new TeleporterChargeMessage().SendToServer();
       //TODO send message to chat?
    }
}