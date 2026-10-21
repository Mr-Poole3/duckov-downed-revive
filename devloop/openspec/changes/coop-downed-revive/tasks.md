## 1. Bootstrap and Compatibility Tracer

- [x] 1.1 Create the pure session project, Mod adapter project, test project, local
  game-reference build configuration, and build/test scripts.
- [x] 1.2 Add the Mod entry point, protocol version, capability handshake,
  connected-peer tracking, and global compatibility fallback.
- [x] 1.3 Add a read-only diagnostic surface that reports session mode, protocol
  version, peer compatibility, and active raid identity.
- [x] 1.4 Add tests for matching, missing, mismatched, late-joining, and
  disconnected peers, then verify the Mod package loads without touching game
  files.

## 2. Raid Configuration and Opportunity Ledger

- [x] 2.1 Associate co-op player identities with spawned characters and the
  active raid epoch.
- [x] 2.2 Implement the host-locked configuration snapshot, validation, control
  panel surface, and next-raid staging behavior.
- [x] 2.3 Implement the per-player opportunity ledger, consumption, and
  base-to-map reset while preserving internal transitions and reconnects.
- [x] 2.4 Add simulation tests for limits, reset boundaries, duplicate messages,
  invalid configuration, and client attempts to override host values.

## 3. End-to-End Downed Timeout

- [ ] 3.1 Add the host-side lethal combat damage classifier and an instrumented
  compatibility spike that proves which health representation owns the final
  damage event.
- [ ] 3.2 Implement atomic down entry, opportunity consumption, freeze, camera
  lock, vehicle dismount, action suppression, AI target exclusion, and inventory
  preservation.
- [ ] 3.3 Synchronize the authoritative down snapshot, countdown, and terminal
  timeout; re-enter the native death path exactly once.
- [ ] 3.4 Add the white countdown bar, rounded seconds, and team-only through-wall
  marker within 30 meters.
- [ ] 3.5 Add policy, network-contract, and integration tests for a lethal bullet
  becoming a downed player, expiring, and producing one native death/loot event.

## 4. End-to-End Rescue and Revival

- [ ] 4.1 Add a native interaction-based rescue target with prompt, 2.5-meter
  range, and three-second hold.
- [ ] 4.2 Implement host-authoritative rescue ownership, progress, timer pause,
  timer resume, and separate yellow progress presentation.
- [ ] 4.3 Implement revival health, immediate control and vulnerability restore,
  and preservation of the consumed down opportunity.
- [ ] 4.4 Add tests for successful rescue, host/client symmetry, and exact 10%
  revive health with a one-point minimum.

## 5. Rescue Contention and Cancellation

- [ ] 5.1 Enforce one owner per target and one target per rescuer, including
  locked-target feedback and switching targets.
- [ ] 5.2 Implement movement and range cancellation, reset of only yellow
  progress, camera-rotation allowance, collision tolerance, and non-lethal
  damage tolerance.
- [ ] 5.3 Interrupt rescue on rescuer death, downed transition, or disconnect.
- [ ] 5.4 Add contract and integration tests for competing rescuers, every
  interruption path, countdown resume values, and stale progress messages.

## 6. Finalization and Teammate Availability

- [ ] 6.1 Add the central idempotent finalization command and terminal down-ID
  ledger.
- [ ] 6.2 Finalize immediately when no teammate exists, the last possible
  rescuer is lost, the downed player disconnects, or raid settlement begins.
- [ ] 6.3 Verify final death uses the native inventory, corpse, loot, and
  extraction rules exactly once.
- [ ] 6.4 Add tests for each terminal source, duplicate delivery, same-raid
  reconnect, and downed extraction.

## 7. Damage Eligibility and Unsafe Contexts

- [ ] 7.1 Complete combat-source classification for bullets, melee, explosions,
  traps, fire, and bleeding.
- [ ] 7.2 Keep starvation, thirst, oxygen loss, cold, poison, scripted death,
  deep water, and off-scene death on the vanilla path.
- [ ] 7.3 Implement configurable downed invulnerability and the subsequent
  lethal-hit rule when vulnerability is enabled.
- [ ] 7.4 Add table-driven tests for every eligible source, bypass source, unsafe
  vehicle case, unsafe scene case, and invulnerability mode.

## 8. Release Hardening

- [ ] 8.1 Run a three-participant in-game verification covering timeout, rescue,
  host down, client down, disconnect, mismatch, and settlement.
- [ ] 8.2 Package the standalone Mod with `info.ini`, versioned protocol
  metadata, user configuration guidance, and rollback instructions.
- [ ] 8.3 Verify no game or co-op Mod files are modified, all tests and builds
  pass, and the installation package contains only the new Mod artifacts.
