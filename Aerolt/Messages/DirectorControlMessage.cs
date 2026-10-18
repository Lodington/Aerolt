using Aerolt.Managers;
using RoR2;
using UnityEngine.Networking;

namespace Aerolt.Messages
{
    public enum DirectorControlType
    {
        StopSpawns,
        ResumeSpawns,
        SetCredits,
        StartBoss
    }

    /// <summary>
    ///     Server-authoritative combat-director control: pause/resume monster spawns, set monster
    ///     credits, or kick off the teleporter boss.
    /// </summary>
    public class DirectorControlMessage : AeroltMessageBase
    {
        private DirectorControlType _type;
        private float _value;

        public DirectorControlMessage()
        {
        }

        public DirectorControlMessage(DirectorControlType type, float value = 0f)
        {
            _type = type;
            _value = value;
        }

        public override void Handle()
        {
            base.Handle();
            if (!NetworkServer.active) return;

            switch (_type)
            {
                case DirectorControlType.StopSpawns:
                    foreach (var director in CombatDirector.instancesList)
                        director.monsterSpawnTimer = float.PositiveInfinity;
                    break;
                case DirectorControlType.ResumeSpawns:
                    foreach (var director in CombatDirector.instancesList)
                        director.monsterSpawnTimer = 0f;
                    break;
                case DirectorControlType.SetCredits:
                    foreach (var director in CombatDirector.instancesList)
                        director.monsterCredit = _value;
                    break;
                case DirectorControlType.StartBoss:
                    StartBoss();
                    break;
            }
        }

        private static void StartBoss()
        {
            var tp = TeleporterInteraction.instance;
            if (!tp || !tp.isIdle) return;
            // Begin the teleporter event, which spawns the boss wave (same as a player activating it).
            tp.OnInteractionBegin(null);
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
            _type = (DirectorControlType)reader.ReadPackedUInt32();
            _value = reader.ReadSingle();
        }
    }
}
