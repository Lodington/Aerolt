using System;
using System.Linq;
using Aerolt.Buttons;
using Aerolt.Classes;
using Aerolt.Helpers;
using Rewired.Config;
using RoR2;
using UnityEngine;
using UnityEngine.UI;


namespace Aerolt.Managers
{

    
    public class TeleporterManager : MonoBehaviour, IModuleStartup
    {
        public GameObject buttonPrefab;
        public GameObject buttonParent;
        
        public void ModuleStart()
        {
            foreach (var scene in SceneCatalog.allSceneDefs.OrderByDescending(x => x.sceneType))
            {
                if(!scene)
                    continue;
                
                GameObject newButton = Instantiate(buttonPrefab, buttonParent.transform);

                var buttonComponet = newButton.GetComponent<CustomButton>();
                buttonComponet.buttonText.text = !string.IsNullOrEmpty(scene.nameToken) ? Language.GetString(scene.nameToken) : scene.cachedName;
                if(scene.previewTexture)
                    buttonComponet.image.sprite = Sprite.Create((Texture2D)scene.previewTexture, new Rect(0, 0,scene.previewTexture.width, scene.previewTexture.height), new Vector2(0.5f, 0.5f));
                buttonComponet.button.onClick.AddListener(() => SetScene(scene));
            }
        }

        public void SetScene(SceneDef scene)
        {
            Run.instance.AdvanceStage(scene);
            Run.instance.stageClearCount--;
        }
        
        
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
                case "void":
                    var portals = TeleporterInteraction.instance.portalSpawners.FirstOrDefault(x => x.spawnMessageToken == "PORTAL_VOID_OPEN");
                    if (portals != default)
                    {
                        if (!Run.instance.IsExpansionEnabled(portals.requiredExpansion)) return;
                        portals.NetworkwillSpawn = true;
                        if (!string.IsNullOrEmpty(portals.spawnPreviewMessageToken))
                        {
                            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                            {
                                baseToken = portals.spawnPreviewMessageToken
                            });
                        }
                        if (portals.previewChild)
                        {
                            portals.previewChild.SetActive(true);
                        }
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
                        {
                            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                            {
                                baseToken = spawner.spawnPreviewMessageToken
                            });
                        }
                        if (spawner.previewChild)
                        {
                            spawner.previewChild.SetActive(true);
                        }
                    }
                    break;
            }
        }
    }
}