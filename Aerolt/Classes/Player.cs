using System;
using System.Collections.Generic;
using System.Linq;
using Aerolt.Managers;
using BepInEx.Bootstrap;
using RiskOfOptions;
using RoR2;
using RoR2.ContentManagement;
using UnityEngine;
using UnityEngine.UI;
using ZioConfigFile;
using ZioRiskOfOptions;
using Random = UnityEngine.Random;

namespace Aerolt.Classes
{
    public class Player : MonoBehaviour
    {
        private NetworkUser owner;
        private ZioConfigFile.ZioConfigFile configFile;

        public Toggle godModeToggle;
        public Toggle infiniteSkillsToggle;
        public Toggle noClipToggle;
        public Toggle alwaysSprintToggle;
        public Toggle disableMobSpawnsToggle;
        public Toggle aimbotToggle;

        public void InfiniteSkillsToggle()
        {
            infiniteSkillsEntry.Value = !infiniteSkillsEntry.Value;
            ApplyInfiniteSkills();
        }

        public void ApplyInfiniteSkills()
        {
            var body = GetBody();
            if (infiniteSkillsEntry.Value)
            {
                if (body)
                    body.onSkillActivatedAuthority += SkillActivated;
                return;
            }
            if (body)
                body.onSkillActivatedAuthority -= SkillActivated;
        }

        public void NoClipToggle()
        {
            noclipEntry.Value = !noclipEntry.Value;
            ApplyNoclip();
        }

        private void ApplyNoclip()
        {
            var body = GetBody();
            if (!body) return;
            
            var behavior = body.GetComponent<NoclipBehavior>();
            if (behavior && !noclipEntry.Value)
                Destroy(behavior);
            else if (noclipEntry.Value && !behavior)
            {
                var noclip = body.gameObject.AddComponent<NoclipBehavior>();
                noclip.shouldUseInteractForDown = noclipInteractDown.Value;
            }
        }

        public void AimbotToggle()
        {
            aimbotEntry.Value = !aimbotEntry.Value;
            ApplyAimbot();
        }

        public void ApplyAimbot()
        {
            var body = GetBody();
            if (!body) return;
            
            var behavior = body.GetComponent<AimbotBehavior>();
            if (behavior && !aimbotEntry.Value)
                Destroy(behavior);
            else if (aimbotEntry.Value && !behavior)
                body.gameObject.AddComponent<AimbotBehavior>();
        }

        public void AlwaysSprintToggle()
        {
            alwaysSprintEntry.Value = !alwaysSprintEntry.Value;
            ApplySprint();
        }

        private void ApplySprint()
        {
            var body = GetBody();
            if (!body) return;
            
            var behavior = body.GetComponent<AlwaysSprintBehavior>();
            if (behavior && !alwaysSprintEntry.Value)
                Destroy(behavior);
            else if (alwaysSprintEntry.Value && !behavior)
                body.gameObject.AddComponent<AlwaysSprintBehavior>();
        }

        public void DisableMobSpawnsToggle()
        {
            mobSpawnsEntry.Value = !mobSpawnsEntry.Value;
            ApplyMobSpawns();
        }

        public void ApplyMobSpawns()
        {
            foreach (var director in CombatDirector.instancesList)
                director.monsterSpawnTimer = mobSpawnsEntry.Value ? float.PositiveInfinity : 0f;
        }
        

        public void Awake()
        {
            var panelManager = GetComponentInParent<PanelManager>();
            owner = panelManager.owner;
            configFile = panelManager.configFile;
            owner.master.onBodyStart += MasterBodyStart;
            owner.master.onBodyDestroyed += MasterDestroyBody;

            noclipEntry = configFile.Bind("PlayerMenu", "Noclip", false, "");
            aimbotEntry = configFile.Bind("PlayerMenu", "Aimbot", false, "");
            infiniteSkillsEntry = configFile.Bind("PlayerMenu", "InfiniteSkills", false, "");
            alwaysSprintEntry = configFile.Bind("PlayerMenu", "AlwaysSprint", false, "");
            // Shared Values
            godModeEntry = Load.Instance.configFile.Bind("PlayerMenu", "GodMode", false, "");
            mobSpawnsEntry = Load.Instance.configFile.Bind("PlayerMenu", "MobSpawns", false, "");

            Apply();
        }

        private void Apply()
        {
            ApplyMobSpawns();
            //godModeEntry.Value = PlayerCharacterMasterController.instances.Any(x => x && x.master && x.master.godMode);
            godModeToggle.SetIsOnWithoutNotify(godModeEntry.Value);
            ApplyGodMode();
            alwaysSprintToggle.SetIsOnWithoutNotify(alwaysSprintEntry.Value);
            disableMobSpawnsToggle.SetIsOnWithoutNotify(mobSpawnsEntry.Value);
            
            noClipToggle.SetIsOnWithoutNotify(noclipEntry.Value);
            ApplyAimbot();
            aimbotToggle.SetIsOnWithoutNotify(aimbotEntry.Value);
            ApplyInfiniteSkills();
            infiniteSkillsToggle.SetIsOnWithoutNotify(infiniteSkillsEntry.Value);

            noclipInteractDown = configFile.Bind("PlayerMenu", "NoclipInteractDown", true, "Should holding interact move you down when noclipping.");
            noclipInteractDown.SettingChanged += NoclipInteractChanged;
            if (Chainloader.PluginInfos.ContainsKey("bubbet.zioriskofoptions")) MakeRiskOfOptions();
            ApplyNoclip();
        }

