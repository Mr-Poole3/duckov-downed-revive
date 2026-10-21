## ADDED Requirements

### Requirement: Downed players display a white countdown

Clients SHALL render remaining down time as a white health-style bar with the
numeric remaining time rounded up to whole seconds.

#### Scenario: Countdown displays remaining seconds

- **WHEN** the authoritative remaining time is 29.2 seconds
- **THEN** the player sees 30 seconds

#### Scenario: Countdown pauses visually

- **WHEN** a valid rescue is active
- **THEN** the white countdown bar stops visibly advancing

### Requirement: Teammates see a through-wall down marker

Living teammates SHALL see a team-only marker through walls within 30 meters.
The marker MUST include the downed player's remaining time.

#### Scenario: Marker appears in range

- **WHEN** a teammate is within 30 meters of a downed player
- **THEN** the teammate sees the down marker through walls

#### Scenario: Marker is hidden outside range

- **WHEN** a teammate is beyond 30 meters
- **THEN** the down marker is not shown

#### Scenario: Enemy does not see the team marker

- **WHEN** an enemy is near the downed player
- **THEN** the team-only down marker is not shown to the enemy

### Requirement: Rescue progress has a separate yellow indicator

The active rescue SHALL display an independent yellow progress bar. Its
progress MUST be visually distinct from the white down countdown.

#### Scenario: Active rescue shows yellow progress

- **WHEN** a valid rescuer begins holding interaction
- **THEN** a yellow rescue progress indicator advances from zero

#### Scenario: Interruption resets only yellow progress

- **WHEN** the rescuer moves or leaves range
- **THEN** the yellow rescue progress resets to zero
- **AND** the white countdown resumes from its paused value
