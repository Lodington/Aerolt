using Aerolt.Managers;
using RoR2;
using UnityEngine.Networking;

namespace Aerolt.Messages
{
	public class ToggleMobSpawnsMessage : AeroltMessageBase
	{
		public bool isOn;

		public ToggleMobSpawnsMessage() {}
		public ToggleMobSpawnsMessage(bool isOn)
		{
			this.isOn = isOn;
		}
		

		public override void Handle()
		{
			base.Handle();
			
			foreach (var (_, ui) in Load.aeroltUIs)
				ui.GetComponentInChildren<LobbyPlayerPageManager>().disableMobSpawnToggle.SetIsOnWithoutNotify(isOn);
			
			if (!NetworkServer.active) return;
			foreach (var director in CombatDirector.instancesList)
				director.monsterSpawnTimer = isOn ? float.PositiveInfinity : 0f;
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			isOn = reader.ReadBoolean();
		}

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(isOn);
		}
	}
}