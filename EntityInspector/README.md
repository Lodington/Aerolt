# Aerolt - Entity Inspector

An add-on for Aerolt that adds an **Inspector** tab: a live readout of a character body's
stats, inventory, and buffs — a focused runtime inspector so you can watch values change
instead of relying on debug logs.

## Requires

- Aerolt

## Usage

1. Open the **Inspector** tab.
2. Pick a target from the dropdown (players are tagged `[P]`), or leave it on
   **Selected player** to follow Aerolt's current selection.
3. Use **Refresh list** if bodies have spawned/despawned since you opened it.

## What it shows

- Health / shield, level, regen
- Move speed, damage, attack speed, crit chance and multiplier, armor
- Inventory summary (unique items, total stacks, current equipment)
- Active buffs with stack counts

Everything updates live while the tab is open.
