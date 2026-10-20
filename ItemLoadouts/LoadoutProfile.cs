using System.Collections.Generic;

namespace ItemLoadouts
{
    public class LoadoutProfile
    {
        public string Name;

        public readonly Dictionary<string, int> Items = new();

        public LoadoutProfile(string name)
        {
            Name = name;
        }
    }
}
