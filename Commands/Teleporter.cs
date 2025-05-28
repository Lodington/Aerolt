using Aerolt_External.Messages;
using RoR2;
using WebSocketSharp.Net.WebSockets;

namespace Aerolt_External.Commands;

public class Teleporter : IWebsocketCommand
{
    public string portalType;
    public void Execute(WebSocketContext context)
    {
        switch (portalType)
        {
            case "spawn_newt_portal":
            case "spawn_gold_portal":
            case "spawn_void_portal":
            case "spawn_celestial_portal":
            case "spawn_all_portals":
                new PortalSpawnMessage(portalType.Split("_")[1]).SendToServer();
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