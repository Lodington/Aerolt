using Aerolt.Managers;
using RoR2;
using UnityEngine.Networking;

namespace Aerolt.Messages
{
	public class SceneChangeMessage : AeroltMessageBase
	{
		private SceneIndex target;
		public SceneChangeMessage(){}
		public SceneChangeMessage(SceneIndex sceneIndex) => target = sceneIndex;
		public override void Handle()
		{
			base.Handle();
			var def = SceneCatalog.GetSceneDef(target);
			if (def)
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
			target = (SceneIndex) reader.ReadInt32();
		}

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int) target);
		}
	}
}