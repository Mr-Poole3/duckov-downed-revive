# Architecture Map

## Overview

`duckov-downed-revive` is a planned additive Unity Mod for Escape from Duckov.
It introduces a host-authoritative downed and rescue state without modifying
the core co-op mod.

The repository currently contains product and workflow documentation only.
Source modules below describe the intended boundaries and will be confirmed as
the first implementation slices land.

## Planned Modules

### `session`

- **Path:** planned `src/DuckovDownedRevive/Session/`
- **Responsibility:** Deterministic raid lifecycle, down limits, timer,
  eligibility, rescue ownership, and compatibility fallback policy.
- **Key Interfaces:**
  - Host-side authoritative down session
  - Damage eligibility decision
  - Rescue lock and progression transitions
- **Dependencies:** none at runtime; pure domain layer
- **Has Spec:** No

### `network`

- **Path:** planned `src/DuckovDownedRevive/Network/`
- **Responsibility:** Versioned capability handshake, host snapshots, client
  intents, and co-op Mod network transport adaptation.
- **Key Interfaces:**
  - Protocol envelope and message codecs
  - Host/client bridge
- **Dependencies:** session, co-op Mod API
- **Has Spec:** No

### `gameplay`

- **Path:** planned `src/DuckovDownedRevive/Gameplay/`
- **Responsibility:** Unity-facing damage interception, player freeze, vehicle
  dismount, interaction, AI target exclusion, revival, and native death bridge.
- **Key Interfaces:**
  - Combat damage adapter
  - Downed player controller
  - Native interaction adapter
- **Dependencies:** session, Unity, game assemblies, co-op Mod API
- **Has Spec:** No

### `presentation`

- **Path:** planned `src/DuckovDownedRevive/Presentation/`
- **Responsibility:** Down timer bar, rescue bar, team-only through-wall marker,
  and localized interaction feedback.
- **Key Interfaces:**
  - Down view model
  - Marker and HUD adapters
- **Dependencies:** session, Unity UI, game HUD
- **Has Spec:** No

### `bootstrap`

- **Path:** planned `src/DuckovDownedRevive/Bootstrap/`
- **Responsibility:** Harmony entry point, dependency checks, lifecycle hooks,
  configuration, logging, and module composition.
- **Key Interfaces:**
  - Mod startup and shutdown
  - Co-op Mod availability probe
  - Host configuration loader
- **Dependencies:** all modules
- **Has Spec:** No

## Dependency Graph

```text
bootstrap -> gameplay, network, presentation, session
gameplay -> session, game assemblies, co-op Mod API
network -> session, co-op Mod API
presentation -> session, Unity UI
session -> standard library only
```

## Current Entry Points

None. The source project and Mod entry point will be created by the first
implementation slice.

## Notes

- The pure session module is the primary deterministic test seam.
- Unity-facing adapters must remain replaceable so host/client contract tests do
  not depend on a live scene.
- The co-op Mod is a binary dependency and must not be modified or repackaged.
- The reported game runtime is Unity `2022.3.62f2` with Mono.
