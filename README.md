# Duckov Downed Revive

An additive co-op Mod for Escape from Duckov that adds a host-authoritative
downed state and teammate rescue.

The feature is under active development. The current alpha implements the build,
Mod bootstrap, versioned compatibility handshake, connected-peer tracking,
global fallback state, host-locked raid configuration, a per-player down
opportunity ledger, an `F8` compatibility overlay, an `F9` host rules panel, a
guarded host down transition, a host-authoritative countdown, remote down-state
sync, and teammate down markers. Teammate rescue and revival are still under
development.

## Compatibility

- Game runtime: Unity 2022.3.62f2 Mono
- Co-op Mod baseline: EscapeFromDuckovCoopMod 1.3.6
- Mod protocol: 2

The feature disables itself when connected participants are missing the Mod or
use an incompatible protocol.

## Build

The repository uses the .NET Framework compiler installed with Windows so the
project can build without an SDK:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/build.ps1 -WarningsAsErrors
```

Artifacts are written to `artifacts/package`.

Set `DUCKOV_GAME_DIR` if the game is installed somewhere other than
`D:\Games\Escape from Duckov`.

## Test

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/test.ps1 -WarningsAsErrors
```

## Current Install

Copy the contents of `artifacts/package` into a new folder inside the game's
`Duckov_Data\Mods` directory, then enable the Mod through the game's Mod menu.
Do not overwrite the existing co-op Mod.

## Current Diagnostics

Press `F8` in game to show or hide the compatibility report. A compatible
two-or-more-player session reports one entry per connected peer and protocol
version 1.

The current diagnostic build logs potentially lethal main-character damage with
the `[DuckovDownedRevive][Probe]` prefix. The probe writes to
`DuckovDownedReviveProbe.log` under Unity's persistent data directory and also
shows the latest events in the `F8` overlay.

## Design And Specifications

- Product requirements: `docs/PRD-downed-revive.md`
- Current OpenSpec change: `devloop/openspec/changes/coop-downed-revive/`
- Technical evidence: `devloop/research/coop-downed-revive-technical-evidence.md`
