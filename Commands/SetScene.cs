using Aerolt_External.Messages;
using RoR2;
using WebSocketSharp.Net.WebSockets;

namespace Aerolt_External.Commands;

public class SetScene : IWebsocketCommand
{
    public string targetScene;
    public void Execute(WebSocketContext context)
    {
        new SceneChangeMessage(SceneCatalog.GetSceneDefFromSceneName(targetScene)!.sceneDefIndex).SendToServer();
    }
}