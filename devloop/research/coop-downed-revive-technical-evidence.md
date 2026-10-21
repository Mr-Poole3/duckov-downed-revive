# Co-op Downed Revive Technical Evidence

## Scope

This note records static, read-only observations from the shipped game and the
installed co-op Mod. No target code was executed. API names and signatures are
observations from managed metadata; integration choices remain design decisions.

## Artifact Identity

| Artifact | SHA-256 | Evidence |
| --- | --- | --- |
| `EscapeFromDuckovModApi.dll` | `9a585015a68b895435ca5f935b497674d66e2638d7c5ff9042c586e7e2d10e7b` | `ev_ac57e7540c309e9b54217d46916bd575076ae4df2fcc367fa651988496231126` |
| `EscapeFromDuckovCoopMod.dll` | `05f06045ce50b9989c5207b23a9a4bb2d6c66137cb4a8a26318a9ca3c48e4706` | `ev_8280cdb597b79648bd69e01c6b1848b297da0ae4db2a6b44ed84e9f5cdbf3f75` |
| `TeamSoda.Duckov.Core.dll` | `298d5d5885427632d5a94b2f3ce587f8ebc9528ec71e575a475158c326ecae8f` | `ev_3ca64f73ef66b62b5f309ab8f259f51d743d916036299e024c186a61951e8be7` |

Both Mod assemblies and the game core target the Unity Mono managed runtime.

## Co-op Mod Extension API

### Network transport

Observed public static API on `EscapeFromDuckovCoopMod.ModNetworkApi`:

- `RegisterHandler(string channel, Action<ModMessageContext> handler)`
- `SendToServer(string channel, Action<BinaryWriter> writePayload) -> bool`
- `SendToClient(object peer, string channel, Action<BinaryWriter> writePayload, bool reliable) -> bool`
- `Broadcast(string channel, Action<BinaryWriter> writePayload, bool reliable) -> int`
- `PeerConnected` and `PeerDisconnected` events
- `NotifyPeerConnected(object peer)` and `NotifyPeerDisconnected(object peer)`

`ModMessageContext` exposes `Service`, `Sender`, `Channel`, `Payload`,
`IsServer`, and `IsHostMessage`. The API performs queuing, replay, and duplicate
suppression internally.

Evidence:

- `ev_8cdf8662a48a1460bb414139ee964fdcfaea1445007aae160495ca3c501617ed`
- `ev_822fb2364ae7325a4c013d6281f9a5eef431b18387b4717211bb5065e294a813`
- `ev_3eaa16fb90d39647be71a456f2e7b5349c8bbf364f6cca49012cd22f4faff988`

### Player lifecycle event

`ModApiEvents.PlayerSpawned` is an event carrying the spawned
`CharacterMainControl`, player name, and a boolean flag. This is the preferred
point to associate a game character with a stable co-op player identity.

Evidence:

- `ev_2bfc8953ae96e22808f17b3fa5ff38085a30ec7f26853a2ce9c7e065ec5cec1c`
- `ev_61c2a567b11d6372465712711b5b1dd39deb050765d0ae86fc6c15c713a86a18`

## Game Death and Health API

Observed members on `Health`:

- `bool Hurt(DamageInfo damageInfo)`
- `float CurrentHealth { get; set; }`
- `float MaxHealth { get; }`
- `bool Invincible { get; }`
- `void SetInvincible(bool value)`
- `void SetHealth(float value)`
- `void AddHealth(float value)`
- `bool IsDead { get; }`
- `UnityEvent/event OnHurt(DamageInfo)` and `OnDead(DamageInfo)` style hooks

Observed members on `DamageInfo` include the damage source character, final
damage, explosion and bleed-related fields, and a constructor taking the
attacking `CharacterMainControl`.

Observed members on `CharacterMainControl` include:

- `Health Health { get; }`
- `void OnDead(DamageInfo damageInfo)`
- `void SetMoveInput(Vector3 input)`
- `void SetRunInput(bool value)`
- `void SetAdsInput(bool value)`
- `void RefreshInteractTarget()`
- `bool Interact(InteractableBase target)` and parameterless `Interact()`
- vehicle and water state properties

Evidence:

