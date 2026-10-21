## ADDED Requirements

### Requirement: Session activation requires a compatible protocol

The Mod MUST use a versioned capability handshake with every participant. The
feature SHALL activate for a raid only when all participants support the same
compatible protocol.

#### Scenario: Compatible participants enable the feature

- **WHEN** every connected participant advertises the same supported protocol
- **THEN** the host activates the downed system for the next eligible raid

#### Scenario: Missing Mod disables the feature

- **WHEN** any connected participant does not advertise the downed protocol
- **THEN** the downed system is disabled globally for that session
- **AND** lethal damage uses vanilla immediate death

#### Scenario: Protocol mismatch disables the feature

- **WHEN** any participant advertises an incompatible protocol version
- **THEN** the downed system is disabled globally without partial operation

#### Scenario: Mismatch joins an active session

- **WHEN** an incompatible participant joins after the downed system has activated
- **THEN** the session enters compatibility fallback
- **AND** every current downed player is finalized through the native death path

### Requirement: Host configuration is authoritative and raid-scoped

The host SHALL own the configuration snapshot. Clients MUST use the host's
locked values for the current raid. Configuration changes made during a raid
SHALL apply only on the next base-to-map transition.

#### Scenario: Client cannot override host settings

- **WHEN** a client presents a different timer, down count, invulnerability
  setting, or revive health value
- **THEN** the host rejects or ignores the client value
- **AND** the session continues using the host snapshot

#### Scenario: Configuration changes wait for the next raid

- **WHEN** the host changes a gameplay setting during an active raid
- **THEN** the current raid keeps the locked configuration
- **AND** the next base-to-map transition uses the new value

#### Scenario: Invalid configuration is rejected

- **WHEN** the host configuration contains a negative down count or a
  non-positive countdown duration
- **THEN** the host clamps or rejects the invalid value
- **AND** never creates a revive result below one health point

### Requirement: Host and client players follow the same rules

Down entry, countdown, rescue, revival, and final death MUST use the same state
transitions regardless of whether the affected player is the host.

#### Scenario: Host player is downed

- **WHEN** the host player takes eligible lethal combat damage
- **THEN** the host enters the same downed state and countdown as a client

#### Scenario: Client player is downed

- **WHEN** a client player takes eligible lethal combat damage
- **THEN** the host owns the entry decision
- **AND** all participants render the host-accepted state
