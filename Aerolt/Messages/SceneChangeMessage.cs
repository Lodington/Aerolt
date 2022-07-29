using Aerolt.Managers;
using RoR2;
using UnityEngine.Networking;

namespace Aerolt.Messages
{
	public class SceneChangeMessage : AeroltMessageBase
	{
		private SceneIndex target;
		private bool isSet;
		public SceneChangeMessage(){}
		public SceneChangeMessage(SceneIndex sceneIndex)
		{
			isSet = true;
			target = sceneIndex;
		}

		public override void Handle()
		{
			base.Handle();
			var def = SceneCatalog.GetSceneDef(target);
			if (isSet && def)
			{
				Run.instance.AdvanceStage(def);
				Run.instance.stageClearCount--;
			}
			else
			{
				Run.instance.AdvanceStage(Run.instance.nextStageScene);
			}
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			isSet = reader.ReadBoolean();
			target = (SceneIndex) reader.ReadInt32();
		}

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(isSet);
			writer.Write((int) target);
		}
	}
}