using System.Collections.Generic;
using Aerolt.Helpers;
using Aerolt.Managers;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace Aerolt.Messages
{
	public class MonsterSpawnMessage : AeroltMessageBase
	{
		private string bodyName;
		private Vector3 location;
		private TeamIndex teamIndex;
		private EquipmentIndex eliteIndex;
		private bool brainDead;
		private Dictionary<ItemDef, uint> itemCounts;

		public MonsterSpawnMessage(){}
		public MonsterSpawnMessage(string bodyName, Vector3 vector3, TeamIndex teamIndex1, EquipmentIndex equipmentIndex, bool brainDead, Dictionary<ItemDef,uint> toDictionary)
		{
			this.bodyName = bodyName;
			location = vector3;
			teamIndex = teamIndex1;
			eliteIndex = equipmentIndex;
			this.brainDead = brainDead;
			itemCounts = toDictionary;
		}

		public override void Handle()
		{
			base.Handle();
			var monsterMaster = MasterCatalog.FindMasterPrefab(bodyName);
			var bodyGameObject = Object.Instantiate(monsterMaster.gameObject, location, Quaternion.identity);
			var master = bodyGameObject.GetComponent<CharacterMaster>();

			master.teamIndex = teamIndex;
			
			foreach (var (key, value) in itemCounts)
				master.inventory.GiveItem(key, (int) value);
			if (eliteIndex != EquipmentIndex.None)
				master.inventory.SetEquipmentIndex(eliteIndex);
			if (brainDead) foreach (var masterAIComponent in master.aiComponents) Object.Destroy(masterAIComponent);


			NetworkServer.Spawn(bodyGameObject);
			master.bodyPrefab = BodyCatalog.FindBodyPrefab(bodyName);
			master.SpawnBody(location, Quaternion.identity);
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			bodyName = reader.ReadString();
			location = reader.ReadVector3();
			teamIndex = reader.ReadTeamIndex();
			eliteIndex = reader.ReadEquipmentIndex();
			brainDead = reader.ReadBoolean();
			itemCounts = reader.ReadItemAmounts();
		}

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(bodyName);
			writer.Write(location);
			writer.Write(teamIndex);
			writer.Write(eliteIndex);
			writer.Write(brainDead);
			writer.Write(itemCounts);
		}
	}
}