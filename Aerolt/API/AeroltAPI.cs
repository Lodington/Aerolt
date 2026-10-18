using System;
using System.Collections.Generic;
using Aerolt.UI;
using RoR2;
using UniverseLib.UI;

namespace Aerolt.API
{
    /// <summary>
    ///     Public extension API for Aerolt. Other BepInEx mods can reference the Aerolt assembly
    ///     and use this class to add their own tabs (and, if they want, their own windows) to the
    ///     Aerolt menu without forking the mod.
    ///
    ///     Typical usage from another plugin:
    ///     <code>
    ///     [BepInPlugin("my.cool.mod", "My Cool Mod", "1.0.0")]
    ///     [BepInDependency("com.Lodington.Aerolt")]
    ///     public class MyMod : BaseUnityPlugin
    ///     {
    ///         private void Awake()
    ///         {
    ///             // Register a tab. Safe to call any time if the Aerolt UI isn't built yet,
    ///             // the tab is queued and added as soon as the menu is created (on run start).
    ///             AeroltAPI.RegisterSection(new MySection());
    ///         }
    ///     }
    ///
    ///     public class MySection : IAeroltSection
    ///     {
    ///         public string Title => "My Tab";
    ///         public GameObject Root { get; private set; }
    ///
    ///         public void Build(GameObject contentParent)
    ///         {
    ///             Root = UIFactory.CreateVerticalGroup(contentParent, "MySection", ...);
    ///             // ... build your controls under Root ...
    ///         }
    ///
    ///         public void OnShown()
    ///         {
    ///             // Called each time the tab becomes active. Refresh from the selected player:
    ///             var user = AeroltAPI.SelectedPlayer;
    ///         }
    ///     }
    ///     </code>
    /// </summary>
    public static class AeroltAPI
    {
        private static readonly List<IAeroltSection> Pending = new();
        private static AeroltUniverseWindow? _window;

        /// <summary>True once the Aerolt UI exists (created when a run starts).</summary>
        public static bool IsReady => _window != null;

        /// <summary>
        ///     Raised when the Aerolt UI becomes ready. If you subscribe after it's already ready,
        ///     your handler is invoked immediately. Use this to build UI that needs the live
        ///     UniverseLib canvas (e.g. your own panels).
        /// </summary>
        public static event Action? Ready
        {
            add
            {
                _ready += value;
                if (IsReady) value?.Invoke();
            }
            remove => _ready -= value;
        }

        private static Action? _ready;

        /// <summary>
        ///     Register a tab in the main Aerolt window. The <paramref name="section" />'s
        ///     <see cref="IAeroltSection.Build" /> runs when the tab is created; <c>OnShown</c>
        ///     runs each time it becomes the active tab. Safe to call before or after the UI is
        ///     built early registrations are queued and flushed when the window is created.
        /// </summary>
        public static void RegisterSection(IAeroltSection section)
        {
            if (section == null) throw new ArgumentNullException(nameof(section));

            if (_window != null)
            {
                _window.RegisterSection(section);
                return;
            }

            if (!Pending.Contains(section))
                Pending.Add(section);
        }

        // --- player targeting (re-exported so mods don't depend on internal UI types) ---

        /// <summary>The player the Aerolt menu is currently targeting, or null.</summary>
        public static NetworkUser? SelectedPlayer => PlayerSelection.Selected;

        /// <summary>Change the targeted player.</summary>
        public static void SelectPlayer(NetworkUser? user) => PlayerSelection.Select(user);

        /// <summary>Raised whenever the targeted player changes.</summary>
        public static event Action? SelectedPlayerChanged
        {
            add => PlayerSelection.SelectionChanged += value;
            remove => PlayerSelection.SelectionChanged -= value;
        }

        // --- menu visibility ---

        /// <summary>True while the Aerolt menu is open.</summary>
        public static bool MenuOpen => AeroltUniverseUI.IsOpen;

        /// <summary>Open or close the Aerolt menu.</summary>
        public static void SetMenuVisible(bool visible) => AeroltUniverseUI.SetVisible(visible);

        /// <summary>Toggle the Aerolt menu.</summary>
        public static void ToggleMenu() => AeroltUniverseUI.Toggle();

        // --- advanced: build your own windows on Aerolt's canvas ---

        /// <summary>
        ///     The shared UniverseLib <see cref="UIBase" /> (canvas) that Aerolt is built on, or
        ///     null if the UI isn't ready yet. Advanced consumers can create their own
        ///     <c>PanelBase</c> against this instead of initializing UniverseLib themselves, so
        ///     their windows share Aerolt's canvas and cursor handling. Prefer
        ///     <see cref="RegisterSection" /> for simple extensions.
        /// </summary>
        public static UIBase? SharedUIBase => AeroltUniverseUI.UiBase;

        // --- internal plumbing (called by Aerolt itself) ---

        internal static void NotifyReady(AeroltUniverseWindow window)
        {
            _window = window;
            _ready?.Invoke();
        }

        /// <summary>Called by the window during construction to drain early registrations.</summary>
        internal static void FlushPendingSections(AeroltUniverseWindow window)
        {
            _window = window;
            foreach (var section in Pending)
                window.RegisterSection(section);
            Pending.Clear();
        }
    }
}
