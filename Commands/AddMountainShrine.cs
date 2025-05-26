using RoR2;
using WebSocketSharp.Net.WebSockets;

namespace Aerolt_External.Commands;

public class AddMountainShrine : IWebsocketCommand
{

    public void Execute(WebSocketContext context)
    {
        TeleporterInteraction.instance.AddShrineStack();
        //todo chat added shrine
    }
}