using System.Collections;
using System.Diagnostics;
using BepInEx;
using BepInEx.Logging;
using Newtonsoft.Json;
using RoR2;
using UnityEngine;
using WebSocketSharp.Server;
using Debug = UnityEngine.Debug;
using LogLevel = WebSocketSharp.LogLevel;
using System.Security.Cryptography;
using Aerolt_External.Commands;
using RoR2.UI;
using WebSocketSharp;

namespace Aerolt_External;

[BepInPlugin("com.Lodington.Aerolt", "Aerolt", "5.0.0")]
public class AeroltPlugin : BaseUnityPlugin
{
    public static ManualLogSource Log;
    public static AeroltPlugin Instance;
    private WebSocketServer _server;
    private static readonly MD5 md5 = MD5.Create();

    [System.Serializable]
    public class ImageHeader
    {
        public string type => "ImageHeader";
        public string fileName; // e.g. "sword.png"
        public int length; // byte length of the following frame
   }

    private void Awake()
    {
        var espObject = new GameObject("Esp");
        espObject.AddComponent<Esp>();
        DontDestroyOnLoad(espObject);
    }

    private void Start()
    {
        Log = Logger;

        Instance = this;
        CommandManager.RegisterAllCommands();
        
        _server = new WebSocketServer("ws://127.0.0.1:8180");
        _server.Log.Level = LogLevel.Info;
        _server.AddWebSocketService<CatalogService>("/ws");
        _server.Start();
        Debug.Log("Started Websocket Server");

        var startInfo =
            new ProcessStartInfo(System.IO.Path.Join(System.IO.Path.GetDirectoryName(Info.Location), "aerolt.exe"));

        startInfo.UseShellExecute = true;
        //Process.Start(startInfo);
        Debug.Log("Started Client");
    }

    void OnDestroy()
    {
        _server.Stop();
    }

    public class CatalogService : WebSocketBehavior
    {
        private static Dictionary<ItemIndex, byte[]> itemIcons = new();
        private static Dictionary<BodyIndex, byte[]> bodyIcons = new();

        protected override void OnOpen()
        {
            Instance.StartCoroutine(SenditemCatalogWhenReady());
            AeroltPlugin.Log.LogInfo("Item Catalog Ready"); 
            Instance.StartCoroutine(SendMonsterCatalogWhenReady());
            AeroltPlugin.Log.LogInfo("Body Catalog Ready");
            
        }

        IEnumerator SendMonsterCatalogWhenReady()
        {
            yield return new WaitUntil(() => BodyCatalog.availability.available);
            
            var simple = BodyCatalog.allBodyPrefabs
                .Select(d =>
                {
                    var characterBody = d.GetComponent<CharacterBody>();
                    if (characterBody.portraitIcon) 
                        bodyIcons[characterBody.bodyIndex] = characterBody.portraitIcon.ToReadable().EncodeToPNG();
                    return new
                    {
                        bodyId = (int)characterBody.bodyIndex, 
                        fileName = d.name + "_" + characterBody.baseNameToken,
                        bodyName = Language.GetString(characterBody.baseNameToken),
                        pickupModel = characterBody.portraitIcon?.name,
                        iconHash = characterBody.portraitIcon
                            ? md5.ComputeHash(bodyIcons[characterBody.bodyIndex])
                            : null
                    };
                })
                .ToList();
            var envelope = new
            {
                type = "Catalog",
                count = simple.Count,
                body = simple,
            };
            string json = JsonConvert.SerializeObject(envelope);
            Send(json);
        }

        IEnumerator SenditemCatalogWhenReady()
        {
            yield return new WaitUntil(() => ItemCatalog.availability.available);

            var simple = ItemCatalog.allItemDefs
                .Select(d =>
                {
                    if (d.pickupIconSprite)
                        itemIcons[d.itemIndex] = d.pickupIconSprite.texture.ToReadable().EncodeToPNG();
                    return new
                    {
                        itemId = (int)d.itemIndex,
                        fileName = d.name + "_" + d.nameToken,
                        itemName = Language.GetString(d.nameToken),
                        tier = d.tier.ToString(),
                        description = Language.GetString(d.descriptionToken),
                        pickupModel = d.pickupModelPrefab?.name,
                        iconHash = d.pickupIconSprite
                            ? md5.ComputeHash(itemIcons[d.itemIndex])
                            : null
                    };
                })
                .ToList();

            var envelope = new
            {
                type = "Catalog",
                count = simple.Count,
                items = simple,
                equipments = Array.Empty<object>()
            };
            string json = JsonConvert.SerializeObject(envelope);
            Send(json);
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            var rawPayload = new WebsocketMessage(e.Data);
            
            CommandManager.Execute(rawPayload.type, e.Data, Context);
            
            switch (rawPayload.type)
            {
                case nameof(IconRequest):
                    var message = new IconRequest(e.Data);
                    switch (message.iconType)
                    {
                        case IconRequest.IconType.Item:
                            foreach (var index in message.icons)
                            {
                                var itemIndex = (ItemIndex)index;
                                var item = ItemCatalog.GetItemDef(itemIndex);
                                var png = itemIcons[itemIndex];
                                var header = new ImageHeader()
                                {
                                    fileName = item.name + "_" + item.nameToken,
                                    length = png.Length
                                };
                                Context.WebSocket.Send(JsonConvert.SerializeObject(header));
                                Context.WebSocket.Send(png);
                            }
                            break;
                        case IconRequest.IconType.Equipment:
                            break;
                        case IconRequest.IconType.Buff:
                            break;
                        case IconRequest.IconType.Monster:
                            break;
                        case IconRequest.IconType.Map:
                            break;
                        case IconRequest.IconType.Interactable:
                            break;
                        case IconRequest.IconType.Skill:
                            break;
                        case IconRequest.IconType.Misc:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
            }

            base.OnMessage(e);
        }
    }
    
    class IconRequest
    {
        [Serializable]
        public enum IconType
        {
            Item,
            Equipment,
            Buff,
            Monster,
            Map,
            Interactable,
            Skill,
            Misc
        }
        public IconType iconType;
        public int[] icons;

        public IconRequest(string serializedData)
        {
            JsonConvert.PopulateObject(serializedData, this);
        }

        public IconRequest()
        {
        }
    }
}