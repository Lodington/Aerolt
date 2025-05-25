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
using WebSocketSharp;

namespace Aerolt_External;

[BepInPlugin("com.Lodington.Aerolt", "Aerolt", "5.0.0")]
public class AeroltPlugin : BaseUnityPlugin
{
    public static ManualLogSource Log;
    public static AeroltPlugin Instance;
    private WebSocketServer _server;
    private static readonly System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();


    [System.Serializable]
    public class ImageHeader
    {
        public string type => "ImageHeader";
        public string fileName; // e.g. "sword.png"
        public int length; // byte length of the following frame
   }

    private void Start()
    {
        Log = Logger;

        Instance = this;

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
        private static Dictionary<ItemIndex, byte[]> itemIcons = new Dictionary<ItemIndex, byte[]>();

        protected override void OnOpen()
        {
            Instance.StartCoroutine(SenditemCatalogWhenReady());
            Debug.Log("ItemCatalog Ready");
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