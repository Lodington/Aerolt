using Newtonsoft.Json;
using RoR2;
using WebSocketSharp.Server;

namespace Aerolt_External
{
    public class WebSocketBehaviour : WebSocketBehavior
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