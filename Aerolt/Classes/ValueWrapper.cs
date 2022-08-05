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

		/// <summary>
		/// Get or Create a ValueWrapper.
		/// </summary>
		/// <param name="category">Config Category</param>
		/// <param name="name">Config Name</param>
		/// <param name="defaultValue"></param>
		/// <param name="description">Config Description</param>
		/// <param name="who">The network user its paired to, can be null and will fallback on Load.configFile and be stored on all clients.</param>
		/// <param name="forceLocalOrRemote">
		/// Null is default behaviour, and doesnt override if its stored locally or remote.
		/// True forces it to be stored locally/on disk, either on the <paramref name="who"/> or fallback to Load.configFile.
		/// False forces it to not be stored on disk, and discards <paramref name="who"/>. <paramref name="who"/> will only be used for the identifier. 
		/// </param>
		/// <typeparam name="T">Any <see cref="ZioConfigEntry{T}"/> serializable type.</typeparam>
		/// <returns></returns>
		public static ValueWrapper<T> Get<T>(string category, string name, T defaultValue, string description, NetworkUser who = null, bool? forceLocalOrRemote = null)
		{
			if (Instances.TryGetValue(GetId(who) + category + name, out var entry))
			{
				var centry = (ValueWrapper<T>) entry;
				return centry;
			}
			var val = new ValueWrapper<T>(category, name, defaultValue, description, who, forceLocalOrRemote);
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
		
		public ValueWrapper(string category, string name, T defaultValue, string description, NetworkUser who = null, bool? forceLocalOrRemote = null) // force = true, it will be a local : force = false, it will be remote 
		{
			user = who;
			if (forceLocalOrRemote.HasValue && forceLocalOrRemote.Value || !who && !forceLocalOrRemote.HasValue || !forceLocalOrRemote.HasValue && who.localUser != null)
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