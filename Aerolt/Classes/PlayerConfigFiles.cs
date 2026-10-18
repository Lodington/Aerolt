using System.Collections.Generic;
using RoR2;

namespace Aerolt.Classes
{
    /// <summary>
    ///     Owns the per-local-user config files (profiles). Previously this lived on the old UI's
    ///     MenuInfo; extracted so per-user ValueWrappers keep working without the old UI.
    /// </summary>
    public static class PlayerConfigFiles
    {
        private static readonly Dictionary<LocalUser, ZioConfigFile.ZioConfigFile> Files = new();

        /// <summary>Get-or-create the config file for a local user's profile.</summary>
        public static ZioConfigFile.ZioConfigFile GetForLocalUser(LocalUser localUser)
        {
            if (!Files.TryGetValue(localUser, out var file))
            {
                file = new ZioConfigFile.ZioConfigFile(RoR2Application.cloudStorage,
                    $"/Aerolt/Profiles/{localUser.userProfile.fileName}.cfg", true);
                Files.Add(localUser, file);
            }

            return file;
        }
    }
}