- `ev_81d250f6b0c5b20290251bfe579f52f4f7b51ff9e74a4d59f169a0e9edba3d5f`
- `ev_afc8e1d8f904983a2ae3b933139a3d2fb8e7a530498fb73ba598aca896d24722`

### Co-op health integration

Observed co-op types and seams:

- `Patch_Health_Hurt_RemoteAnti.Prefix(Health, ref DamageInfo)`
- `Patch_Health_Hurt_HPClamp.Transpiler(...)`
- `HealthM.Instance`
- `HealthM.ForceSetHealth(...)`
- `HealthM.Server_ApplyDamageToHost(DamageInfo)`
- `HealthM.Client_ApplyForwardedDamage(DamageInfo)`
- `HealthM.BroadcastPlayerSnapshot(...)`
- `PlayerDamageRequestRpc`
- `PlayerHealthBroadcastRpc`

The co-op Mod already centralizes client damage requests and host health
broadcasts. The revival Mod should observe and cooperate with this path rather
than introduce a second health replication mechanism.

Evidence:

- `ev_7636b2b7adf9efce6a0ca0c4e1dbeb679587ab05b9dfc87eed05344cdf65e733`
- `ev_1803dc05cc20a6a1df1aeae3ca530982594c34c5883422592197ec4b8d327149`

## Freeze, Camera, and Input

Observed public seams:

- `GameCamera.Instance`
- `GameCamera.FreezeCamera(float duration)`
- `InputManager.instance`
- `InputManager.DisableInput(Object source)`
- `InputManager.ActiveInput(Object source)`
- `CharacterInputControl.Bind(...)` and `Unbind(...)`
- `CharacterMainControl.SetMoveInput`, `SetRunInput`, `SetAdsInput`

Evidence:

- `ev_3fbfd38675d5a9bccbb058caffac6a876294bf2ae23cbd08d526cf0118ea748a`

## Interaction

Observed `InteractableBase` API:

- `float InteractTime`
- `string InteractName`
- `bool StartInteract(CharacterMainControl character)`
- `void UpdateInteract(CharacterMainControl character, float deltaTime)`
- `void FinishInteract(CharacterMainControl character)`
- `void StopInteract()`
- `bool IsInteractable()`
- `bool CheckInteractable()`
- `void RefreshInteract()`

`InputManager` exposes `Interact()`, and `CharacterMainControl` exposes
`GetInteractableTargetToInteract()` and `Interact(InteractableBase)`. This
supports a native-prompt-compatible rescue interactable.

Evidence:

- `ev_3fbfd38675d5a9bccbb058caffac6a876294bf2ae23cbd08d526cf0118ea748a`

## Raid Lifecycle

Observed public `RaidUtilities` events and members:

- `OnNewRaid`
- `OnRaidDead`
- `OnRaidEnd`
- `CurrentRaid`
- `NewRaid()`, `NotifyDead()`, `NotifyEnd()`

`RaidInfo` includes `valid`, `ID`, `dead`, `ended`, and timing fields.

Evidence:

- `ev_e6870ff2045874db0938e75396ccd567df470878369cc811ba53e0aaa24e6268`
- `ev_ee118499fdcf5a8b14b9e86dbbee0f6732abcd308642898992a77aed1abe72c9`

## Mod Loading

The installed Harmony loader exposes `HarmonyLoadMod.ModBehaviour`, a Unity
MonoBehaviour-derived Mod base with `Awake`, `OnEnable`, `OnDisable`,
`OnDestroy`, and `Load0Harmony`. Installed Mods contain a managed DLL and
`info.ini` with `name`, `displayName`, `description`, and `publishedFileId`.

Evidence:

- `ev_1803dc05cc20a6a1df1aeae3ca530982594c34c5883422592197ec4b8d327149`

## Constraints and Unknowns

- No third-party source for the game or co-op Mod is available.
- Static metadata proves signatures, not runtime patch ordering under every
  co-op scenario.
- The first implementation must include an in-game diagnostic mode to confirm
  which representation receives lethal damage on host and client.
- Co-op Mod versions other than the recorded artifact are not proven
  compatible and must enter fallback mode.
- No existing test project or build toolchain exists in the repository.
