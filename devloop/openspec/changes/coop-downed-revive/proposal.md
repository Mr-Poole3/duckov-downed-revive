## Why

Escape from Duckov treats lethal combat damage as immediate death. In co-op,
that removes the rescue gameplay expected when a nearby teammate can save a
fallen player. The feature must be delivered as an additive Mod because the
existing co-op Mod already owns health, player, scene, and network state.

## What Changes

- Add a host-authoritative downed state for eligible lethal combat damage.
- Consume one configurable down opportunity per player per raid on entry.
- Freeze downed players, keep the first version standing, lock their camera,
  stop AI targeting, and expose a 30-second host-authoritative countdown.
- Add team-only through-wall markers, a white countdown bar, and a separate
  yellow rescue progress bar.
- Allow one living, non-downed teammate within 2.5 meters to hold the native
  interaction input for three seconds to revive the target.
- Pause and resume the down countdown around a valid rescue without resetting
  its remaining time. Reset rescue progress on interruption.
- Restore control and set health to a configurable percentage of maximum health
  on revival, with no post-revive invulnerability.
- Finalize timeout, no-rescuer, last-rescuer-loss, disconnect, and raid
  settlement through the native death and loot path.
- Add a versioned protocol handshake and fail-closed session fallback when any
  participant is missing the Mod or uses an incompatible protocol.
- Lock host configuration for the active raid and apply changes on the next
  base-to-map transition.
- Deliver the feature as a separate Mod without modifying or rebuilding the
  existing co-op Mod.

## Capabilities

### New Capabilities

- `downed-state`: Eligible damage entry, incapacitation, countdown, and final
  death behavior.
- `teammate-rescue`: Native interaction, rescue lock, interruption, and revival.
- `downed-presentation`: Countdown, rescue, and team marker presentation.
- `session-compatibility`: Host authority, version handshake, configuration,
  and fallback.
- `raid-lifecycle`: Per-raid opportunity limits and reset boundaries.

### Modified Capabilities

None. The core co-op Mod's public behavior is extended by composition, not
modified.

## Impact

- Adds a standalone C# Unity Mod assembly and release package.
- Integrates with the game's health, death, player, camera, input, interaction,
  AI, scene, and raid lifecycle APIs.
- Integrates with the co-op Mod's public event and network APIs plus the
  existing host-authoritative health path.
- Adds a versioned Mod network protocol and explicit compatibility fallback.
- Requires focused unit tests, host/client contract tests, and manual in-game
  verification with at least two players.
- Rollback is achieved by disabling or removing the standalone Mod; the core
  co-op Mod and game files remain unchanged.
