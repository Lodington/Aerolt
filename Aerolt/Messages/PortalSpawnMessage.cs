using System.Linq;
using Aerolt.Managers;
using RoR2;
using UnityEngine.Networking;

namespace Aerolt.Messages
{
    public class PortalSpawnMessage : AeroltMessageBase
    {
        private string portal;

        public PortalSpawnMessage(string portal)
        {
            this.portal = portal;
        }

        public override void Handle()
        {
            base.Handle();
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
                case "void":
                    var portals =
                        TeleporterInteraction.instance.portalSpawners.FirstOrDefault(x =>
                            x.spawnMessageToken == "PORTAL_VOID_OPEN");
                    if (portals != default)
                    {
                        if (!Run.instance.IsExpansionEnabled(portals.requiredExpansion)) return;
                        portals.NetworkwillSpawn = true;
                        if (!string.IsNullOrEmpty(portals.spawnPreviewMessageToken))
                            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                            {
                                baseToken = portals.spawnPreviewMessageToken
                            });
                        if (portals.previewChild) portals.previewChild.SetActive(true);
                    }

                    break;
                case "all":
                    Chat.AddMessage("<color=red>Spawned All Portal</color>");
                    TeleporterInteraction.instance.shouldAttemptToSpawnGoldshoresPortal = true;
                    TeleporterInteraction.instance.shouldAttemptToSpawnShopPortal = true;
                    TeleporterInteraction.instance.shouldAttemptToSpawnMSPortal = true;
                    foreach (var spawner in TeleporterInteraction.instance.portalSpawners)
                    {
                        if (!Run.instance.IsExpansionEnabled(spawner.requiredExpansion)) continue;
                        spawner.NetworkwillSpawn = true;
                        if (!string.IsNullOrEmpty(spawner.spawnPreviewMessageToken))
                            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                            {
                                baseToken = spawner.spawnPreviewMessageToken
                            });
                        if (spawner.previewChild) spawner.previewChild.SetActive(true);
                    }

                    break;
            }
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            portal = reader.ReadString();
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(portal);
        }
    }
}