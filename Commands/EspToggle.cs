using WebSocketSharp.Net.WebSockets;

namespace Aerolt_External.Commands;

public class EspToggle : IWebsocketCommand
{
    public bool showNewtAlterToggle;
    public bool showAdvancedToggle;
    public bool showTeleporterToggle;
    public bool showChestToggle;
    public bool showMultiShopToggle;
    public bool showBarrelToggle;
    public bool showScrapperToggle;
    public bool ShowSecretToggle;
    public bool showDuplicatorToggle;
    public bool showDroneToggle;
    public bool showShrineToggle;

    public void Execute(WebSocketContext context)
    {
        Esp.ToggleEspOptions(this);
    }
}