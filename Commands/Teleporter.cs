using Aerolt_External.Messages;
using RoR2;
using WebSocketSharp.Net.WebSockets;

namespace Aerolt_External.Commands;

public class Teleporter : IWebsocketCommand
{
    public string option;
    public void Execute(WebSocketContext context)
    {
        switch (option)
        {
            case "spawn_newt_portal":
            case "spawn_gold_portal":
            case "spawn_void_portal":
            case "spawn_celestial_portal":
            case "spawn_all_portals":
                new PortalSpawnMessage(option.Split("_")[1]).SendToServer();
                break;
            case "skip_stage":
                new SceneChangeMessage().SendToServer();
                break;
            case "instant_charge_teleporter":
                new TeleporterChargeMessage().SendToServer();
                break;
            case "add_mountain_shrine":
                TeleporterInteraction.instance.AddShrineStack();
                break;
        }
    }
}