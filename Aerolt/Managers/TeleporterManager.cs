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
            Tools.Log(Aerolt.Enums.LogLevel.Information, "Charged Teleporter");
        }
        public void SkipStage()
        {
            Run.instance.AdvanceStage(Run.instance.nextStageScene);
            Tools.Log(Enums.LogLevel.Information, "Skipped Stage");
        }
        public void AddMountain()
        {
            TeleporterInteraction.instance.AddShrineStack();
        }
        public void SpawnPortals(string portal)
        {
            switch (portal)
            {
                case "gold":
                    Chat.AddMessage("<color=yellow>Spawned Gold Portal</color>");
                    TeleporterInteraction.instance.shouldAttemptToSpawnGoldshoresPortal = true;
                    break;
                case "newt":
                    Chat.AddMessage("<color=blue>Spawned Newt Portal</color>");
                    TeleporterInteraction.instance.shouldAttemptToSpawnShopPortal = true;
                    break;
                case "blue":
                    Chat.AddMessage("<color=cyan>Spawned Celestial Portal</color>");
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