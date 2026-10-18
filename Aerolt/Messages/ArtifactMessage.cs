using Aerolt.Managers;
using RoR2;
using UnityEngine.Networking;

namespace Aerolt.Messages
{
    /// <summary>Server-authoritative toggle of an artifact's enabled state for the current run.</summary>
    public class ArtifactMessage : AeroltMessageBase
    {
        private int _artifactIndex;
        private bool _enabled;

        public ArtifactMessage()
        {
        }

        public ArtifactMessage(ArtifactIndex index, bool enabled)
        {
            _artifactIndex = (int)index;
            _enabled = enabled;
        }

        public override void Handle()
        {
            base.Handle();
            if (!NetworkServer.active || !RunArtifactManager.instance) return;
            var def = ArtifactCatalog.GetArtifactDef((ArtifactIndex)_artifactIndex);
            if (def) RunArtifactManager.instance.SetArtifactEnabledServer(def, _enabled);
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.WritePackedUInt32((uint)_artifactIndex);
            writer.Write(_enabled);
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            _artifactIndex = (int)reader.ReadPackedUInt32();
            _enabled = reader.ReadBoolean();
        }
    }
}
