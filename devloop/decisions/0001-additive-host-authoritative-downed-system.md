# ADR-0001: Additive Host-Authoritative Downed System

> Architecture Decision Record
> Status: Accepted
> Date: 2026-10-21

## Context

Escape from Duckov uses immediate vanilla death for lethal damage. The existing
co-op Mod already synchronizes player health, damage, death, scene, and peer
state. Rebuilding or modifying that Mod would couple the feature to a third-party
release and create a second source of truth for player state.

The observed co-op Mod exposes managed network handlers and peer lifecycle
events, and its health manager already follows a host-authoritative damage flow.
The game exposes `Health.Hurt`, `RaidUtilities` lifecycle events, camera freeze,
input suppression, and native interactables.

## Decision

Build a separate additive Mod. Its pure session policy runs only when the host
has an active compatible co-op session. The host owns the authoritative state
for down eligibility, timer, rescue ownership, revival, fallback, and final
death. Clients send intent and render host snapshots; they never complete a
rescue or finalize a down independently.

Use a versioned protocol exchange through the co-op Mod network API. If any
participant lacks the protocol or reports an incompatible version, disable the
feature globally for that session and keep vanilla immediate death.

Use Harmony and game-facing adapters only at the boundaries. Keep the session
state machine independent of Unity so it can be driven deterministically by
tests. Final outcomes re-enter the game's native death and loot path exactly
once.

## Consequences

- The feature can ship and update independently of the core co-op Mod.
- Host state remains coherent across all players.
- Compatibility failures degrade safely to vanilla behavior.
- The Mod needs a protocol version, capability discovery, and explicit fallback
  diagnostics.
- The implementation must bridge both the game's native health path and the
  co-op Mod's host health synchronization without double-applying death.
- In-game verification is mandatory because static metadata cannot prove patch
  ordering in every co-op scenario.

## Alternatives Considered

- **Fork the core co-op Mod:** Rejected because it creates a competing Mod
  distribution and makes updates and compatibility harder.
- **Client-authoritative down state:** Rejected because clients can disagree on
  eligibility, timer, rescue locks, and final death.
- **Best-effort operation with a mismatched peer:** Rejected because ambiguous
  health and death state can duplicate loot or revive an invalid player.
- **Custom loot and death path:** Rejected because it duplicates native
  inventory and raid settlement behavior.
- **Prone or ragdoll animation in V1:** Rejected to keep the first version
  focused on the gameplay state machine and network contract.

## References

- `docs/PRD-downed-revive.md`
- `devloop/research/coop-downed-revive-technical-evidence.md`
