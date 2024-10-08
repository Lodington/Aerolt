using System;
using Aerolt.Buttons;
using Aerolt.Classes;
using Aerolt.Helpers;
using Aerolt.Messages;
using RiskOfOptions;
using RoR2;
using ZioRiskOfOptions;

namespace Aerolt.Managers
{
    public class PlayerConfigBinding // TODO network the values;
    {
        public ValueWrapper<bool> Aimbot;
        public ValueWrapper<float> AimbotWeight;
        public ValueWrapper<bool> AlwaysSprint;
        public CustomButton customButton; // this probably shouldn't be in here, but it was convenient
        public ValueWrapper<bool> GodMode;
        public ValueWrapper<bool> InfiniteSkills;
        public ValueWrapper<bool> Noclip;
        public ValueWrapper<bool> NoclipInteractForDown;
        private readonly NetworkUser user;

        public PlayerConfigBinding(NetworkUser currentUser, CustomButton button)
        {
            customButton = button;
            user = currentUser;

            AimbotWeight = ValueWrapper.Get("PlayerMenu", "AimbotWeight", 0.5f, "", user);
            AimbotWeight.settingChanged += OnAimbotWeightChanged;
            Aimbot = ValueWrapper.Get("PlayerMenu", "Aimbot", false, "", user);
            Aimbot.settingChanged += OnAimbotChanged;
            InfiniteSkills = ValueWrapper.Get("PlayerMenu", "InfiniteSkills", false, "", user);
            InfiniteSkills.settingChanged += OnInfiniteSkillsChanged;
            AlwaysSprint = ValueWrapper.Get("PlayerMenu", "AlwaysSprint", false, "", user);
            AlwaysSprint.settingChanged += OnAlwaysSprintChanged;
            GodMode = ValueWrapper.Get("PlayerMenu", "GodMode", false, "", user);
            GodMode.settingChanged += OnGodModeChanged;
            Noclip = ValueWrapper.Get("PlayerMenu", "Noclip", false, "", user);
            Noclip.settingChanged += OnNoclipChanged;
            var who = user && user.localUser != null
                ? Load.Name + " " + user.GetNetworkPlayerName().GetResolvedName()
                : Load.Guid;
            NoclipInteractForDown = ValueWrapper.Get("PlayerMenu", "NoclipInteractForDown", true, "", user,
                firstSetup: config => ModSettingsManager.AddOption(new ZioCheckBoxOption(config), who, who));
            NoclipInteractForDown.settingChanged += OnNoclipForInteractDownChanged;
        }

        public void OnDestroy()
        {
            AimbotWeight.settingChanged -= OnAimbotWeightChanged;
            Aimbot.settingChanged -= OnAimbotChanged;
            InfiniteSkills.settingChanged -= OnInfiniteSkillsChanged;
            AlwaysSprint.settingChanged -= OnAlwaysSprintChanged;
            GodMode.settingChanged -= OnGodModeChanged;
            Noclip.settingChanged -= OnNoclipChanged;
            NoclipInteractForDown.settingChanged -= OnNoclipForInteractDownChanged;
        }

        private void OnNoclipForInteractDownChanged()
        {
            if (!user.master) return;
            var body = user.master.GetBody();
            if (!body) return;
            var comp = body.GetComponent<NoclipBehavior>();
            if (comp) comp.shouldUseInteractForDown = NoclipInteractForDown.Value;
        }

        private void OnNoclipChanged()
        {
            if (!user.master) return;
            SetNoclip(user.master.GetBody(), Noclip.Value);
            OnNoclipForInteractDownChanged();
        }

        private void OnGodModeChanged()
        {
            if (!user.master) return;
            SetGodMode(user.master.GetBody(), GodMode.Value);
        }

        private void OnAlwaysSprintChanged()
        {
            if (!user.master) return;
            SetAlwaysSprint(user.master.GetBody(), AlwaysSprint.Value);
        }

        private void OnInfiniteSkillsChanged()
        {
            if (!user.master) return;
            SetInfiniteSkills(user.master.GetBody(), InfiniteSkills.Value);
        }

        private void OnAimbotChanged()
        {
            if (!user.master) return;
            SetAimbot(user.master.GetBody(), Aimbot.Value, AimbotWeight.Value);
        }

        private void OnAimbotWeightChanged()
        {
            if (!user.master) return;
            SetAimbotWeight(user.master.GetBody(), AimbotWeight.Value);
        }

        public void Bind(Action updateCheckboxValues)
        {
            AimbotWeight.settingChanged += updateCheckboxValues;
            Aimbot.settingChanged += updateCheckboxValues;
            InfiniteSkills.settingChanged += updateCheckboxValues;
            AlwaysSprint.settingChanged += updateCheckboxValues;
            GodMode.settingChanged += updateCheckboxValues;
            Noclip.settingChanged += updateCheckboxValues;
        }

        public void UnBind(Action updateCheckboxValues)
        {
            AimbotWeight.settingChanged -= updateCheckboxValues;
            Aimbot.settingChanged -= updateCheckboxValues;
            InfiniteSkills.settingChanged -= updateCheckboxValues;
            AlwaysSprint.settingChanged -= updateCheckboxValues;
            GodMode.settingChanged -= updateCheckboxValues;
            Noclip.settingChanged -= updateCheckboxValues;
        }

        public void Sync()
        {
            AimbotWeight.Sync();
            Aimbot.Sync();
            InfiniteSkills.Sync();
            AlwaysSprint.Sync();
            GodMode.Sync();
            Noclip.Sync();
        }

        public static void SetAimbot(CharacterBody bod, bool enable, float weight)
        {
            if (bod) bod.ToggleComponent<AimbotBehavior>(enable, comp => comp.weight = weight);
        }

        public static void SetAimbotWeight(CharacterBody bod, float weight)
        {
            if (!bod) return;
            var behavior = bod.GetComponent<AimbotBehavior>();
            if (behavior)
                behavior.weight = weight;
        }

        public static void SetGodMode(CharacterBody bod, bool enable)
        {
            if (bod) new GodModeMessage(bod.master, enable).SendToServer();
        }

        public static void SetNoclip(CharacterBody bod, bool enable)
        {
            if (bod) bod.ToggleComponent<NoclipBehavior>(enable);
        }

        public static void SetInfiniteSkills(CharacterBody bod, bool enable)
        {
            if (enable && bod) bod.onSkillActivatedAuthority += InfiniteSkillsActivated;
            else if (bod) bod.onSkillActivatedAuthority -= InfiniteSkillsActivated;
        }

        public static void SetAlwaysSprint(CharacterBody bod, bool enable)
        {
            if (bod) bod.ToggleComponent<AlwaysSprintBehavior>(enable);
        }

        public static void InfiniteSkillsActivated(GenericSkill obj)
        {
            obj.AddOneStock();
        }
    }
}