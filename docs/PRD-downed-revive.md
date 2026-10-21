# PRD: Co-op Down

## Problem Statement

Escape from Duckov was designed as a single-player game, so lethal combat
damage immediately ends the run. In co-op, that behavior removes the team
rescue gameplay players expect from the multiplayer experience: one player can
make a mistake and be sent straight to the death and loot flow while their
teammates are still nearby and able to help.

The existing co-op mod already carries player state and provides death, player
spawn, input, interaction, and network extension points. However, changing the
core co-op mod would couple the feature to that mod's release cycle and make
the rules harder for other players to reuse. The downed/revive feature must
therefore be delivered as a separate additive mod that runs alongside the
co-op mod.

## Solution

Add a host-authoritative co-op downed state. When a player would die from
eligible combat damage and still has a configured per-raid down opportunity,
the normal death path is deferred and the player enters a frozen downed state
for a configurable countdown. Living teammates can hold the native interaction
input near the downed player to revive them.

By default, a downed player is invulnerable, cannot move or act, keeps a fixed
camera, loses AI aggro, is marked for teammates through walls, and has a white
health-style countdown. A teammate can rescue without item or skill cost by
holding the native interaction input for three seconds within 2.5 meters. An
active rescue pauses the countdown and displays a separate yellow rescue
progress bar. A completed rescue restores control immediately and restores the
player to a configurable percentage of maximum health.

Each player has one down opportunity per raid by default. The down is consumed
on entry. At the end of the countdown, when no living teammate can rescue, when
the last possible rescuer is lost, or when the downed player disconnects, the
player is finalized through the game's native death and loot rules.

The feature is enabled only when the game is in an active co-op raid and every
participant supports the same mod protocol. If the protocol or version does not
match, the system disables itself for the session and lethal damage keeps the
vanilla immediate-death behavior.

## User Stories

1. As a co-op player, I want eligible lethal combat damage to put me into a
   downed state instead of immediately killing me, so that my teammates have a
   chance to rescue me.
2. As a co-op player, I want my down opportunity to be consumed immediately
   when I enter the downed state, so that the limit cannot be bypassed by
   interruption or reconnection.
3. As a co-op player, I want each player to have an independent down
   opportunity, so that one teammate using theirs does not consume mine.
4. As a server owner, I want the maximum number of downs per player per raid to
   be configurable, so that I can tune difficulty.
5. As a downed player, I want my movement, weapons, item use, interactions, and
   other in-game actions disabled, so that being downed is a meaningful
   incapacitated state.
6. As a downed player, I want to remain standing with movement and animation
   frozen in the first version, so that the feature does not depend on a custom
   ragdoll or downed animation.
7. As a downed player, I want my camera direction fixed when I enter the downed
   state, so that I cannot continue scouting for my team.
8. As a downed player, I want my equipped items and inventory preserved, so
   that a successful rescue restores the run without an extra transfer step.
9. As a downed player, I want artificial intelligence to stop targeting me, so
   that enemies do not continue focusing an incapacitated target.
10. As a downed player, I want world collision to remain active, so that my
    body does not fall through terrain or create invalid positioning.
11. As a downed player, I want to be invulnerable while downed by default, so
    that enemies cannot immediately finish me before teammates can respond.
12. As a server owner, I want downed invulnerability to be configurable, so
    that groups can choose a harsher rule set.
13. As a downed player with invulnerability disabled, I want a second lethal
    combat hit to finalize me, so that vulnerability follows a clear rule.
14. As a downed player, I want a host-authoritative countdown, so that every
    participant sees the same remaining rescue time.
15. As a downed player, I want the countdown to default to 30 seconds, so that
    teammates have a predictable rescue window.
16. As a downed player, I want the white countdown bar to show remaining time
    rounded up to whole seconds, so that the display never implies a full
    second remains when it does not.
17. As a teammate, I want a team-only marker visible through walls within 30
    meters, so that I can locate a downed teammate without map knowledge.
18. As a teammate, I want the marker to show the remaining down time, so that I
    can make an informed rescue decision.
19. As a teammate, I want to use the native interaction prompt and input to
    rescue, so that revival feels consistent with the rest of the game.
20. As a teammate, I want to establish rescue by holding the interaction input
    for three seconds within 2.5 meters, so that rescue requires deliberate
    cooperation.
