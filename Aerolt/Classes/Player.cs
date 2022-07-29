using System;
using System.Collections.Generic;
using System.Linq;
using Aerolt.Managers;
using Aerolt.Messages;
using BepInEx.Bootstrap;
using RiskOfOptions;
using RoR2;
using RoR2.ContentManagement;
using UnityEngine;
using UnityEngine.Networking;
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

        public Slider aimbotWeightSlider;
        
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
            var panelManager = GetComponentInParent<MenuInfo>();
            owner = panelManager.Owner;
            configFile = panelManager.ConfigFile;
            owner.master.onBodyStart += MasterBodyStart;
            owner.master.onBodyDestroyed += MasterDestroyBody;

            noclipEntry = configFile.Bind("PlayerMenu", "Noclip", false, "");
            aimbotEntry = configFile.Bind("PlayerMenu", "Aimbot", false, "");
            infiniteSkillsEntry = configFile.Bind("PlayerMenu", "InfiniteSkills", false, "");
            alwaysSprintEntry = configFile.Bind("PlayerMenu", "AlwaysSprint", false, "");
            // Shared Values
            godModeEntry = Load.Instance.configFile.Bind("PlayerMenu", "GodMode", false, "");
            mobSpawnsEntry = Load.Instance.configFile.Bind("PlayerMenu", "MobSpawns", false, "");
            aimbotWeightEntry = configFile.Bind("PlayerMenu", "AimbotWeight", 0.5f, "0 is weighted entirely to distance, while 1 is entirely to angle.");
            aimbotWeightEntry.SettingChanged += AimbotWeightEntryChanged;

            Apply();
        }

        public void AimbotWeightChanged(float arg0)
        {
            aimbotWeightDirty = true;
        }

        private void AimbotWeightEntryChanged(ZioConfigEntryBase arg1, object arg2, bool arg3)
        {
            var body = GetBody();
            if (!body) return;
            var aimbot = body.GetComponent<AimbotBehavior>();
            if (aimbot) aimbot.weight = aimbotWeightEntry.Value / 100f; // normalize this to 0-1, should be done in slider already lodington wtf
        }

        private void Apply()
        {
            //godModeEntry.Value = PlayerCharacterMasterController.instances.Any(x => x && x.master && x.master.godMode);
            godModeToggle.SetIsOnWithoutNotify(godModeEntry.Value);
            disableMobSpawnsToggle.SetIsOnWithoutNotify(mobSpawnsEntry.Value);
            ApplyMobSpawns();
            
            ApplySprint();
            alwaysSprintToggle.SetIsOnWithoutNotify(alwaysSprintEntry.Value);
            ApplyAimbot();
            if (aimbotWeightSlider)
                aimbotWeightSlider.SetValueWithoutNotify(aimbotWeightEntry.Value);
            AimbotWeightEntryChanged(null, null, false);
            aimbotToggle.SetIsOnWithoutNotify(aimbotEntry.Value);
            ApplyInfiniteSkills();
            infiniteSkillsToggle.SetIsOnWithoutNotify(infiniteSkillsEntry.Value);

            noClipToggle.SetIsOnWithoutNotify(noclipEntry.Value);
            noclipInteractDown = configFile.Bind("PlayerMenu", "NoclipInteractDown", true, "Should holding interact move you down when noclipping.");
            noclipInteractDown.SettingChanged += NoclipInteractChanged;
            if (Chainloader.PluginInfos.ContainsKey("bubbet.zioriskofoptions")) MakeRiskOfOptions();
            ApplyNoclip();
        }

        private void Update()
        {
            if (aimbotWeightDirty && !aimbotWeightSlider.isPointerDown)
            {
                aimbotWeightEntry.Value = aimbotWeightSlider.value;
                aimbotWeightDirty = false;
            }
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
            aimbotWeightEntry.SettingChanged -= AimbotWeightEntryChanged;
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
        private ZioConfigEntry<float> aimbotWeightEntry;
        private bool aimbotWeightDirty;

        public CharacterBody GetBody()
        {
            if (_cachedBody) return _cachedBody;
            _cachedBody = owner.master.GetBody();
            return _cachedBody;
        }
        
        public static void MasterBodyStart(CharacterBody obj)
        {
            if (true)
            {
                obj.onSkillActivatedAuthority += SkillActivated;
            }
        }
        public static void MasterDestroyBody(CharacterBody obj)
        {
            if (true)
            {
                obj.onSkillActivatedAuthority -= SkillActivated;
            }
        }

        public static void ApplyGodModeForAll(bool godmodeEnabled)
        {
            foreach (var playerInstance in PlayerCharacterMasterController.instances)
            {
                playerInstance.master.godMode = godmodeEnabled;
                playerInstance.master.UpdateBodyGodMode();
            }
        }

        public static void RollRandomItems(CharacterBody localUser)
        {
            WeightedSelection<List<PickupIndex>> weightedSelection = new WeightedSelection<List<PickupIndex>>(8);
            weightedSelection.AddChoice(Run.instance.availableTier1DropList, 80f);
            weightedSelection.AddChoice(Run.instance.availableTier2DropList, 19f);
            weightedSelection.AddChoice(Run.instance.availableTier3DropList, 1f);
            var items = new Dictionary<ItemDef, int>();
            for (int i = 0; i < Random.Range(0, 100); i++)
            {
                List<PickupIndex> list = weightedSelection.Evaluate(Random.value);
                var def = PickupCatalog.GetPickupDef(list[Random.Range(0, list.Count)]);
                if (def == null) continue;
                var item = ItemCatalog.GetItemDef(def.itemIndex);
                if (!items.ContainsKey(item)) items[item] = localUser.inventory.GetItemCount(item);
                items[item] += Random.Range(0, 100);
            }

            if (NetworkServer.active)
                foreach (var item in items)
                    localUser.inventory.GiveItem(item.Key, item.Value);
            else
                new SetItemCountMessage(localUser.inventory, items).SendToServer();

        }
        
        public static void SkillActivated(GenericSkill obj) => obj.AddOneStock();

        public static void GiveAllItems()
        {
            foreach (var networkUser in NetworkUser.readOnlyInstancesList) GiveAllItemsTo(networkUser);
        }

        public static void GiveAllItemsTo(NetworkUser networkUser)
        {
            if (NetworkServer.active) foreach (var item in ContentManager.itemDefs) networkUser.master.inventory.GiveItem(item);
            else
            {
                var items = ContentManager.itemDefs.ToDictionary(x => x, def => networkUser.master.inventory.GetItemCount(def) + 1);
                new SetItemCountMessage(networkUser.master.inventory, items).SendToServer();
            }
        }
        public static void ClearItemsTo(NetworkUser networkUser)
        {
            if (NetworkServer.active) foreach (var item in ContentManager.itemDefs) networkUser.master.inventory.RemoveItem(item, networkUser.master.inventory.GetItemCount(item));
            else
            {
                var items = ContentManager.itemDefs.ToDictionary(x => x, _ => 0);
                new SetItemCountMessage(networkUser.master.inventory, items).SendToServer();
            }
        }

        public static void GiveAllItemsToAll()
        {
            foreach (var networkUser in NetworkUser.readOnlyInstancesList)
                GiveAllItemsTo(networkUser);
        }

        public static void ClearInventory()
        {
            foreach (var networkUser in NetworkUser.readOnlyInstancesList)
            {
                if (!networkUser.isLocalPlayer) continue;
                ClearItemsTo(networkUser);
            }
        }
        public static void ClearInventoryAll()
        {
            foreach (var networkUser in NetworkUser.readOnlyInstancesList)
            {
                ClearItemsTo(networkUser);
            }
        }
       
        
    }
}