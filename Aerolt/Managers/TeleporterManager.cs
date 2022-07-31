using System.Collections.Generic;
using System.Linq;
using Aerolt.Buttons;
using Aerolt.Helpers;
using Aerolt.Messages;
using BepInEx;
using RoR2;
using TMPro;
using UnityEngine;


namespace Aerolt.Managers
{
    public class TeleporterManager : MonoBehaviour
    {
        public GameObject buttonPrefab;
        public GameObject buttonParent;
        public TMP_InputField searchFilter;
        private Dictionary<SceneDef, CustomButton> sceneDefRef = new();

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
                sceneDefRef[scene] = buttonComponet;
            }
            if(searchFilter)
                searchFilter.onValueChanged.AddListener(FilterUpdated);
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
        
        private void FilterUpdated(string text)
        {
            if (text.IsNullOrWhiteSpace())
            {
                foreach (var buttonGen in sceneDefRef)
                {
                    buttonGen.Value.gameObject.SetActive(true);
                }
                return;
            }
            
            var arr = sceneDefRef.Values.ToArray();
            var matches = Tools.FindMatches(arr, x => x.buttonText.text, text);
            foreach (var buttonGen in arr)
            {
                buttonGen.gameObject.SetActive(matches.Contains(buttonGen));
            }
        }
    }
}