21. As a teammate, I want line of sight not to be required in the first
    version, so that rescue remains reliable around nearby cover.
22. As a teammate, I want to rotate my camera while rescuing, so that I can
    watch for threats.
23. As a rescuer, I want moving to reset my rescue progress to zero, so that
    rescue cannot be completed while repositioning.
24. As a rescuer, I want leaving the 2.5-meter range to reset progress to zero,
    so that rescue cannot be completed from a remote position.
25. As a rescuer, I want incidental collision or terrain sliding not to cancel
    rescue while I remain in range, so that small physical displacement does
    not feel unfair.
26. As a rescuer, I want non-lethal damage not to interrupt an in-range rescue,
    so that taking return fire does not automatically cancel the attempt.
27. As a rescuer, I want my own death or downed transition to cancel the
    rescue, so that incapacitated players cannot continue rescuing.
28. As a rescuer, I want rescue to require no consumable, skill, or cooldown in
    the first version, so that the core mechanic remains accessible.
29. As a rescuer, I want the downed player's countdown to pause while a valid
    rescue is in progress, so that a successful effort is not invalidated by
    the timer.
30. As a rescuer, I want an interrupted rescue to reset only the yellow rescue
    progress while the white countdown resumes from its remaining value, so
    that both progress states have clear and distinct rules.
31. As a rescuer, I want to see a separate yellow rescue progress bar, so that
    rescue progress is visually distinct from the down timer.
32. As a rescuer, I want only the first valid rescuer to own the rescue, so that
    progress cannot be stacked by multiple players.
33. As any other teammate, I want to be told the target is already being
    rescued when another player owns the interaction, so that the lock is
    understandable.
34. As a rescuer, I want to hold ownership of only one downed target at a time,
    so that rescue intent cannot be split across several players.
35. As any living, non-downed teammate, I want to be able to rescue a teammate,
    including when friendly fire caused the down, so that eligibility is simple
    and consistent.
36. As a downed player, I want to be unable to self-revive, so that the feature
    requires cooperative play.
37. As a player, I want enemies to be unable to revive downed players, so that
    rescue remains a player-team mechanic.
38. As a revived player, I want to receive a configurable minimum of 10% of
    maximum health, so that I return to play but remain vulnerable.
39. As a revived player, I want full control and damage vulnerability restored
    immediately after the broadcast completes, so that there is no unexplained
    grace period.
40. As a revived player, I want no post-revive invulnerability, so that combat
    rules remain predictable.
41. As a downed player, I want to receive an immediate vanilla death if no
    living, non-downed teammate exists when I am downed, so that the run does
    not wait for an impossible rescue.
42. As a downed player, I want to receive an immediate vanilla death if the
    last possible rescuer dies, becomes downed, or disconnects, so that rescue
    availability stays truthful.
43. As a downed player, I want my disconnect to immediately finalize my death
    and loot state, so that reconnecting cannot preserve or duplicate a down.
44. As a downed player, I want to be unable to extract, so that being downed
    cannot bypass the raid outcome.
45. As a co-op team, I want any player still downed at raid settlement to be
    finalized as dead, so that extraction cannot rescue an incapacitated player.
46. As a player, I want final death to use the game's native death, inventory,
    and loot processing, so that the mod does not create a second item economy.
47. As a player, I want only combat-related lethal damage to trigger downed
    state, so that non-combat survival failures keep their vanilla meaning.
48. As a player, I want bullets, melee, explosions, traps, fire, and bleeding to
    qualify as combat damage, so that common combat sources share one rule.
49. As a player, I want starvation, thirst, oxygen loss, cold, poison, and
    scripted death to bypass downed state, so that environmental and
    narrative outcomes remain authoritative.
50. As a player, I want to be force-dismounted before entering downed state if
    I am in a vehicle, so that vehicle occupancy is not duplicated.
51. As a player, I want an unsafe forced dismount to use vanilla death, so that
    the mod does not place my body in an invalid state.
52. As a player, I want a lethal event in deep water or outside the playable
    scene to use vanilla death, so that rescue cannot create unreachable or
    invalid downed states.
53. As a player, I want the host to use exactly the same down, timer, rescue,
    and final-death rules as every client, so that host status grants no hidden
    mechanic advantage.
