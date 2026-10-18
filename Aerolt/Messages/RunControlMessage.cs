using Aerolt.Managers;
using RoR2;
using UnityEngine.Networking;

namespace Aerolt.Messages
{
    public enum RunControlType
    {
        SetStopwatch,
        SetStagesCleared,
        SetDifficulty
    }

    /// <summary>
    ///     Server-authoritative changes to Run-level state: the run timer, stages cleared, and
    ///     difficulty. Sent through the existing Aerolt network channel.
    /// </summary>
    public class RunControlMessage : AeroltMessageBase
    {
        private RunControlType _type;
        private float _value;

        public RunControlMessage()
        {
        }

        public RunControlMessage(RunControlType type, float value)
        {
            _type = type;
            _value = value;
        }

        public override void Handle()
        {
            base.Handle();
            if (!NetworkServer.active || !Run.instance) return;

            switch (_type)
            {
                case RunControlType.SetStopwatch:
                    Run.instance.SetRunStopwatch(_value);
                    break;
                case RunControlType.SetStagesCleared:
                    Run.instance.NetworkstageClearCount = (int)_value;
                    break;
                case RunControlType.SetDifficulty:
                    Run.instance.selectedDifficulty = (DifficultyIndex)(int)_value;
                    break;
            }
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.WritePackedUInt32((uint)_type);
            writer.Write(_value);
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            _type = (RunControlType)reader.ReadPackedUInt32();
            _value = reader.ReadSingle();
        }
    }
}