        private void Update()
        {
            if (setup) return;
            var body = GetBody();
            if (!body) return;
            Apply();
            setup = true;
        }

        private void NoclipInteractChanged(ZioConfigEntryBase z, object o, bool b)
        {
            var body = GetBody();
            if (!body) return;
            var noclip = body.GetComponent<NoclipBehavior>();
            if (noclip) noclip.shouldUseInteractForDown = noclipInteractDown.Value;
        }

        private void MakeRiskOfOptions()
        {
            var who = Load.Name + " " + owner.GetNetworkPlayerName().GetResolvedName();
            if (!riskOfOptions.Contains(who))
            {
                ModSettingsManager.AddOption(new ZioCheckBoxOption(noclipInteractDown), who, who);
                riskOfOptions.Add(who);
            }
        }

        private void OnDestroy()
        {
            owner.master.onBodyStart -= MasterBodyStart;
            owner.master.onBodyDestroyed -= MasterDestroyBody;
            noclipInteractDown.SettingChanged -= NoclipInteractChanged;
        }

        private CharacterBody _cachedBody;
        
        private ZioConfigEntry<bool> noclipEntry;
        private ZioConfigEntry<bool> aimbotEntry;
        private ZioConfigEntry<bool> infiniteSkillsEntry;
        private ZioConfigEntry<bool> godModeEntry;
        private ZioConfigEntry<bool> mobSpawnsEntry;
        private ZioConfigEntry<bool> alwaysSprintEntry;
        private ZioConfigEntry<bool> noclipInteractDown;
        private static List<string> riskOfOptions = new();
        private bool setup;

        public CharacterBody GetBody()
        {
            if (_cachedBody) return _cachedBody;
            _cachedBody = owner.master.GetBody();
            return _cachedBody;
        }
        
        private void MasterBodyStart(CharacterBody obj)
        {
            if (infiniteSkillsEntry.Value)
            {
                obj.onSkillActivatedAuthority += SkillActivated;
            }
        }
        private void MasterDestroyBody(CharacterBody obj)
        {
            if (infiniteSkillsEntry.Value)
            {
                obj.onSkillActivatedAuthority -= SkillActivated;
            }
        }

        public void GodModeToggle()
        {
            godModeEntry.Value = !godModeEntry.Value;
            ApplyGodMode();
        }

        public void ApplyGodMode()
        {
            foreach (var playerInstance in PlayerCharacterMasterController.instances)
            {
                playerInstance.master.godMode = godModeEntry.Value;
                playerInstance.master.UpdateBodyGodMode();
            }
        }

        public void RollRandomItems()
        {
            var localUser = GetBody();
            WeightedSelection<List<PickupIndex>> weightedSelection = new WeightedSelection<List<PickupIndex>>(8);
            weightedSelection.AddChoice(Run.instance.availableTier1DropList, 80f);
            weightedSelection.AddChoice(Run.instance.availableTier2DropList, 19f);
            weightedSelection.AddChoice(Run.instance.availableTier3DropList, 1f);
            for (int i = 0; i < Random.Range(0, 100); i++)
            {
                List<PickupIndex> list = weightedSelection.Evaluate(Random.value);
                var def = PickupCatalog.GetPickupDef(list[Random.Range(0, list.Count)]);
                if (def == null) continue;
                localUser.inventory.GiveItem(def.itemIndex, Random.Range(0, 100));
            }
            
        }
        
        private void SkillActivated(GenericSkill obj) => obj.AddOneStock();

        public void GiveAllItems()
        {
            foreach (var networkUser in NetworkUser.readOnlyInstancesList)
            {
                if (!networkUser.isLocalPlayer) continue; // why
                foreach (var itemDef in ContentManager._itemDefs)
                    networkUser.master.inventory.GiveItem(itemDef, 1);
            }
        }
        public void GiveAllItemsToAll()
        {
            foreach (var networkUser in NetworkUser.readOnlyInstancesList)
            foreach (var itemDef in ContentManager._itemDefs)
                networkUser.master.inventory.GiveItem(itemDef, 1);
        }

        public void ClearInventory()
        {
            foreach (var networkUser in NetworkUser.readOnlyInstancesList)
            {
                if (!networkUser.isLocalPlayer) continue;
                foreach (var itemDef in ContentManager._itemDefs)
                    networkUser.master.inventory.RemoveItem(itemDef, networkUser.master.inventory.GetItemCount(itemDef));
            }
        }
        public void ClearInventoryAll()
        {
            foreach (var networkUser in NetworkUser.readOnlyInstancesList)
            {
                foreach (var itemDef in ContentManager._itemDefs)
                    networkUser.master.inventory.RemoveItem(itemDef, networkUser.master.inventory.GetItemCount(itemDef));
            }
        }
        public void KillAllMobs()
        {
            var mobs = CharacterMaster.instancesList.Where(x => x && x.teamIndex != owner.master.teamIndex).ToArray();
            foreach (var characterMaster in mobs)
            {
                var body = characterMaster.GetBody();
                if (body)
                    Chat.AddMessage($"<color=yellow>Killed {body.GetDisplayName()} </color>");
                characterMaster.TrueKill();
            }
        }
        
    }
}