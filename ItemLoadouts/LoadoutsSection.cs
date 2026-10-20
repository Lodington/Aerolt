using System.Collections.Generic;
using System.Linq;
using Aerolt.API;
using Aerolt.Messages;
using Aerolt.UI;
using RoR2;
using RoR2.ContentManagement;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;
using UniverseLib.UI.Models;

namespace ItemLoadouts
{
 
    public class LoadoutsSection : IAeroltSection
    {
        private GameObject _listContent = null!;
        private InputFieldRef _nameInput = null!;

        public string Title => "Loadouts";
        public GameObject Root { get; private set; } = null!;

        public bool ScrollableContent => false;

        public void Build(GameObject contentParent)
        {
            Root = UIFactory.CreateVerticalGroup(contentParent, "LoadoutsSection", false, false, true, true, 4,
                default, new Color(0.14f, 0.14f, 0.14f));
            UIFactory.SetLayoutElement(Root, flexibleWidth: 9999, flexibleHeight: 9999);

            var addRow = UIFactory.CreateHorizontalGroup(Root, "AddRow", false, false, true, true, 4);
            UIFactory.SetLayoutElement(addRow, minHeight: 30, flexibleHeight: 0, flexibleWidth: 9999);

            _nameInput = UIFactory.CreateInputField(addRow, "NameInput", "New profile name...");
            UIFactory.SetLayoutElement(_nameInput.Component.gameObject, minHeight: 28, flexibleWidth: 9999);

            var add = UIFactory.CreateButton(addRow, "Add", "Add Profile");
            UIFactory.SetLayoutElement(add.Component.gameObject, minHeight: 28, minWidth: 110, flexibleWidth: 0);
            add.OnClick += AddProfile;

            var hint = UIFactory.CreateLabel(Root, "Hint",
                "Add Profile copies the selected player's current items. Click a profile to clear " +
                "the inventory and load it.", TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(hint.gameObject, minHeight: 20, flexibleWidth: 9999);
            hint.fontSize = 12;

            UIFactory.CreateScrollView(Root, "List", out _listContent, out _, new Color(0.1f, 0.1f, 0.1f));
            UIFactory.SetLayoutGroup<VerticalLayoutGroup>(_listContent, false, false, true, true, 2, 2, 2, 2, 2);

            RebuildList();
        }

        public void OnShown()
        {
            RebuildList();
        }

        private void AddProfile()
        {
            var inv = SelectedInventory();
            if (!inv)
            {
                Chat.AddMessage("<color=red>Item Loadouts: no player selected.</color>");
                return;
            }

            var items = new Dictionary<string, int>();
            foreach (var def in ContentManager.itemDefs)
            {
                if (!def) continue;
                var count = inv!.GetItemCountEffective(def);
                if (count > 0)
                    items[def.name] = count;
            }

            var name = string.IsNullOrWhiteSpace(_nameInput.Text)
                ? $"Loadout {Plugin.Store.Profiles.Count + 1}"
                : _nameInput.Text.Trim();

            Plugin.Store.Add(name, items);
            _nameInput.Text = "";
            RebuildList();
            Chat.AddMessage($"<color=cyan>Saved loadout '{name}' ({items.Count} items).</color>");
        }

        private static void LoadProfile(LoadoutProfile profile)
        {
            var inv = SelectedInventory();
            if (!inv)
            {
                Chat.AddMessage("<color=red>Item Loadouts: no player selected.</color>");
                return;
            }
            var target = new Dictionary<ItemDef, int>();
            foreach (var def in ContentManager.itemDefs)
            {
                if (!def) continue;
                target[def] = profile.Items.TryGetValue(def.name, out var count) ? count : 0;
            }

            new SetItemCountMessage(inv!, target).SendToServer();
            Chat.AddMessage($"<color=cyan>Loaded loadout '{profile.Name}'.</color>");
        }

        private void RebuildList()
        {
            if (_listContent == null) return;

            foreach (Transform child in _listContent.transform)
                Object.Destroy(child.gameObject);

            if (Plugin.Store.Profiles.Count == 0)
            {
                var empty = UIFactory.CreateLabel(_listContent, "Empty",
                    "No loadouts yet. Grab some items, then click Add Profile.", TextAnchor.MiddleLeft);
                UIFactory.SetLayoutElement(empty.gameObject, minHeight: 26, flexibleWidth: 9999);
                return;
            }

            foreach (var profile in Plugin.Store.Profiles.ToList())
                CreateRow(profile);
        }

        private void CreateRow(LoadoutProfile profile)
        {
            var row = UIFactory.CreateHorizontalGroup(_listContent, "Row", false, false, true, true, 4,
                new Vector4(2, 2, 2, 2), new Color(0.13f, 0.13f, 0.13f));
            UIFactory.SetLayoutElement(row, minHeight: 30, flexibleWidth: 9999);

            // Click the main button to load the profile.
            var load = UIFactory.CreateButton(row, "Load", $"{profile.Name}  ({profile.Items.Count} items)");
            UIFactory.SetLayoutElement(load.Component.gameObject, minHeight: 26, flexibleWidth: 9999);
            load.OnClick += () => LoadProfile(profile);

            var del = UIFactory.CreateButton(row, "Delete", "X");
            UIFactory.SetLayoutElement(del.Component.gameObject, minHeight: 26, minWidth: 30, flexibleWidth: 0);
            del.OnClick += () =>
            {
                Plugin.Store.Remove(profile);
                RebuildList();
            };
        }

        private static Inventory? SelectedInventory()
        {
            var user = AeroltAPI.SelectedPlayer;
            if (!user || !user!.master) return null;
            return user.master.inventory ? user.master.inventory : null;
        }
    }
}
