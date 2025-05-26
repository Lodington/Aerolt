using RoR2;
using WebSocketSharp.Net.WebSockets;

namespace Aerolt_External.Commands;

public class AddMountainShrineCommand : IWebsocketCommand
{
    public string CommandName => "addMountainShrine";
    public void Execute(string payload, WebSocketContext context)
    {
        TeleporterInteraction.instance.AddShrineStack();
        //todo chat added shrine
    }
}