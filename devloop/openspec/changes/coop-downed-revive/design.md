## Context

Escape from Duckov is a Unity `2022.3.62f2` Mono game. The installed co-op Mod
`1.3.6` already owns player spawning, host-authoritative health, damage requests,
death replication, scene loading, and peer lifecycle. The game exposes native
health, death, camera, input, interaction, AI, and raid APIs.

The feature is cross-module and high risk because it changes death behavior,
coordinates multiple peers, and must remain safe when versions differ. Static
evidence for the relevant APIs is recorded in
`devloop/research/coop-downed-revive-technical-evidence.md`.

## Goals / Non-Goals

**Goals:**

- Deliver a standalone Mod that composes with the existing co-op Mod.
- Convert eligible lethal combat damage into a frozen, host-authoritative downed
  state.
- Support exclusive three-second teammate rescue, countdown pause/resume, and
  configurable revival health.
- Present a white down countdown, yellow rescue progress, and team-only marker.
- Reuse native final death, loot, and inventory behavior exactly once.
- Fail closed to vanilla death when compatibility is uncertain.

**Non-Goals:**

- Modify, recompile, fork, or redistribute the core co-op Mod.
- Add prone, crawling, ragdoll, or custom injury animation.
- Add self-revive, enemy revive, rescue items, skills, cooldowns, or costs.
- Add rescue line-of-sight checks or multi-rescuer acceleration.
- Preserve rescue progress across interruption.
- Replace vanilla loot, extraction, or raid settlement.

## Decisions

### Additive Mod Composition

Build `DuckovDownedRevive.dll` as a C# class library targeting Unity's .NET
Standard profile. The Mod entry point derives from the installed
`HarmonyLoadMod.ModBehaviour` and composes four internal boundaries:

- a pure host session policy;
- a co-op network and lifecycle adapter;
- game-facing health/input/interaction adapters;
- presentation adapters.

Game and co-op assemblies are compile-time references only. They are never
copied into the repository or rebuilt.

### Host Authority

Only the host session policy mutates down eligibility, timers, rescue ownership,
revival, and final death. Clients submit intent and render host snapshots.
Every state transition carries a stable down ID and is idempotent.

The host's co-op-aware health path is the highest existing damage seam. The
implementation SHALL observe the final structured `DamageInfo`, make the down
decision before vanilla final death, and suppress the original lethal outcome
atomically. Clients MUST NOT infer a down from locally reaching zero health.

The downed record stores the lethal `DamageInfo` needed to re-enter the native
death path on finalization. A guarded terminal transition invokes that path
exactly once, bypassing the down interceptor without bypassing native loot or
settlement processing.

### Session Policy

Keep a pure C# session layer with injected time and stable player IDs. It owns:

- one down opportunity per stable player per raid by default;
- down record state and remaining time;
- eligible teammate availability;
- one rescue owner per target;
- one target per rescuer;
- revival and final-death decisions.

The policy does not reference Unity types or co-op Mod types. That allows fast
deterministic tests for the full state machine.

### Raid Lifecycle

Use `RaidUtilities.OnNewRaid` to start a new opportunity epoch and
`RaidUtilities.OnRaidEnd` or the base transition to end it. Associate the epoch
with the game's raid identity. Internal scene loading and same-raid reconnect
MUST preserve consumed opportunities.

### Network Protocol

Register a dedicated channel through `ModNetworkApi`. Use a protocol version
and explicit messages for:

- capability announcement;
- locked host configuration;
- raid state;
- down entry snapshot;
- rescue start/cancel/progress;
- revival;
- final death;
- global fallback.

The host sends authoritative snapshots. Client messages carry intent and a down
ID, never a terminal state. The co-op Mod's replay and queue behavior is used,
but the implementation still treats every message as potentially duplicated.

The adapter tracks `PeerConnected`, `PeerDisconnected`, and `PlayerSpawned`.
Any connected peer without a matching protocol forces global fallback. If peer
enumeration is unavailable through the public API, presence tracking starts
from those events and missing capability is treated as incompatible.

