using RoR2;

namespace Aerolt_External.Messages;

public class TeleporterChargeMessage : AeroltMessageBase
{
    public override void Handle()
    {
        base.Handle();
        if (TeleporterInteraction.instance && TeleporterInteraction.instance.holdoutZoneController)
            TeleporterInteraction.instance.holdoutZoneController.charge = 1f;
    }
}