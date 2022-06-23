using System;
using System.Collections.Generic;
using System.Linq;
using Aerolt.Helpers;
using Aerolt.Managers;
using RoR2;
using RoR2.ContentManagement;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Aerolt.Classes
{
    public class Player : MonoBehaviour
    {
        private bool infiniteSkills;
        private NetworkUser owner;
        private ZioConfigFile.ZioConfigFile configFile;

        public Toggle godModeToggle;
        public Toggle infiniteSkillsToggle;
        public Toggle noClipToggle;
        public Toggle alwaysSprintToggle;
        public Toggle doMassiveDamageToggle;
        public Toggle disableMobSpawnsToggle;
        public Toggle aimbotToggle;

        public void InfiniteSkillsToggle()
        {
            var body = GetBody();
            if (!infiniteSkills)
            {
                if (body)
                    body.onSkillActivatedServer += SkillActivated;
                infiniteSkills = true;
                return;
            }
            if (body)
                body.onSkillActivatedServer -= SkillActivated;
            infiniteSkills = false;
        }

        public void NoClipToggle()
        {
            noClipOn = !noClipOn;
            ApplyNoclip();
        }

        private void ApplyNoclip()
        {
            var body = GetBody();
            if (!body) return;
            
            var behavior = body.GetComponent<NoclipBehavior>();
            if (behavior && !noClipOn)
                Destroy(behavior);
            else if (noClipOn && !behavior)
                body.gameObject.AddComponent<NoclipBehavior>();
        }

        public void AimbotToggle()
        {
            aimbotOn = !aimbotOn;
            ApplyAimbot();
        }

        public void ApplyAimbot()
        {
            var body = GetBody();
            if (!body) return;
            
            var behavior = body.GetComponent<AimbotBehavior>();
            if (behavior && !aimbotOn)
                Destroy(behavior);
            else if (aimbotOn && !behavior)
                body.gameObject.AddComponent<AimbotBehavior>();
        }

        public void AlwaysSprintToggle(){}
        
        private float localBaseDamage;
        public void DoMassiveDamageToggle() // Do we still need this? the body stats do the same job
        {
            
            if (doMassiveDamageToggle.isOn)
            {
                GetBody().baseDamage = 1000000;
                return;
            }
            
            GetBody().baseDamage = localBaseDamage;

        }
        public void DisableMobSpawnsToggle()
        {
            foreach (var director in CombatDirector.instancesList)
                director.monsterSpawnTimer = disableMobSpawnsToggle.isOn ? float.PositiveInfinity : 0f;
        }
        

        public void Awake()
        {
            var panelManager = GetComponentInParent<PanelManager>();
            owner = panelManager.owner;
            configFile = panelManager.configFile;
            owner.master.onBodyStart += MasterBodyStart;
            owner.master.onBodyDestroyed += MasterDestroyBody;
            
            ApplyNoclip();
            noClipToggle.SetIsOnWithoutNotify(noClipOn);
            ApplyAimbot();
            aimbotToggle.SetIsOnWithoutNotify(aimbotOn);
            infiniteSkillsToggle.SetIsOnWithoutNotify(infiniteSkills);
            godModeToggle.SetIsOnWithoutNotify(PlayerCharacterMasterController.instances.Any(x=> x.master.godMode));
            
            localBaseDamage = GetBody().baseDamage;
        }

        private void OnDestroy()
        {
            owner.master.onBodyStart -= MasterBodyStart;
            owner.master.onBodyDestroyed -= MasterDestroyBody;
        }

        private CharacterBody _cachedBody;
        private bool noClipOn;
        private bool aimbotOn;

        public CharacterBody GetBody()
        {
            if (_cachedBody) return _cachedBody;
            _cachedBody = owner.master.GetBody();
            return _cachedBody;
        }
        
        private void MasterBodyStart(CharacterBody obj)
        {
            if (infiniteSkills)
            {
                obj.onSkillActivatedServer += SkillActivated;
            }
        }
        private void MasterDestroyBody(CharacterBody obj)
        {
            if (infiniteSkills)
            {
                obj.onSkillActivatedServer -= SkillActivated;
            }
        }

        public void GodModeToggle()
        {
            bool hasNotYetRun = true;
            foreach (var playerInstance in PlayerCharacterMasterController.instances)
            {
                playerInstance.master.ToggleGod();
                if (hasNotYetRun)
                {
                    hasNotYetRun = false;
                }
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
        public void AlwaysSprint() // TODO turn this into a component
        {
            var localUser = LocalUserManager.GetFirstLocalUser();
            if (localUser == null || localUser.cachedMasterController == null || localUser.cachedMasterController.master == null) return;
            var controller = localUser.cachedMasterController;
            var body = controller.master.GetBody();
            if (body && !body.isSprinting && !localUser.inputPlayer.GetButton("Sprint"))
                controller.sprintInputPressReceived = true;
        }

        public void KillAllMobs()
        {
            foreach (var characterMaster in CharacterMaster.instancesList.Where(x => x.teamIndex != owner.master.teamIndex))
            {
                var body = characterMaster.GetBody();
                if (body)
                    Chat.AddMessage($"<color=yellow>Killed {body.GetDisplayName()} </color>");
                characterMaster.TrueKill();
            }
        }
        
    }
}