54. As a player, I want the host's settings to be authoritative, so that each
    client cannot choose a different difficulty.
55. As a player, I want gameplay settings to lock for the current raid, so that
    all participants play under the same rules until they return to base.
56. As a server owner, I want changed gameplay settings to apply on the next
    raid, so that configuration changes cannot alter an active encounter.
57. As a player, I want every participant to use the same mod and protocol
    version, so that incompatible state cannot be interpreted silently.
58. As a player, I want the downed system globally disabled whenever a
    participant is missing the mod or has a mismatched protocol, so that the
    session safely falls back to vanilla death.
59. As a single-player user, I want vanilla immediate death, so that the mod
    has no effect outside co-op.
60. As a player, I want the downed system to reset only when a new raid starts
    from base, so that internal transitions and same-raid reconnects do not
    restore opportunities.
61. As a player, I want returning to base to end the raid, so that the next
    base-to-map transition starts with fresh per-player limits.
62. As a player, I want a rescued teammate to resume the same raid, so that a
    successful rescue preserves the current session and inventory state.
63. As a player, I want a failed rescue to use the full vanilla death and loot
    rules, so that no equipment is duplicated, hidden, or deleted incorrectly.
64. As a user of the modded game, I want this delivered as a separate additive
    mod, so that I do not need to modify or recompile the core co-op mod.

## Implementation Decisions

### Product and Session Rules

- Deliver the feature as a separate additive mod for the existing co-op mod.
  Do not modify, recompile, or redistribute core co-op mod internals as part of
  this feature.
- Enable the feature only during an active co-op raid. In single-player or when
  the co-op session is inactive, use vanilla death behavior.
- Use a versioned capability handshake. Every participant must advertise a
  compatible downed/revive protocol version and required capabilities.
- If any participant is missing the mod, advertises no protocol, or advertises
  an incompatible version, disable the downed system globally for the current
  co-op session. Affected lethal damage uses vanilla death.
- If a mismatched participant joins after the downed system has activated,
  immediately enter global fallback mode for the session. Resolve any player
  currently downed through the vanilla death path.
- Treat the host as authoritative for eligibility, configuration, down state,
  remaining time, rescue ownership, rescue progress, and final death.
- Clients send intent and input transitions to the host; the host broadcasts
  accepted state transitions and periodic snapshots. Clients render from
  authoritative state and must not complete rescue locally.
- Apply identical state transitions for host and client players.

### Raid Lifecycle and Limits

- A new raid begins on the base-to-raid-map transition. Reset each player's
  down opportunity there.
- Internal scene transitions, same-raid reconnects, and mission sub-scenes do
  not reset per-player down opportunities.
- Returning to base ends the raid.
- Default maximum downs per player is one. The host may configure a
  non-negative limit before the raid.
- Consume a down opportunity when the player enters the downed state.
- Track limits by stable player identity for the raid, not by temporary scene
  object identity.
- A successful rescue does not restore a consumed down opportunity.

### Damage and Entry Rules

- Intercept the co-op-aware lethal damage path before vanilla final death is
  processed.
- Only lethal combat damage is eligible:
  - bullets;
  - melee;
  - explosions;
  - traps;
  - fire;
  - bleeding.
- Environmental, survival, and scripted death remains vanilla:
  - starvation;
  - thirst;
  - oxygen loss;
  - cold;
  - poison;
  - scripted or narrative death.
- A vehicle occupant must be force-dismounted before entry. If a safe forced
  dismount cannot be completed, use vanilla death.
- Deep-water lethal events and lethal events outside the playable scene use
  vanilla death.
- If no living, non-downed teammate exists at entry time, use vanilla death
  immediately.
- An eligible entry transition must atomically consume the opportunity,
  suppress vanilla death, freeze the player, and create the authoritative down
  record.

### Downed State and Presentation

- First version keeps the character standing.
- Freeze locomotion and animation. Disable input, weapons, item use,
  interactions, extraction, and all other in-game actions.
- Force dismount before the downed state is broadcast as active.
- Freeze camera movement and direction at the entry view.
- Preserve inventory, equipped items, quest state, and other persistent player
  state.
- Stop AI from selecting the downed player as a target while leaving world
  collision enabled.
