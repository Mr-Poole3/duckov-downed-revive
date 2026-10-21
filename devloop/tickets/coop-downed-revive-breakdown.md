# Proposed Issue Breakdown

Parent PRD: `#1`

## 1. Bootstrap and Compatibility Tracer

- **Type:** AFK
- **Blocked by:** None
- **User stories:** 57-59
- **Vertical result:** The Mod builds and loads, exchanges a versioned
  capability handshake, reports compatible peers, and falls back safely when a
  peer is missing or incompatible.

## 2. Raid Configuration and Opportunity Ledger

- **Type:** AFK
- **Blocked by:** 1
- **User stories:** 2-4, 53-56, 60-61
- **Vertical result:** A host can configure and lock the rules for the next raid,
  every player receives an independent opportunity ledger, and raid boundaries
  reset it only when appropriate.

## 3. End-to-End Downed Timeout

- **Type:** AFK
- **Blocked by:** 2
- **User stories:** 1, 5-18, 46-48
- **Vertical result:** An eligible lethal bullet causes a frozen downed state,
  the white countdown and team marker render, timeout triggers one native death,
  and the opportunity is consumed.

## 4. End-to-End Rescue and Revival

- **Type:** AFK
- **Blocked by:** 3
- **User stories:** 19-31, 35-40, 62
- **Vertical result:** A teammate uses the native interaction prompt, pauses the
  countdown, completes a three-second rescue, and restores control at 10%
  health through the host-authoritative flow.

## 5. Rescue Contention and Cancellation

- **Type:** AFK
- **Blocked by:** 4
- **User stories:** 22-37
- **Vertical result:** Competing rescuers, movement, range loss, damage, death,
  downing, disconnects, and target switching follow the exact ownership and
  reset rules.

## 6. Finalization and Teammate Availability

- **Type:** AFK
- **Blocked by:** 3
- **User stories:** 41-46, 50-52, 63
- **Vertical result:** No-teammate, last-rescuer-loss, disconnect, settlement,
  and duplicate-message cases finalize through the native death and loot path
  exactly once.

## 7. Damage Eligibility and Unsafe Contexts

- **Type:** AFK
- **Blocked by:** 3
- **User stories:** 11-13, 47-52
- **Vertical result:** Combat and bypass damage sources, unsafe vehicle
  dismounts, deep water, off-scene death, and both invulnerability modes follow
  the specified transition table.

## 8. Release Hardening and In-Game Verification

- **Type:** HITL
- **Blocked by:** 4, 5, 6, 7
- **User stories:** 53-64
- **Vertical result:** Package metadata, rollback guidance, multi-participant
  verification, host/client symmetry, and final release artifacts are complete.
