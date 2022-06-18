using Aerolt.Helpers;
using Rewired.Config;
using RoR2;
using UnityEngine;


namespace Aerolt.Managers
{
    public class TeleporterManager : MonoBehaviour
    {
        public void InstaTeleporter()
        {
            if (!TeleporterInteraction.instance) return;
            typeof(HoldoutZoneController).GetProperty("charge")?.SetValue(TeleporterInteraction.instance.holdoutZoneController, 1f);
            Chat.AddMessage("<color=yellow>Charged teleporter</color>");
        }
        public void SkipStage()
        {
            Run.instance.AdvanceStage(Run.instance.nextStageScene);
            Chat.AddMessage("<color=yellow>Skipping Stage</color>");
        }
        public void AddMountain()
        {
            TeleporterInteraction.instance.AddShrineStack();
            Chat.AddMessage("<color=yellow>Added Mountain stack</color>");
        }
        public void SpawnPortals(string portal)
        {
            switch (portal)
            {
                case "gold":
                    TeleporterInteraction.instance.shouldAttemptToSpawnGoldshoresPortal = true;
                    break;
                case "newt":
                    TeleporterInteraction.instance.shouldAttemptToSpawnShopPortal = true;
                    break;
                case "blue":
                    TeleporterInteraction.instance.shouldAttemptToSpawnMSPortal = true;
                    break;
                case "all":
                    Chat.AddMessage("<color=red>Spawned All Portal</color>");
                    TeleporterInteraction.instance.shouldAttemptToSpawnGoldshoresPortal = true;
                    TeleporterInteraction.instance.shouldAttemptToSpawnShopPortal = true;
                    TeleporterInteraction.instance.shouldAttemptToSpawnMSPortal = true;
                    break;
            }
        }
    }
}