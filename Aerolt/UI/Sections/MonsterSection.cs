using System;
using System.Collections.Generic;
using System.Linq;
using Aerolt.Classes;
using Aerolt.Messages;
using RoR2;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib;
using UniverseLib.UI;

namespace Aerolt.UI.Sections
{
    public class MonsterSection : IAeroltSection
    {
        private readonly List<EquipmentIndex> _eliteIndices = new();
        private Dropdown _teamDropdown = null!;
        private Dropdown _eliteDropdown = null!;
        private Toggle _brainDead = null!;
        private IconButtonList<CharacterMaster> _list = null!;

        public string Title => "Monsters";
        public GameObject Root { get; private set; } = null!;

        public void Build(GameObject contentParent)
        {
            Root = UIFactory.CreateVerticalGroup(contentParent, "MonsterSection", false, false, true, true, 4,
                default, new Color(0.14f, 0.14f, 0.14f));
            UIFactory.SetLayoutElement(Root, flexibleWidth: 9999, flexibleHeight: 9999);

            var options = UIFactory.CreateHorizontalGroup(Root, "Options", false, false, true, true, 6);
            UIFactory.SetLayoutElement(options, minHeight: 32, flexibleHeight: 0, flexibleWidth: 9999);

            var teamNames = Enum.GetNames(typeof(TeamIndex));
            UIFactory.CreateDropdown(options, "TeamDropdown", out _teamDropdown, "Team", 14, null, teamNames);
            UIFactory.SetLayoutElement(_teamDropdown.gameObject, minHeight: 28, minWidth: 120, flexibleWidth: 9999);
            var monsterTeam = Array.IndexOf(teamNames, nameof(TeamIndex.Monster));
            if (monsterTeam >= 0) _teamDropdown.value = monsterTeam;

            BuildEliteOptions();
            UIFactory.CreateDropdown(options, "EliteDropdown", out _eliteDropdown, "Elite", 14, null,
                _eliteNames.ToArray());
            UIFactory.SetLayoutElement(_eliteDropdown.gameObject, minHeight: 28, minWidth: 120, flexibleWidth: 9999);

            var toggleObj = UIFactory.CreateToggle(options, "BrainDead", out _brainDead, out var toggleText);
            UIFactory.SetLayoutElement(toggleObj, minHeight: 28, minWidth: 100, flexibleWidth: 0);
            toggleText.text = "Braindead";

            _list = new IconButtonList<CharacterMaster>(
                master => Language.GetString(master.bodyPrefab.GetComponent<CharacterBody>().baseNameToken),
                (master, icon, stillValid) =>
                {
                    var sprite = IconCache.GetMasterIcon(master);
                    if (sprite && icon && stillValid()) icon.sprite = sprite;
                },
                SpawnMonster);

            _list.Build(Root, MasterCatalog.allMasters
                .Where(x => x.bodyPrefab && x.bodyPrefab.GetComponent<CharacterBody>()?.portraitIcon)
                .OrderBy(x => Language.GetString(x.bodyPrefab.GetComponent<CharacterBody>().baseNameToken)));
        }

        private readonly List<string> _eliteNames = new();

        private void BuildEliteOptions()
        {
            _eliteNames.Clear();
            _eliteIndices.Clear();

            _eliteNames.Add("None");
            _eliteIndices.Add(EquipmentIndex.None);

            foreach (var eliteDef in EliteCatalog.eliteDefs)
            {
                if (!eliteDef || !eliteDef.eliteEquipmentDef) continue;
                _eliteNames.Add(Language.GetStringFormatted(eliteDef.modifierToken, "Monster"));
                _eliteIndices.Add(eliteDef.eliteEquipmentDef.equipmentIndex);
            }
        }

        public void OnShown()
        {
        }

        private void SpawnMonster(CharacterMaster monsterMaster)
        {
            var localUser = LocalUserManager.GetFirstLocalUser();
            if (localUser == null || !localUser.cachedBody) return;

            var location = localUser.cachedBody.transform.position;
            var body = monsterMaster.bodyPrefab;
            var elite = _eliteDropdown.value >= 0 && _eliteDropdown.value < _eliteIndices.Count
                ? _eliteIndices[_eliteDropdown.value]
                : EquipmentIndex.None;
            var team = (TeamIndex)_teamDropdown.value;

            new MonsterSpawnMessage(monsterMaster.name, body.name, location, team, elite, _brainDead.isOn,
                new Dictionary<ItemDef, uint>()).SendToServer();
        }
    }
}
