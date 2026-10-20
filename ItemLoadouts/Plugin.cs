using System.IO;
using Aerolt.API;
using BepInEx;
using BepInEx.Logging;

namespace ItemLoadouts
{

    [BepInPlugin(Guid, Name, Version)]
    [BepInDependency("com.Lodington.Aerolt")]
    public class Plugin : BaseUnityPlugin
    {
        public const string Name = "Aerolt - Item Loadouts";
        public const string Guid = "com.Lodington.ItemLoadouts";
        public const string Version = "1.0.0";

        public static ManualLogSource Log = null!;
        public static LoadoutStore Store = null!;

        private void Awake()
        {
            Log = Logger;

            var path = Path.Combine(Paths.ConfigPath, "Aerolt", "ItemLoadouts.txt");
            Store = new LoadoutStore(path);
            AeroltAPI.RegisterSection(new LoadoutsSection());

            Log.LogInfo($"{Name} {Version} loaded ({Store.Profiles.Count} saved loadouts).");
        }
    }
}
