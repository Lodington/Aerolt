using Aerolt_External.Messages;
using RoR2;
using WebSocketSharp.Net.WebSockets;

namespace Aerolt_External.Commands;

public class SetSceneCommand : IWebsocketCommand
{
    public string CommandName => "setScene";
    public void Execute(string payload, WebSocketContext context)
    {
        new SceneChangeMessage(SceneCatalog.GetSceneDefFromSceneName(payload)!.sceneDefIndex).SendToServer();
    }
}