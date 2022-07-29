using System;
using System.Linq;
using Aerolt.Buttons;
using Aerolt.Helpers;
using Aerolt.Messages;
using Rewired.Config;
using RoR2;
using UnityEngine;
using UnityEngine.UI;


namespace Aerolt.Managers
{
    public class TeleporterManager : MonoBehaviour
    {
        public GameObject buttonPrefab;
        public GameObject buttonParent;
        
        public void Start()
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
            new SceneChangeMessage(scene.sceneDefIndex).SendToServer();
        }

        public void InstaTeleporter()
        {
            new TeleporterChargeMessage().SendToServer();
            Chat.AddMessage("<color=yellow>Charged teleporter</color>");
        }
        public void SkipStage()
        {
            new SceneChangeMessage().SendToServer();
            Chat.AddMessage("<color=yellow>Skipping Stage</color>");
        }
        public void AddMountain()
        {
            TeleporterInteraction.instance.AddShrineStack();
            Chat.AddMessage("<color=yellow>Added Mountain stack</color>");
        }
        public void SpawnPortals(string portal)
        {
            new PortalSpawnMessage(portal).SendToServer();
        }
    }
}