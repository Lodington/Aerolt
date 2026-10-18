using System.Collections.Generic;
using RoR2;

namespace Aerolt.Classes
{

    public static class PlayerConfigFiles
    {
        private static readonly Dictionary<LocalUser, ZioConfigFile.ZioConfigFile> Files = new();


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
