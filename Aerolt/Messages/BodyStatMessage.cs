using System.Reflection;
using Aerolt.Managers;
using RoR2;
using UnityEngine.Networking;

namespace Aerolt.Buttons
{
	public class BodyStatMessage : AeroltMessageBase
	{
		private CharacterBody TargetBody;
		private float value;
		private string fieldName;

		public BodyStatMessage(CharacterBody targetBody, string fieldName, float result)
		{
			this.fieldName = fieldName;
			TargetBody = targetBody;
			value = result;
		}

		public override void Handle()
		{
			base.Handle();
			var field = typeof(CharacterBody).GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
			field.SetValue(TargetBody, value);
			TargetBody.statsDirty = true;
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			var obj = Util.FindNetworkObject(reader.ReadNetworkId());
			if (obj) TargetBody = obj.GetComponent<CharacterBody>();
			fieldName = reader.ReadString();
			value = reader.ReadSingle();
		}

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(TargetBody.netId);
			writer.Write(fieldName);
			writer.Write(value);
		}
	}
}