## ADDED Requirements

### Requirement: Eligible lethal combat damage starts downed state

The host SHALL convert eligible lethal combat damage into downed state instead
of vanilla final death when the player has a down opportunity and at least one
living, non-downed teammate.

Eligible sources are bullets, melee, explosions, traps, fire, and bleeding.
Starvation, thirst, oxygen loss, cold, poison, scripted death, deep water, and
lethal events outside the playable scene MUST use vanilla death.

#### Scenario: Bullet causes eligible down

- **WHEN** a player with an available opportunity would die from a bullet
- **AND** a living teammate is available
- **THEN** the host suppresses vanilla final death
- **AND** consumes the player's down opportunity
- **AND** broadcasts an authoritative downed state

#### Scenario: Non-combat death remains vanilla

- **WHEN** lethal starvation, thirst, oxygen loss, cold, poison, or scripted
  death is applied
- **THEN** the player follows the vanilla death path
- **AND** no down opportunity is consumed

#### Scenario: Deep-water or off-scene death remains vanilla

- **WHEN** a player would die in deep water or outside the playable scene
- **THEN** the player follows the vanilla death path

#### Scenario: No teammate means immediate death

- **WHEN** a player would enter downed state but no living, non-downed teammate
  exists
- **THEN** the player is finalized through the native death path immediately

#### Scenario: Unsafe vehicle dismount means immediate death

- **WHEN** a vehicle occupant would enter downed state but cannot be safely
  dismounted
- **THEN** the player follows the vanilla death path

### Requirement: Downed players are incapacitated

A downed player MUST remain standing with locomotion and animation frozen.
Movement, weapons, item use, interactions, extraction, and other in-game
actions MUST be disabled. Camera position and direction MUST remain fixed.

#### Scenario: Downed input is rejected

- **WHEN** a player is downed
- **THEN** movement, fire, item, interaction, and action inputs do not affect
  the character
- **AND** the camera remains fixed

#### Scenario: Inventory survives incapacitation

- **WHEN** a player enters downed state
- **THEN** inventory, equipment, and quest state remain preserved
- **AND** no loot is spawned before final death

#### Scenario: AI stops targeting and collision remains

- **WHEN** a player is downed
- **THEN** AI stops selecting the player as a target
- **AND** world collision remains active

### Requirement: Downed invulnerability is configurable

Downed players SHALL be invulnerable by default. When the host disables
invulnerability, a subsequent lethal combat hit SHALL finalize the player
immediately.

#### Scenario: Ordinary damage does not finalize an invulnerable player

- **WHEN** a downed player is invulnerable and takes damage
- **THEN** the player remains downed
- **AND** the countdown continues unless a valid rescue is active

#### Scenario: Lethal hit finalizes a vulnerable player

- **WHEN** downed invulnerability is disabled
- **AND** the player takes a subsequent lethal combat hit
- **THEN** the player is finalized through the native death path

#### Scenario: Non-damaging collision does not finalize

- **WHEN** a vulnerable downed player collides with the world without a lethal
  combat-damage event
- **THEN** the player remains downed

### Requirement: Host-authoritative countdown ends in final death

The host SHALL own a configurable down countdown with a default of 30 seconds.
When the countdown expires, the player MUST be finalized through the game's
native death, inventory, and loot processing.

#### Scenario: Countdown expires

- **WHEN** the authoritative down countdown reaches zero
- **THEN** the host finalizes the player exactly once
- **AND** the native death and loot flow runs

#### Scenario: Host and clients observe the same time

- **WHEN** a client renders remaining down time
- **THEN** it derives the value from the host's authoritative state

### Requirement: Terminal conditions use native final death

The system MUST finalize a downed player through the native death path exactly
once for timeout, downed disconnect, last-rescuer loss, compatibility fallback,
or raid settlement. A downed player MUST NOT extract successfully.

#### Scenario: Downed player disconnects

- **WHEN** a downed player disconnects
- **THEN** the host immediately finalizes the player
- **AND** reconnecting does not restore the downed state

#### Scenario: Last possible rescuer is lost

- **WHEN** the final living, non-downed teammate dies, becomes downed, or
  disconnects
- **THEN** affected downed players are finalized immediately

#### Scenario: Downed player reaches raid settlement

- **WHEN** raid settlement begins while a player remains downed
- **THEN** the player is finalized as dead before settlement resolves
- **AND** cannot count as successfully extracted

#### Scenario: Finalization is attempted twice

- **WHEN** duplicate timeout, disconnect, or settlement messages arrive
- **THEN** the native death and loot path runs at most once
