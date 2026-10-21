## ADDED Requirements

### Requirement: Down opportunities are scoped to a raid

Each player SHALL have an independent, configurable number of down
opportunities for each raid. The default SHALL be one. A down opportunity MUST
be consumed when the player enters the downed state.

#### Scenario: New raid resets opportunities

- **WHEN** the host transitions from the base into a raid map
- **THEN** every participant receives the configured number of down opportunities
- **AND** the reset uses the same raid identity for host and clients

#### Scenario: Internal transition preserves consumption

- **WHEN** a player has consumed a down opportunity and the game performs an
  internal scene transition
- **THEN** the opportunity remains consumed

#### Scenario: Same-raid reconnect preserves consumption

- **WHEN** a player reconnects while the same raid is still active
- **THEN** the host does not restore the player's consumed down opportunities

#### Scenario: Successful rescue does not refund a down

- **WHEN** a downed player is revived successfully
- **THEN** the consumed down opportunity remains consumed

#### Scenario: Returning to base ends the raid

- **WHEN** the session returns to the base
- **THEN** the active raid state ends
- **AND** a later base-to-map transition starts with fresh opportunities

### Requirement: Inactive co-op uses vanilla death

The downed system MUST remain inactive outside an active, compatible co-op
raid.

#### Scenario: Single-player lethal damage

- **WHEN** a player takes lethal combat damage in single-player
- **THEN** the player follows the vanilla immediate-death behavior

#### Scenario: Co-op session not active

- **WHEN** the Mod is loaded but no active co-op raid is available
- **THEN** the downed system does not alter damage or death behavior
