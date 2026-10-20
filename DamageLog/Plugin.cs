using Aerolt.API;
using BepInEx;
using BepInEx.Logging;

namespace DamageLog
{
    [BepInPlugin(Guid, Name, Version)]
    [BepInDependency("com.Lodington.Aerolt")]
    public class Plugin : BaseUnityPlugin
    {
        public const string Name = "Aerolt - Damage Log";
        public const string Guid = "com.Lodington.DamageLog";
        public const string Version = "1.0.0";

        public static ManualLogSource Log = null!;

        private void Awake()
        {
            Log = Logger;
            AeroltAPI.RegisterSection(new DamageLogSection());
            Log.LogInfo($"{Name} {Version} loaded.");
        }
    }
}
