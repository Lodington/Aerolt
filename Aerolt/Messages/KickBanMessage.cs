using System.Collections.Generic;
using System.Linq;
using RoR2;
using UnityEngine.Networking;

namespace Aerolt.Managers
{
	public class KickBanMessage : AeroltMessageBase
	{
		private bool kick;
		private bool ban;
		private NetworkUser who;
		public KickBanMessage(){}
		public KickBanMessage(NetworkUser who, bool kick = false, bool ban = false)
		{
			this.who = who;
			this.kick = kick;
			this.ban = ban;
		}

		public override void Handle()
		{
			base.Handle();
			var host= NetworkUser.localPlayers.First();
			if (kick)
			{
				Console.instance.RunCmd(host, "kick_steam",new List<string> {who.Network_id.steamId.ToString()});
			}
			if (ban)
			{
				Console.instance.RunCmd(host, "ban_steam",new List<string> {who.Network_id.steamId.ToString()});
			}
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			who = NetworkUser.instancesList.First(x => x.netId == reader.ReadNetworkId());
			kick = reader.ReadBoolean();
			ban = reader.ReadBoolean();
		}

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(who.netId);
			writer.Write(kick);
			writer.Write(ban);
		}
	}
}