- Keep the player invulnerable by default. Make this setting host-configurable.
- With invulnerability disabled, a subsequent lethal combat hit finalizes the
  player immediately. Non-damaging collisions do not finalize them.
- Use a white health-style bar to display down countdown time. Display the
  remaining time in whole seconds using ceiling.
- Show team-only through-wall markers within 30 meters. Include the downed
  player's remaining time so teammates can judge rescue feasibility.
- An active rescue pauses the white countdown. Display rescue progress as a
  separate yellow bar.
- A rescue interruption resets rescue progress to zero but does not add time or
  reset the down countdown. The countdown resumes from its authoritative
  remaining value.

### Rescue and Revival Rules

- Only a living, non-downed player teammate can own or advance a rescue.
  Enemies, the downed player, and disconnected players are not valid rescuers.
- Friendly fire does not affect rescue eligibility. The player who caused the
  down may rescue the same teammate if they remain alive and non-downed.
- Use the game's native interaction prompt and input binding.
- Require the rescuer to remain within 2.5 meters of the target. Do not require
  line of sight in the first version.
- The default rescue hold duration is three seconds.
- Only one rescuer may own a target. The first valid rescuer to start obtains
  the lock; additional players cannot stack progress.
- Other eligible players who interact with a locked target receive the native
  interaction feedback "Rescue in progress."
- A rescuer may maintain ownership of only one target at a time. Starting a
  rescue on another target interrupts and resets the previous rescue.
- Allow camera rotation during rescue.
- Movement input interrupts rescue and resets its progress to zero.
- Leaving the 2.5-meter range interrupts rescue and resets its progress to
  zero.
- External collision, knockback, or terrain sliding does not interrupt rescue
  while the rescuer remains in range and continues holding the interaction
  input.
- Non-lethal damage does not interrupt rescue. The rescuer's death, downed
  transition, knockout, or disconnect interrupts rescue.
- Pause the target's down countdown only while a valid host-confirmed rescue
  owns the target.
- If rescue is interrupted, reset only rescue progress. The down countdown
  resumes from its paused remaining value.
- On completion, restore full control and vulnerability immediately and set
  current health to the configured percentage of maximum health.
- The default revived health percentage is 10% of maximum health, rounded up
  with a minimum result of one health point. The host can configure this value.
- Successful revival does not grant temporary invulnerability.
- Successful revival preserves the same raid, position, inventory, equipment,
  and consumed down opportunity.

### Final Death and Disconnect Rules

- Final death always uses the game's native death, corpse, inventory, and loot
  processing. Do not create a mod-owned loot path.
- Finalize a downed player as dead when the authoritative countdown expires.
- Finalize a downed player as dead immediately when there are no living,
  non-downed player teammates.
- When the last possible rescuer dies, becomes downed, disconnects, leaves the
  session, or otherwise becomes invalid, immediately finalize every player
  whose only remaining rescue option was that teammate.
- Finalize a downed player immediately on disconnect. Do not retain the downed
  record for reconnection, and do not let reconnecting restore a consumed
  opportunity in the same raid.
- A downed player cannot start extraction or count as successfully extracted.
  If raid settlement begins while the player is still downed, finalize them
  through the native death path before settlement resolves.
- If global fallback mode activates for compatibility or session errors,
  finalize all current downed players through the native death path.

### Configuration and Authority

- The host owns the authoritative configuration snapshot for the raid.
- The minimum configuration surface includes:
  - maximum down opportunities per player per raid, default 1;
  - downed countdown duration, default 30 seconds;
  - downed invulnerability, default enabled;
  - revived health percentage, default 10%.
- Treat V1 rescue hold duration as three seconds, rescue radius as 2.5 meters,
  and teammate marker radius as 30 meters.
- Lock the configuration when the raid becomes active. Changes made while in
  the raid apply only on the next base-to-map transition.
- Broadcast the locked configuration and its revision to every participant.
  Clients use the host snapshot for presentation and behavior.
- Reject or clamp invalid settings on the host, including negative down counts,
  non-positive countdown duration, invalid health percentages, and values that
  would produce less than one revived health point.

### Network State and Failure Handling

- Define a versioned network protocol with explicit messages for:
  - capability and version handshake;
  - configuration snapshot and revision;
  - raid activation and reset;
  - down entry and rejection;
  - rescue start, cancel, progress, ownership, and completion;
  - periodic authoritative down snapshots;
  - final death and session-wide fallback.
