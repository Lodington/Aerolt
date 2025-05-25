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

namespace Aerolt_External;

[BepInPlugin("com.Lodington.Aerolt", "Aerolt", "5.0.0")]
public class AeroltPlugin : BaseUnityPlugin
{
    public static ManualLogSource Log;
    public static AeroltPlugin Instance;
    private WebSocketServer _server;
    
    [System.Serializable]
    public struct ImageEntry
    {
        public string relativePath; // under StreamingAssets
        public string itemName;
        public int itemId;
    }

    [System.Serializable]
    public class ImageHeader
    {
        public string type; // "image"
        public string filename; // e.g. "sword.png"
        public int length; // byte length of the following frame
        public string itemName; // extra game data
        public int itemId; // extra game data
    }

    private void Start()
    {
        Log = Logger;

        Instance = this;
        
        _server = new WebSocketServer("ws://127.0.0.1:8181");
        _server.Log.Level = LogLevel.Info;
        _server.AddWebSocketService<CatalogService>("/ws");
        _server.Start();
        
        
        Debug.Log("Started Websocket Server");
        
        var startInfo = new ProcessStartInfo("\"C:\\Users\\Lodington\\AppData\\Roaming\\com.kesomannen.gale\\riskofrain2\\profiles\\Default\\BepInEx\\plugins\\Lodington-Aerolt\\aerolt.exe\"");
        
        startInfo.UseShellExecute = true;
        Process.Start(startInfo);
        Debug.Log("Started Client");
    }

    void OnDestroy()
    {
        _server.Stop();
    }

    public class CatalogService : WebSocketBehavior
    {
        protected override void OnOpen()
        {
            Instance.StartCoroutine(SenditemCatalogWhenReady());
            Debug.Log("ItemCatalog Ready");
        }
        
        IEnumerator SenditemCatalogWhenReady()
        {
            yield return new WaitUntil(() => Equals(ItemCatalog.availability, ItemCatalog.availability.available));
            
            var simple = ItemCatalog.allItemDefs
                .Select(d => new
                {
                    itemId = (int)d.itemIndex,
                    itemName = Language.GetString(d.nameToken),
                    tier = d.tier.ToString(),
                    description = Language.GetString(d.descriptionToken),
                    pickupModel = d.pickupModelPrefab?.name
                })
                .ToList();

            var envelope = new
            {
                type = "itemCatalog",
                count = simple.Count,
                items = simple
            };
            string json = JsonConvert.SerializeObject(envelope);
            Send(json);
            
        }
    }
}
