using BepInEx;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Aerolt_External;

[BepInPlugin("com.Lodington.Aerolt","Aerolt","5.0.0")]
public class AeroltPlugin : BaseUnityPlugin
{
    private void Awake()
    {
        var socket = new WebSocketServer("ws://127.0.0.1:8181");
        socket.Log.Level = LogLevel.Info;
        socket.AddWebSocketService<WebSocketBehaviour>("/ws");
        socket.Start();
    }
}