- Make state transitions idempotent. Duplicate messages and retransmission must
  not consume extra opportunities, add time, duplicate loot, or complete a
  rescue twice.
- Include a stable down instance identifier so late messages cannot affect a
  newer down in the same raid.
- The host alone decides whether a damage event causes a down, whether a
  rescuer may start, and whether rescue completes.
- Clients send input transitions and intent, not unilateral final state.
- If a client cannot decode host state or the host cannot decode a client's
  capability, enter global fallback mode and warn participants that downed
  revive is disabled for the session.
- Network failure must fail closed: an uncertain rescue does not complete, and
  an uncertain final death uses the vanilla death outcome.

### Extension Points

- Use the co-op mod's public death interception, player state, interaction, and
  network APIs before considering reflection into private internals.
- Reuse the game's native camera freeze behavior to lock the downed player's
  view.
- Reuse the game's native interaction input, prompt, and held-interaction
  patterns so controller and keyboard bindings remain consistent.
- Integrate AI targeting through the game's existing targeting or aggro
  extension point rather than directly disabling enemy behaviors.
- Treat the co-op-aware death path as the highest existing integration seam.
  Do not replace or bypass the core co-op mod's player-state synchronization.
- Keep downed/revive logic independent of any specific map, enemy, weapon, or
  loot table.

### Authoritative State Model

- Session mode is one of inactive, handshake pending, active, or fallback.
- Each raider is one of alive, downed, revived, finalized dead, or disconnected
  for the purposes of this feature.
- A down record is one of countdown running, rescue active, revival committed,
  or final death committed.
- Eligible lethal combat damage transitions alive to downed only when the
  opportunity limit, teammate availability, scene, and compatibility checks all
  pass.
- A down record transitions from countdown running to rescue active when the
  host grants the first valid rescue lock.
- A rescue lock transitions back to countdown running when any required rescue
  condition fails.
- Active rescue transitions to revival committed only after the authoritative
  host has accumulated the full three-second hold and the rescuer is still
  valid.
- Countdown running, vulnerability hit, rescue unavailability, disconnect, or
  raid settlement may transition a down record to final death committed.
- Revival committed and final death committed are terminal for that down
  record. A later state must create a new down record with a new identifier.

## Testing Decisions

Good tests assert externally observable session behavior: which state a player
enters, what teammates can do, what the host broadcasts, and whether the game's
native death and loot path runs exactly once. Tests should not lock in private
method names, reflection targets, or incidental Unity object layout.

The repository does not yet contain implementation code or tests, so there is
no local prior art to copy. The first implementation should establish these
test seams, preferring simulation and host/client contract tests over brittle
scene-script inspection.

### Session Policy Simulation

- Extract eligibility, limits, countdown, rescue ownership, and fallback rules
  into a deterministic host-side policy layer that can run without a live scene.
- Drive time through an injected clock and drive player identities explicitly.
- Verify that each player has an independent per-raid limit.
- Verify that entry consumes the opportunity exactly once.
- Verify that a successful rescue does not refund the opportunity.
- Verify that base-to-map starts a new limit state while internal transitions
  and same-raid reconnects do not.
- Verify immediate final death when no teammate can rescue.
- Verify immediate final death when the last rescuer dies, becomes downed, or
  disconnects.
- Verify that the countdown pauses only during a host-confirmed rescue and
  resumes with the same remaining duration after interruption.

### Network Contract Tests

- Test capability handshakes for matching, missing, and mismatched versions.
- Test that one incompatible participant forces global fallback.
- Test host authority for configuration and rescue completion.
- Test idempotency for duplicate entry, rescue, completion, timer, and death
  messages.
- Test that stale down identifiers cannot mutate a newer down record.
- Test that clients cannot choose a different timer, rescue range, invulnerable
  state, or revived health percentage.
- Test that fallback mode resolves current down records through the native death
  path.
- Test host and client player identities through the same contract.

### Combat and Death Integration Tests

- Use the public co-op death interception seam with representative combat
  damage sources and verify the transition matrix.
- Verify bullets, melee, explosions, traps, fire, and bleeding can trigger a
  down when all eligibility checks pass.
