# Aerolt

A rebuilt, networked cheat menu for Risk of Rain 2 with a fast new UniverseLib UI.

- Press **F1** (rebindable) or click the Aerolt logo in the top-left to open the menu.
- Networked: actions work in multiplayer, with a host-only mode to lock clients out.
- Select any player in your lobby from the bar at the top of the window; player-
  targeted tabs act on whoever is selected. Supports full 16-player lobbies.
- Mods can add their own tabs via the Extension API (see below).

## Host Only

A console command lets the host block clients from running Aerolt actions. There's also
a toggle for it under the Settings tab.

```
aerolt_host_only        -> flips the current state
aerolt_host_only 1      -> force on   (also true/on)
aerolt_host_only 0      -> force off  (also false/off)
```

## Player

Toggles and actions for the selected player:
- God Mode
- Noclip
- Aimbot (with an aimbot weight slider)
- Infinite Skills
- Always Sprint
- Disable Mob Spawns
- Kill / Revive / Goto / Bring / Kick / Ban
- Set Money, Lunar Coins, Void Coins, and Level
- Switch team

## Body Stats

Edit any float stat on the selected player's body (health, move speed, damage, etc.).
- Live "Default" view of the body's current values
- 3 saveable stat profiles you can apply with one click

## Items

Searchable grid of item icons for the selected player.
- Click an item to give one; Shift+click gives 5. Right-click removes one (Shift -5).
- Give All, Give Random, and Clear inventory
- **Edit Items** opens a two-grid window: the full item list on the left (click to give)
  and the player's current inventory on the right (click to remove). Shift for ±5.

## Equipment

Searchable equipment list; click an entry to give it to the selected player.

## Buffs

Searchable buff editor; set per-buff counts and apply them to the selected player.

## Spawn As

Change the selected player's character to any body in the game.

## Monsters

Spawn any monster and tune it before spawning:
- Braindead toggle (disables AI)
- Team selection
- Elite type
- Edit Items — give the monster an item loadout to spawn with (two-grid editor)
- Kill All Monsters

## Interactables

Spawn any interactable that has a spawn card (chests, shrines, printers, etc.) at your
aim point. Includes a Refresh button since the available cards depend on the stage.

## Teleporter

- Change to any selectable stage
- Spawn Gold / Newt / Blue (Celestial) / Void / All portals at the teleporter
- Instantly charge the teleporter
- Skip the current stage
- Add a Mountain shrine stack

## World

Run- and stage-wide controls:
- Game speed slider (local) with reset
- Charge Teleporter, Next Stage, Restart Stage
- Set the run timer and stages-cleared count
- Change difficulty
- Combat Director: stop/resume spawns, start the boss, set monster credits
- Toggle any artifact on or off

## ESP

Highlights objects in the world. Toggle any of:
- Teleporter
- Chests
- Multishops
- Barrels
- Scrappers
- Secret Buttons
- Duplicators
- Drones
- Shrines
- Newt Altars
- Advanced Info (expanded, non-collapsed labels)

ESP colors are configurable under Settings.

## Settings

- Host Only toggle
- Rebind the menu keybind
- Customize ESP colors (shared with the ESP tab and Risk of Options)

# Privacy Policy – No Personal Data Collection
This app and/or service does not collect, store, or transmit any personal information.
This mod only collects your Risk of Rain 2 profile name or your Steam name.

# Support us
Support Lodington
<a href="https://links.lodington.dev/buymeacoffee" target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/default-orange.png" alt="Buy Me A Coffee" height="41" width="174"></a>

Support Bubbet
<a href='https://ko-fi.com/H2H021R7R' target='_blank'><img height='36' style='border:0px;height:36px;' src='https://cdn.ko-fi.com/cdn/kofi3.png?v=3' border='0' alt='Buy Me a Coffee at ko-fi.com' /></a>

A shout out to Voidsenight✨#0060 for helping make the icons!

# Extension API (for mod developers)

Other BepInEx mods can add their own tabs (and windows) to the Aerolt menu without
forking it. Everything lives in the `Aerolt.API.AeroltAPI` static class.

## Setup

1. Add a reference to `Aerolt.dll` in your project.
2. Declare a dependency so Aerolt loads first:

```csharp
[BepInPlugin("my.cool.mod", "My Cool Mod", "1.0.0")]
[BepInDependency("com.Lodington.Aerolt")]
public class MyMod : BaseUnityPlugin { /* ... */ }
```

Aerolt builds its UI the first time a run starts (not at the main menu), so the window
may not exist when your plugin's `Awake` runs. The API handles this for you:
`RegisterSection` queues your tab if the UI isn't ready yet and adds it as soon as the
window is created.

## Add a tab

Implement `Aerolt.UI.IAeroltSection` and register it:

```csharp
using Aerolt.API;
using Aerolt.UI;
using UnityEngine;
using UniverseLib.UI;

public class MySection : IAeroltSection
{
    public string Title => "My Tab";          // nav button label
    public GameObject Root { get; private set; }

    public void Build(GameObject contentParent)
    {
        // Called once, when the tab is created. Build your controls under Root.
        Root = UIFactory.CreateVerticalGroup(contentParent, "MySection",
            false, false, true, true, 4, default, new Color(0.14f, 0.14f, 0.14f));
        UIFactory.SetLayoutElement(Root, flexibleWidth: 9999, flexibleHeight: 9999);

        var btn = UIFactory.CreateButton(Root, "Hello", "Say hi");
        btn.OnClick += () => Debug.Log("hi from my tab");
    }

    public void OnShown()
    {
        // Called every time this tab becomes active. Good place to refresh from the
        // currently targeted player.
        var user = AeroltAPI.SelectedPlayer;
    }
}
```

```csharp
private void Awake()
{
    AeroltAPI.RegisterSection(new MySection());
}
```

The UI is built with [UniverseLib](https://github.com/sinai-dev/UniverseLib); use its
`UIFactory` to create controls so your tab matches the rest of the menu.

### Scrolling

By default your tab's content is automatically wrapped in a vertical scroll view, so if
you stack more controls than fit, the tab scrolls — you don't need to add anything.
Just build your controls and let them stack; size your `Root` to its content rather than
forcing it to fill.

If your tab manages its own scrolling internally (for example a UniverseLib `ScrollPool`
or your own `ScrollView`), opt out of the automatic wrapper so scrollers aren't nested:

```csharp
public bool ScrollableContent => false;
```

## Targeting the selected player

The player bar at the top of the window picks which player actions apply to. Read and
react to it from your tab:

```csharp
NetworkUser target = AeroltAPI.SelectedPlayer;      // may be null
AeroltAPI.SelectPlayer(someUser);                   // change the target
AeroltAPI.SelectedPlayerChanged += () => { /* re-sync your UI */ };
```

## Menu visibility

```csharp
bool open = AeroltAPI.MenuOpen;
AeroltAPI.ToggleMenu();
AeroltAPI.SetMenuVisible(true);
```

## Knowing when the UI exists

```csharp
if (AeroltAPI.IsReady) { /* window exists now */ }

AeroltAPI.Ready += () => { /* fires when the UI is created; immediate if already ready */ };
```

## Advanced: your own window

For a full separate window rather than a tab, create your own UniverseLib `PanelBase`
against Aerolt's shared canvas so it inherits the same cursor handling:

```csharp
AeroltAPI.Ready += () =>
{
    var uiBase = AeroltAPI.SharedUIBase;   // UniverseLib.UI.UIBase
    var myPanel = new MyPanel(uiBase);     // your PanelBase subclass
};
```

Prefer `RegisterSection` for simple extensions — it's less code and stays consistent
with the rest of the menu.
