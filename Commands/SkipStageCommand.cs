using Aerolt_External.Messages;
using WebSocketSharp.Net.WebSockets;

namespace Aerolt_External.Commands;

public class SkipStageCommand : IWebsocketCommand
{
    public string CommandName => "skipStage";
    public void Execute(string payload, WebSocketContext context)
    {
        new SceneChangeMessage().SendToServer();
        //todo send message to chat?
    }
}