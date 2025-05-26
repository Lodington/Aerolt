using Aerolt_External.Messages;
using Newtonsoft.Json;
using RoR2;
using WebSocketSharp.Net.WebSockets;

namespace Aerolt_External.Commands;

public class SpawnAsBody : IWebsocketCommand
{
    public void Execute(WebSocketContext context)
    {
        var user = NetworkUser.instancesList.FirstOrDefault(user =>
            user.userName != null &&
            user.userName.Equals(playerID, StringComparison.OrdinalIgnoreCase));

        var newBody = BodyCatalog.FindBodyPrefab(newbody);

        new SetBodyMessage(user, newBody.GetComponent<CharacterBody>()).SendToServer();
    }

    public string playerID;
    public string newbody;
}