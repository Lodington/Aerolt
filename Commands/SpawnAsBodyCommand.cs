using Aerolt_External.Messages;
using Newtonsoft.Json;
using RoR2;
using WebSocketSharp.Net.WebSockets;

namespace Aerolt_External.Commands;

public class SpawnAsBodyCommand : IWebsocketCommand
{
    public string CommandName => "spawnAsBody";
    public void Execute(string payload, WebSocketContext context)
    {
        var data = JsonConvert.DeserializeObject<BodyPayload>(payload);
        
        var user = NetworkUser.instancesList.FirstOrDefault(user =>
            user.userName != null &&
            user.userName.Equals(data.playerID, StringComparison.OrdinalIgnoreCase));

        var newbody = BodyCatalog.FindBodyPrefab(data.newbody);
        
        new SetBodyMessage(user, newbody.GetComponent<CharacterBody>()).SendToServer();
    }

    public class BodyPayload
    {
        public string playerID { get; set; }
        public string newbody { get; set; }
    }
    
}