# Project Context

## Project Purpose

Add a reusable co-op downed and revive system to Escape from Duckov without
modifying the core co-op Mod. The feature lets an eligible player survive one
lethal combat hit, wait for a nearby teammate to rescue them, and resume the
same raid after a successful revive.

## Core Domain Terms

| Term | Definition |
|------|------------|
| Raid | A player's active expedition from the base into a gameplay map until return to base or run settlement. |
| Base | The out-of-raid hub where configuration changes may be staged for the next raid. |
| Down Opportunity | One player's remaining permission to enter downed state during the current raid. |
| Downed State | Incapacitated gameplay state in which the player is frozen and can be rescued. |
| Down Record | Host-owned identity and state for one specific downed transition. |
| Rescuer | A living, non-downed player teammate responsible for an active rescue. |
| Rescue Lock | Exclusive host-granted ownership that prevents multiple rescuers from stacking progress. |
| Rescue Progress | Independently resettable progress toward revival, shown as a yellow bar. |
| Revive | Successful completion of rescue that restores control and health. |
| Final Death | Irreversible end of the player's current run through the game's native death path. |
| Compatibility Fallback | Session-wide safe mode used when Mod support or protocol versions do not match. |
| Native Death Path | The game's original death, corpse, inventory, and loot processing. |
| Host Authority | The rule that only the host decides eligibility, timing, rescue ownership, and final state. |

## Module Glossary

| Module | Business Role |
|--------|---------------|
| session | Owns deterministic raid policy, down records, timer, and rescue rules. |
| network | Synchronizes capabilities, configuration, host snapshots, and client intents. |
| gameplay | Adapts Unity and game systems for damage, freezing, interaction, AI, and death. |
| presentation | Renders countdown, rescue, marker, and feedback state. |
| bootstrap | Starts the Mod, checks dependencies, and composes the other modules. |

## Architecture Decisions

- The feature is an additive Mod and does not recompile or modify the core
  co-op Mod.
- The host is authoritative for all down and rescue state.
- The session policy remains independent of Unity so it can be tested
  deterministically.
- Compatibility failure disables the feature globally for the session instead
  of attempting partial operation.
- Final outcome delegates to the native death and loot path exactly once.

## Conventions

### Naming

- Use the domain terms in this document consistently.
- Name protocol state and tests after player-observable behavior.
- Avoid synonyms such as "bleed out" when "down countdown" is intended.

### Testing

- Test external behavior rather than private implementation details.
- Prefer deterministic host-policy tests before Unity integration tests.
- Verify native death and loot are invoked exactly once.
- Verify host and client players follow the same contract.

### API

- Prefer public game and co-op Mod extension points.
- Version the Mod protocol explicitly.
- Fail closed on ambiguous state or version mismatches.

## Out of Bounds

- Do not modify or redistribute files in either installed game directory.
- Do not modify or rebuild the core co-op Mod.
- Do not commit game assemblies, user settings, credentials, or saves.
- Do not add custom loot or bypass native death processing.
- Do not implement prone animation, self-revive, or rescue costs in V1.
