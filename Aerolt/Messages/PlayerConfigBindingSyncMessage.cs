using System;
using System.Linq;
using Aerolt.Managers;
using BepInEx.Configuration;
using RoR2;
using UnityEngine.Networking;

namespace Aerolt.Messages
{
	public class PlayerConfigBindingSyncMessage : AeroltMessageBase
	{
		private string val;
		private string field;
		private int type;
		private NetworkUser user;
		public PlayerConfigBindingSyncMessage(){}
		public PlayerConfigBindingSyncMessage(NetworkUser user, string name, object value)
		{
			this.user = user;
			field = name;
			var typ = value.GetType();
			type = GetTypeFromValue(typ);
			val = TomlTypeConverter.ConvertToString(value, typ);
		}
		public override void Handle()
		{
			base.Handle();
			var newVal = TomlTypeConverter.ConvertToValue(val, GetValueFromType(type));
			foreach (var (_, ui) in Load.aeroltUIs)
			{
				var (_, value) = ui.GetComponentInChildren<LobbyPlayerManager>(true).users
					.FirstOrDefault(x => x.Key == user);
				switch (newVal)
				{
					case bool b:
						value.SetValue(field, b);
						break;
					case float f:
						value.SetValue(field, f);
						break;
				}
			}
		}
		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			var obj = Util.FindNetworkObject(reader.ReadNetworkId());
			if (obj) user = obj.GetComponent<NetworkUser>();
			field = reader.ReadString();
			type = reader.ReadInt32();
			val = reader.ReadString();
		}
		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(user.netId);
			writer.Write(field);
			writer.Write(type);
			writer.Write(val);
		}
		
		private static int GetTypeFromValue(Type configSettingType)
		{
			var i = 0;
			foreach (var type in TomlTypeConverter.GetSupportedTypes())
			{
				if (type == configSettingType) return i;
				i++;
			}
			return -1;
		}
		private static Type? GetValueFromType(int type)
		{
			var i = 0;
			foreach (var supportedType in TomlTypeConverter.GetSupportedTypes())
			{
				if (i == type) return supportedType;
				i++;
			}
			return null;
		}
	}
}