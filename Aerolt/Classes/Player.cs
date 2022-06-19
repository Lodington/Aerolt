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


        public void InfiniteSkillsToggle()
        {
            
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

        public void AlwaysSprintToggle(){}
        public void DoMassiveDamageToggle(){}
        public void DisableMobSpawnsToggle(){}
        

        public void Awake()
        {
            var panelManager = GetComponentInParent<PanelManager>();
            owner = panelManager.owner;
            configFile = panelManager.configFile;
            owner.master.onBodyStart += MasterBodyStart;
            owner.master.onBodyDestroyed += MasterDestroyBody;
            
            ApplyNoclip();
            noClipToggle.SetIsOnWithoutNotify(noClipOn);
            infiniteSkillsToggle.SetIsOnWithoutNotify(infiniteSkills);
            godModeToggle.SetIsOnWithoutNotify(PlayerCharacterMasterController.instances.Any(x=> x.master.godMode));
        }

        private void OnDestroy()
        {
            owner.master.onBodyStart -= MasterBodyStart;
            owner.master.onBodyDestroyed -= MasterDestroyBody;
        }

        private CharacterBody _cachedBody;
        private bool noClipOn;

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
            var localUser = LocalUserManager.GetFirstLocalUser();
            WeightedSelection<List<PickupIndex>> weightedSelection = new WeightedSelection<List<PickupIndex>>(8);
            weightedSelection.AddChoice(Run.instance.availableTier1DropList, 80f);
            weightedSelection.AddChoice(Run.instance.availableTier2DropList, 19f);
            weightedSelection.AddChoice(Run.instance.availableTier3DropList, 1f);
            for (int i = 0; i < Random.Range(0, 100); i++)
            {
                List<PickupIndex> list = weightedSelection.Evaluate(UnityEngine.Random.value);
                localUser.cachedMaster.inventory.GiveItem(list[UnityEngine.Random.Range(0, list.Count)].itemIndex, Random.Range(0, 100));
            }
            
        }

        #region InfiniteSkills
        public void SkillToggle()
        {
            var body = owner.GetCurrentBody();
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
        
        private void SkillActivated(GenericSkill obj)
        {
            obj.AddOneStock();
        }
        #endregion

        public void GiveAllItems()
        {
            foreach (var networkUser in NetworkUser.readOnlyInstancesList)
            {
                if (!networkUser.isLocalPlayer) continue;
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
            
        }
        public void ClearInventoryAll()
        {
            
        }
        public void AimBot()
        {
            if (Tools.CursorIsVisible())
                return;
            var localUser = LocalUserManager.GetFirstLocalUser();
            var controller = localUser.cachedMasterController;
            if (!controller)
                return;
            var body = controller.master.GetBody();
            if (!body)
                return;
            var inputBank = body.GetComponent<InputBankTest>();
            var aimRay = new Ray(inputBank.aimOrigin, inputBank.aimDirection);
            var bullseyeSearch = new BullseyeSearch();
            var team = body.GetComponent<TeamComponent>();
            bullseyeSearch.teamMaskFilter = TeamMask.all;
            bullseyeSearch.teamMaskFilter.RemoveTeam(team.teamIndex);
            bullseyeSearch.filterByLoS = true;
            bullseyeSearch.searchOrigin = aimRay.origin;
            bullseyeSearch.searchDirection = aimRay.direction;
            bullseyeSearch.sortMode = BullseyeSearch.SortMode.Distance;
            bullseyeSearch.maxDistanceFilter = float.MaxValue;
            bullseyeSearch.maxAngleFilter = 20f;// ;// float.MaxValue;
            bullseyeSearch.RefreshCandidates();
            var hurtBox = bullseyeSearch.GetResults().FirstOrDefault();
            if (hurtBox)
            {
                Vector3 direction = hurtBox.transform.position - aimRay.origin;
                inputBank.aimDirection = direction;
            }
        }
        public void AlwaysSprint()
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