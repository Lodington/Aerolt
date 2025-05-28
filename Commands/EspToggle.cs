using WebSocketSharp.Net.WebSockets;

namespace Aerolt_External.Commands;

public class EspToggle : IWebsocketCommand
{
    public string id;
    public bool toggled;

    public void Execute(WebSocketContext context)
    {
        ToggleFromJsonKey(id, toggled);
    }

    public static void ToggleFromJsonKey(string key, bool value)
    {
        switch (key)
        {
            case "advanced_toggle":
                Esp.Instance.showAdvancedToggle = value;
                break;
            case "teleporter_toggle":
                Esp.Instance.showTeleporterToggle = value;
                break;
            case "chest_toggle":
                Esp.Instance.showChestToggle = value;
                break;
            case "multishop_toggle":
                Esp.Instance.showMultiShopToggle = value;
                break;
            case "barrel_toggle":
                Esp.Instance.showBarrelToggle = value;
                break;
            case "scrapper_toggle":
                Esp.Instance.showScrapperToggle = value;
                break;
            case "secret_toggle":
                Esp.Instance.ShowSecretToggle = value;
                break;
            case "duplicator_toggle":
                Esp.Instance.showDuplicatorToggle = value;
                break;
            case "drone_toggle":
                Esp.Instance.showDroneToggle = value;
                break;
            case "shrine_toggle":
                Esp.Instance.showShrineToggle = value;
                break;
        }
    }
}