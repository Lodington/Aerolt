using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ItemLoadouts
{

    public class LoadoutStore
    {
        private readonly string _path;
        public readonly List<LoadoutProfile> Profiles = new();

        public LoadoutStore(string path)
        {
            _path = path;
            Load();
        }

        public LoadoutProfile Add(string name, Dictionary<string, int> items)
        {
            var profile = new LoadoutProfile(name);
            foreach (var kvp in items)
                if (kvp.Value > 0)
                    profile.Items[kvp.Key] = kvp.Value;
            Profiles.Add(profile);
            Save();
            return profile;
        }

        public void Remove(LoadoutProfile profile)
        {
            if (Profiles.Remove(profile))
                Save();
        }

        public void Rename(LoadoutProfile profile, string name)
        {
            profile.Name = name;
            Save();
        }

        public void Save()
        {
            try
            {
                var dir = Path.GetDirectoryName(_path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var lines = Profiles.Select(p =>
                {
                    var items = string.Join(",", p.Items.Select(i => $"{i.Key}:{i.Value}"));
                    return $"{Sanitize(p.Name)}|{items}";
                });
                File.WriteAllLines(_path, lines.ToArray());
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Failed to save loadouts: {e}");
            }
        }

        private void Load()
        {
            Profiles.Clear();
            if (!File.Exists(_path)) return;

            try
            {
                foreach (var line in File.ReadAllLines(_path))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var sep = line.IndexOf('|');
                    if (sep < 0) continue;

                    var name = line.Substring(0, sep);
                    var itemsPart = line.Substring(sep + 1);
                    var profile = new LoadoutProfile(name);

                    if (!string.IsNullOrEmpty(itemsPart))
                        foreach (var pair in itemsPart.Split(','))
                        {
                            var colon = pair.LastIndexOf(':');
                            if (colon < 0) continue;
                            var itemName = pair.Substring(0, colon);
                            if (int.TryParse(pair.Substring(colon + 1), out var count) && count > 0)
                                profile.Items[itemName] = count;
                        }

                    Profiles.Add(profile);
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Failed to load loadouts: {e}");
            }
        }

        private static string Sanitize(string name)
        {
            return (name ?? "Profile").Replace('|', ' ');
        }
    }
}
