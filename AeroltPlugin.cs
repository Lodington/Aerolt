using System.Collections;
using System.Diagnostics;
using BepInEx;
using Newtonsoft.Json;
using RoR2;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;
using Console = System.Console;
using Debug = System.Diagnostics.Debug;

namespace Aerolt_External;

[BepInPlugin("com.Lodington.Aerolt", "Aerolt", "5.0.0")]
public class AeroltPlugin : BaseUnityPlugin
{
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

    private void Awake()
    {
        var socket = new WebSocketServer("ws://127.0.0.1:8181");
        socket.Log.Level = LogLevel.Info;
        socket.AddWebSocketService<WebSocketBehaviour>("/ws");
        socket.Start();
        
        
        

        var startInfo = new ProcessStartInfo("\"C:\\Users\\Lodington\\AppData\\Roaming\\com.kesomannen.gale\\riskofrain2\\profiles\\Default\\BepInEx\\plugins\\Lodington-Aerolt\\aerolt.exe\"");
        
        startInfo.UseShellExecute = true;
        Process.Start(startInfo);
    }

    public class CatalogService : WebSocketBehavior
    {
        protected override void OnOpen()
        {
            new Thread(() =>
                {
                    while (!ItemCatalog.availability.available)
                    {
                        Thread.Sleep(100);
                        
                    }
                    
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
                })
                { IsBackground = true }.Start();
        }
    }
}
