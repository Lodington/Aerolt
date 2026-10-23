using Aerolt.API;
using BepInEx;
using BepInEx.Logging;

namespace EntityInspector
{

    [BepInPlugin(Guid, Name, Version)]
    [BepInDependency("com.Lodington.Aerolt")]
    public class Plugin : BaseUnityPlugin
    {
        public const string Name = "Aerolt - Entity Inspector";
        public const string Guid = "com.Lodington.EntityInspector";
        public const string Version = "1.0.0";

        public static ManualLogSource Log = null!;

        private void Awake()
        {
            Log = Logger;
            AeroltAPI.RegisterSection(new EntityInspectorSection());
            Log.LogInfo($"{Name} {Version} loaded.");
        }
    }
}
