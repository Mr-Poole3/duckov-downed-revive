## ADDED Requirements

### Requirement: Teammates can initiate rescue through native interaction

Any living, non-downed player teammate within 2.5 meters SHALL be able to rescue
a downed player by holding the game's native interaction input for three
seconds. Line of sight SHALL NOT be required in V1.

#### Scenario: Eligible teammate sees the rescue prompt

- **WHEN** a living teammate enters the rescue range
- **THEN** the native interaction prompt is available for the downed target

#### Scenario: Downed player or enemy cannot rescue

- **WHEN** the downed player or an enemy attempts to rescue
- **THEN** no rescue can start

#### Scenario: Friendly-fire attacker can rescue

- **WHEN** the player who caused the down remains living and non-downed
- **THEN** that player can rescue the same teammate

### Requirement: Rescue progress has explicit cancellation rules

Movement input or leaving 2.5 meters MUST interrupt rescue and reset rescue
progress to zero. Camera rotation, non-lethal damage, collision, or terrain
sliding within range MUST NOT interrupt rescue. The rescuer's death or downed
transition MUST interrupt rescue.

#### Scenario: Movement cancels rescue

- **WHEN** a rescuer presses a movement input before three seconds elapse
- **THEN** rescue progress resets to zero

#### Scenario: Range loss cancels rescue

- **WHEN** the rescuer moves beyond 2.5 meters
- **THEN** rescue progress resets to zero

#### Scenario: Camera rotation does not cancel

- **WHEN** the rescuer rotates the camera while holding interaction and
  remaining in range
- **THEN** rescue progress continues

#### Scenario: Non-lethal damage does not cancel

- **WHEN** the rescuer takes non-lethal damage while in range and holding input
- **THEN** rescue progress continues

#### Scenario: Rescuer incapacitation cancels

- **WHEN** the rescuer dies, becomes downed, or disconnects
- **THEN** rescue ends immediately
- **AND** its progress resets

### Requirement: Rescue ownership is exclusive

Only one rescuer MAY own a target. The first valid rescuer to start SHALL obtain
the lock. A rescuer MUST NOT progress more than one target at a time. Other
players interacting with a locked target SHALL receive feedback that rescue is
already in progress.

#### Scenario: First rescuer owns the target

- **WHEN** two eligible teammates attempt to rescue the same target
- **THEN** the first valid requester obtains the lock
- **AND** progress does not stack from the second player

#### Scenario: Locked target gives feedback

- **WHEN** another teammate interacts with a target that has an owner
- **THEN** the client displays "Rescue in progress" or its localized equivalent

#### Scenario: Rescuer switches target

- **WHEN** a rescuer starts on another downed target
- **THEN** the previous rescue is interrupted and reset
- **AND** ownership moves to the new target

### Requirement: Active rescue pauses the countdown

The down countdown MUST pause while a host-confirmed valid rescue owns the
target. When rescue is interrupted, the countdown SHALL resume from its paused
remaining value and rescue progress SHALL reset to zero.

#### Scenario: Rescue pauses countdown

- **WHEN** the host grants rescue ownership
- **THEN** the white down countdown stops advancing

#### Scenario: Interrupted rescue resumes countdown

- **WHEN** a valid rescue is interrupted
- **THEN** rescue progress becomes zero
- **AND** the countdown resumes with the same remaining time it had when paused

### Requirement: Successful rescue restores playable state

On authoritative completion, the player SHALL regain control and vulnerability
immediately. Current health MUST become the configured percentage of maximum
health, defaulting to 10% with a minimum of one health point. No post-revive
invulnerability SHALL be applied.

#### Scenario: Rescue completes

- **WHEN** the rescuer holds interaction for three seconds without interruption
- **THEN** the host broadcasts revival
- **AND** the revived player regains full controls and damage vulnerability

#### Scenario: Revival health is applied

- **WHEN** revival completes at the default health percentage
- **THEN** the player's current health is 10% of maximum health
- **AND** never below one health point

#### Scenario: Revival preserves run state

- **WHEN** revival completes
- **THEN** the player remains in the same raid with inventory and equipment
  preserved