- Verify starvation, thirst, oxygen loss, cold, poison, and scripted death
  remain vanilla.
- Verify deep water, off-scene death, unsafe vehicle dismount, no opportunity,
  no teammate, inactive co-op, and protocol fallback remain vanilla.
- Verify a safe vehicle dismount occurs before the downed state becomes active.
- Verify invulnerability blocks final death by ordinary damage during the
  countdown.
- Verify that disabled invulnerability allows a subsequent lethal combat hit to
  finalize exactly once.
- Verify timeout, disconnect, raid settlement, and compatibility fallback call
  the native death and loot path exactly once.
- Verify revival does not invoke the native death path.

### Interaction and Presentation Tests

- Represent rescue prompts and HUD data as a view model derived from host state,
  then test that view model without requiring pixel-perfect screenshots.
- Verify the native interaction prompt appears only for eligible teammates.
- Verify three-second completion, movement cancellation, range cancellation,
  and in-range collision tolerance.
- Verify camera rotation does not cancel while movement does.
- Verify non-lethal rescuer damage does not cancel.
- Verify a rescuer death or downed transition cancels.
- Verify the first valid rescuer owns the target and other interactions receive
  the locked feedback.
- Verify switching rescue targets resets the previous rescue.
- Verify the white countdown uses ceiling for displayed whole seconds.
- Verify the white countdown pauses during rescue and the yellow bar remains
  independent.
- Verify the 30-meter team-only through-wall marker includes remaining time.

### End-to-End Raid Tests

- Add an in-game smoke test with host and at least two clients using the same
  protocol.
- Put one client into downed state, verify freeze and marker behavior on the
  other clients, perform a rescue, and verify restored health, control,
  inventory, and vulnerability.
- Repeat with timeout and verify native death and loot on every observer.
- Repeat with a client disconnect while downed and verify native death on the
  host and remaining clients.
- Repeat with a mismatched or missing mod and verify the entire session falls
  back to vanilla death.
- Repeat with the host player downed and verify host/client rule symmetry.

## Out of Scope

- Modifying, recompiling, forking, or redistributing the core co-op mod.
- A prone, crawling, ragdoll, custom injury, or custom downed animation.
- Self-revive, enemy revive, NPC revive, or pet revive.
- Consumables, healing items, skills, cooldowns, currency, or class abilities
  as rescue costs.
- Revive progress that persists after movement, range loss, or other
  interruption.
- Rescue stacking, partial credit for multiple rescuers, or multiple simultaneous
  rescuer bonuses.
- Line-of-sight checks for rescue in the first version.
- Downed players moving, crawling, using items, firing weapons, dropping items,
  looting, extracting, or driving.
- Downed state from non-combat survival or scripted death.
- Custom loot, custom inventory transfer, or replacement of vanilla loot.
- Live configuration changes during an active raid.
- Backward compatibility with clients that do not implement the same protocol.
- Host migration or rescue-state recovery beyond what the core co-op mod already
  provides.
- A redesign of the core co-op health, reconnect, or death systems.
- Supporting host-client version mismatches through best-effort state
  interpretation.

## Further Notes

- Read-only analysis indicates the existing game and co-op mod expose usable
  seams for death interception, health synchronization, player spawn, mod
  networking, native interaction, and camera freezing. The feature is feasible
  as an additive mod without replacing the co-op mod.
- The implementation should favor public extension APIs. If a required behavior
  is only reachable through private internals, document that gap before adding
  a fragile patch.
- The exact combat-damage classification must use the strongest structured
  damage context available. A damage-event name or source type should not be
  inferred from localized display text.
- The mod must tolerate scene transitions, repeated spawn callbacks, duplicate
  player objects, and delayed network messages without corrupting per-raid
  opportunity state.
- "Rescue in progress" is the product intent for the locked-target message.
  Final presentation text should follow the project's localization approach
  when localization is introduced.
- Release packaging should include the mod version and protocol version so that
  hosts can verify everyone has a compatible build before entering a raid.
- The first release should clearly identify the supported game and co-op mod
  versions. Unsupported versions should enter fallback rather than risk silent
  corruption.
- This PRD covers product behavior and test seams. Implementation file layout,
  concrete API signatures, and task decomposition belong in the subsequent
  implementation plan.