### Downed Control

Entry force-dismounts vehicles before activation. The downed adapter:

- sets movement and action inputs to inactive;
- suppresses weapon, item, and interaction paths;
- freezes the camera through `GameCamera.FreezeCamera`;
- holds the standing pose by preventing animation/action updates;
- removes AI target eligibility;
- preserves collision and inventory.

Control is restored through the inverse adapter calls after the host's revival
snapshot. No ragdoll or new animation assets are introduced.

### Rescue Interaction

Create a game-native rescue `InteractableBase` on or near the downed player. The
prompt uses the normal interaction path and input binding. Rescue requires a
three-second hold and host-owned progress. Movement or range loss resets the
client progress and sends cancellation. Camera rotation is not part of the
movement-input predicate.

The interaction adapter reports only intent. The host accumulates authoritative
progress using elapsed host time and validates range/status when accepting start
and completion.

### Downed and Rescue Presentation

Derive presentation from a read-only view model generated from host state:

- white down bar using `ceiling(remainingSeconds)`;
- separate yellow rescue bar;
- team-only through-wall marker within 30 meters;
- marker remaining seconds;
- locked-target feedback.

The first implementation may use existing HUD primitives or a small dedicated
UGUI overlay. Rendered text and localization are adapter concerns, not session
policy concerns.

### Configuration

The host owns an immutable per-raid snapshot with:

- maximum opportunities: default 1;
- down duration: default 30 seconds;
- downed invulnerability: default true;
- revive health percentage: default 10%.

V1 fixes rescue duration at three seconds, rescue range at 2.5 meters, and
marker range at 30 meters. Settings changed during a raid are staged for the
next raid.

### Failure and Finalization

Disconnects, missing rescuers, timeout, raid settlement, and compatibility
fallback all resolve through one finalization command. The command is
idempotent and records a terminal down ID before invoking native death.

If the feature cannot prove compatibility or completion, it fails toward vanilla
death. It never leaves a player in an unkillable or unrescuable state.

## Risks / Trade-offs

- **Co-op patch ordering is uncertain** -> Start with a diagnostic slice that
  identifies whether host or client health representations receive the lethal
  event, then anchor the final patch at the host-authoritative path.
- **Client can reach local zero before the host responds** -> Keep a guarded
  client lethal path pending during host validation and resolve deterministically
  to either the host down snapshot or native death.
- **Native death can run twice** -> Use a terminal guard plus a down ID ledger;
  integration tests assert one native death/loot invocation.
- **Input disabling can break camera rotation or revive interaction** ->
  Suppress gameplay input selectively, not the entire input manager.
- **A rescue interactable can conflict with loot or world interaction** ->
  Give it exclusive interaction priority only while the down record is active.
- **Unity objects can be recreated across scene loads** -> Key player and down
  state by stable co-op identity, not Unity instance ID.
- **Private co-op internals may be required** -> Prefer public APIs. Any private
  Harmony target must be isolated in an adapter and covered by explicit version
  compatibility checks.
- **Network queue drops/replays** -> Make all transitions idempotent and use
  periodic host snapshots as the recovery source.

## Migration Plan

1. Build and test the standalone package without changing either game install.
2. Add a diagnostic release that can be enabled without affecting death.
3. Validate compatibility and host/client patch ordering in a private raid.
4. Enable the feature for compatible sessions only.
5. Roll back by removing or disabling the standalone Mod; the game and co-op
  Mod files remain unchanged.

## Open Questions

1. Which exact co-op health method sees the final host-side lethal event before
   `Health.Hurt` mutates `isDead`, and how is the same event represented on the
   originating client?
2. Which existing game HUD and world-to-screen marker primitives are safest for
   the white/yellow bars and through-wall team marker without adding a prefab
   asset bundle?

These questions are resolved by the first diagnostic implementation slice before
the full down transition is enabled.
