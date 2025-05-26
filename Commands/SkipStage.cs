using Aerolt_External.Messages;
using WebSocketSharp.Net.WebSockets;

namespace Aerolt_External.Commands;

public class SkipStage : IWebsocketCommand
{
    public void Execute(WebSocketContext context)
    {
        new SceneChangeMessage().SendToServer();
        //todo send message to chat?
    }
}