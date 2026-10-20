# Aerolt - Damage Log

An add-on for Aerolt that adds a **Damage Log** tab: a live, scrolling list of recent
server damage events for debugging damage, proc chains, and damage types.

## Requires

- Aerolt

## What it shows

Each entry: attacker, victim, damage dealt, CRIT marker, proc coefficient, damage type,
and how long ago it happened.

## Controls

- **Pause / Resume** — stop/continue capturing events (the list freezes so you can read it).
- **Clear** — empty the log.
- **Selected player only** — show only events where the Aerolt-selected player is the
  attacker or victim.

Only the most recent events are kept (capped), newest at the top. Capture is
server-side, so this is most useful on the host.
