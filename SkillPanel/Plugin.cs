using Aerolt.API;
using BepInEx;
using BepInEx.Logging;

namespace SkillPanel
{

    [BepInPlugin(Guid, Name, Version)]
    [BepInDependency("com.Lodington.Aerolt")]
    public class Plugin : BaseUnityPlugin
    {
        public const string Name = "Aerolt - Skill Panel";
        public const string Guid = "com.Lodington.SkillPanel";
        public const string Version = "1.0.0";

        public static ManualLogSource Log = null!;

        private void Awake()
        {
            Log = Logger;
            AeroltAPI.RegisterSection(new SkillPanelSection());
            Log.LogInfo($"{Name} {Version} loaded.");
        }
    }
}
