using System;
using System.Collections.Generic;
using Aerolt.Managers;
using BepInEx.Configuration;
using RoR2;
using UnityEngine.Networking;
using ZioConfigFile;

namespace Aerolt.Classes
{
	public abstract class ValueWrapper
	{
		protected static readonly Dictionary<string, ValueWrapper> Instances = new();
		protected abstract void SetValue(object value);
		public static void HandleSync(string key, object value)
		{
			if (Instances.TryGetValue(key, out var wrapper)) wrapper.SetValue(value);
		}

		public static ValueWrapper<T> Get<T>(string category, string name, T defaultValue, string description, NetworkUser who = null)
		{
			if (Instances.TryGetValue(GetId(who) + category + name, out var entry))
			{
				var centry = (ValueWrapper<T>) entry;
				return centry;
			}
			var val = new ValueWrapper<T>(category, name, defaultValue, description, who);
			return val;
		}

		public static string GetId(NetworkUser user)
		{
			if (!user) return string.Empty; 
			var id = user.id;
			return (id.value != 0UL ? id.value.ToString() : id.strValue) + id.subId;
		}
	}
	public class ValueWrapper<T> : ValueWrapper
	{
		private ZioConfigEntry<T> configEntry;
		private T fallbackValue;
		private bool isLocalBinding;
		public event Action settingChanged;
		private bool duckChange;
		protected string identifier;
		public NetworkUser user;

		public override string ToString()
		{
			return $"{nameof(ValueWrapper)}({identifier}: {Value})";
		}
		
		public ValueWrapper(string category, string name, T defaultValue, string description, NetworkUser who = null)
		{
			user = who;
			if (!who || who.localUser != null)
			{
				isLocalBinding = true;
				var file = who ? MenuInfo.Files[who.localUser!] : Load.configFile;
				configEntry = file.Bind(category, name, defaultValue, description);
			}
			else
			{
				fallbackValue = defaultValue;
			}

			identifier = GetId(who) + category + name;
			Instances.Add(identifier, this);
		}

		public T Value
		{
			get => isLocalBinding ? configEntry.Value : fallbackValue;
			set
			{
				//if (value.Equals(Value)) return;
				if (isLocalBinding)
					configEntry.Value = value;
				else
					fallbackValue = value;
				if (!duckChange)
				{
					Sync();
				}
				settingChanged?.Invoke();
			}
		}

		public void Sync()
		{
			new ValueWrapperSyncMessage(identifier, Value).SendToEveryone();
		}

		protected override void SetValue(object value)
		{
			duckChange = true;
			Value = (T) value;
			duckChange = false;
		}
	}

	public class ValueWrapperSyncMessage : AeroltMessageBase
	{
		private string key;
		private int type;
		private string value;

		public ValueWrapperSyncMessage(){}
		public ValueWrapperSyncMessage(string valueWrapperKey, object value)
		{
			key = valueWrapperKey;
			var typ = value.GetType();
			type = GetTypeFromValue(typ);
			this.value = TomlTypeConverter.ConvertToString(value, typ);
		}

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(key);
			writer.Write(type);
			writer.Write(value);
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			key = reader.ReadString();
			type = reader.ReadInt32();
			value = reader.ReadString();
		}

		public override void Handle()
		{
			base.Handle();
			ValueWrapper.HandleSync(key, TomlTypeConverter.ConvertToValue(value, GetValueFromType(type)));
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