using System;
using System.IO;
using System.Reflection;
using DuckovDownedRevive.Core;
using EscapeFromDuckovCoopMod;
using HarmonyLib;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

namespace DuckovDownedRevive.Mod
{
    public sealed class DownedReviveModBehaviour :
        Duckov.Modding.ModBehaviour
    {
        private const float RetryIntervalSeconds = 1.0f;
        private const string PrefMaxDowns = "DuckovDownedRevive.MaxDowns";
        private const string PrefDownDuration = "DuckovDownedRevive.DownDuration";
        private const string PrefInvulnerable = "DuckovDownedRevive.Invulnerable";
        private const string PrefReviveHealth = "DuckovDownedRevive.ReviveHealth";

        private readonly CompatibilityTracker _tracker =
            new CompatibilityTracker();
        private readonly HostConfigSession _configSession =
            new HostConfigSession();
        private readonly DownOpportunityLedger _ledger =
            new DownOpportunityLedger();
        private readonly PlayerRoster _roster = new PlayerRoster();
        private static DownedReviveModBehaviour _instance;

        private IDisposable _handlerRegistration;
        private Harmony _harmony;
        private Action<CharacterMainControl, string, bool> _playerSpawnedHandler;
        private Action<NetPeer> _peerConnectedHandler;
        private Action<NetPeer> _peerDisconnectedHandler;
        private Action<RaidUtilities.RaidInfo> _newRaidHandler;
        private Action<RaidUtilities.RaidInfo> _raidEndHandler;

        private bool _isHost;
        private bool _overlayVisible;
        private bool _configPanelVisible;
        private bool _fallbackBroadcast;
        private float _nextRetryAt;
        private uint _currentRaidId;
        private uint _observedRaidId;
        private DownedReviveConfig _replicatedConfig;
        private CompatibilityStatus _lastLoggedStatus;
        private string _diagnosticText = "Downed Revive not initialized.";

        public static DownedReviveModBehaviour Instance
        {
            get { return _instance; }
        }

        public bool IsHost
        {
            get { return _isHost; }
        }

        private void Awake()
        {
            _instance = this;
            LoadSettings();

            _playerSpawnedHandler = OnPlayerSpawned;
            _peerConnectedHandler = OnPeerConnected;
            _peerDisconnectedHandler = OnPeerDisconnected;
            _newRaidHandler = OnNewRaid;
            _raidEndHandler = OnRaidEnd;

            _handlerRegistration = ModNetworkApi.RegisterHandler(
                DownedReviveProtocol.ChannelName,
                OnNetworkMessage);
            ModNetworkApi.PeerConnected += _peerConnectedHandler;
            ModNetworkApi.PeerDisconnected += _peerDisconnectedHandler;
            ModApiEvents.PlayerSpawned += _playerSpawnedHandler;
            RaidUtilities.OnNewRaid += _newRaidHandler;
            RaidUtilities.OnRaidEnd += _raidEndHandler;

            RaidUtilities.RaidInfo currentRaid = RaidUtilities.CurrentRaid;
            if (currentRaid.valid)
            {
                _currentRaidId = currentRaid.ID;
            }

            _harmony = new Harmony("duckov.downed-revive");
            _harmony.PatchAll(typeof(DownedReviveModBehaviour).Assembly);
            RefreshDiagnostics();
            Debug.Log("[DuckovDownedRevive] Compatibility tracer loaded. " +
                _diagnosticText.Replace("\n", " | "));
        }

        private void OnEnable()
        {
            RefreshDiagnostics();
        }

        private void OnDisable()
        {
            if (_handlerRegistration != null)
            {
                _handlerRegistration.Dispose();
                _handlerRegistration = null;
            }

            if (_peerConnectedHandler != null)
            {
                ModNetworkApi.PeerConnected -= _peerConnectedHandler;
            }

            if (_playerSpawnedHandler != null)
            {
                ModApiEvents.PlayerSpawned -= _playerSpawnedHandler;
            }

            if (_peerDisconnectedHandler != null)
            {
                ModNetworkApi.PeerDisconnected -= _peerDisconnectedHandler;
            }

            if (_newRaidHandler != null)
            {
                RaidUtilities.OnNewRaid -= _newRaidHandler;
            }

            if (_raidEndHandler != null)
            {
                RaidUtilities.OnRaidEnd -= _raidEndHandler;
            }

            if (_harmony != null)
            {
                _harmony.UnpatchAll(_harmony.Id);
                _harmony = null;
            }

            if (ReferenceEquals(_instance, this))
            {
                _instance = null;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F8))
            {
                _overlayVisible = !_overlayVisible;
            }

            if (Input.GetKeyDown(KeyCode.F9))
            {
                _configPanelVisible = !_configPanelVisible;
            }

            float now = Time.realtimeSinceStartup;
            if (now >= _nextRetryAt)
            {
                _nextRetryAt = now + RetryIntervalSeconds;
                UpdateHostRole();
                UpdateObservedRaid();
                RefreshDiagnostics();

                if (_tracker.Evaluate(now).Status ==
                    CompatibilityStatus.AwaitingHandshake)
                {
                    TrySendCapabilityToServer();
                }
            }
        }

        private void OnGUI()
        {
            if (_overlayVisible)
            {
                GUI.Box(new Rect(16, 16, 520, 168), "Duckov Downed Revive");
                GUI.Label(new Rect(28, 44, 496, 128), _diagnosticText);
                GUI.Label(new Rect(16, 188, 520, 20),
                    "F8 toggles this compatibility report.");
            }

            if (_configPanelVisible)
            {
                DrawConfigPanel();
            }
        }

        private void OnPeerConnected(NetPeer peer)
        {
            _tracker.SetSessionActive(true);
            _tracker.RegisterPeer(PeerId(peer), Time.realtimeSinceStartup);
            TrySendCapabilityToServer();
            if (_isHost)
            {
                BroadcastCapability();
            }

            RefreshDiagnostics();
        }

        private void OnPeerDisconnected(NetPeer peer)
        {
            _tracker.DisconnectPeer(PeerId(peer));
            RefreshDiagnostics();
        }

        private void OnPlayerSpawned(
            CharacterMainControl character,
            string playerName,
            bool isLocal)
        {
            string identity = !string.IsNullOrEmpty(playerName)
                ? playerName
                : (isLocal ? "local-player" : "unknown-player");
            _roster.AddOrUpdate(identity, isLocal);
            if (_isHost && _ledger.IsActive)
            {
                _ledger.RegisterPlayer(identity);
            }
        }

        private void OnNewRaid(RaidUtilities.RaidInfo raid)
        {
            _currentRaidId = raid.valid ? raid.ID : 0;
            _tracker.SetRaid(_currentRaidId);
        }

        private void OnRaidEnd(RaidUtilities.RaidInfo raid)
        {
            _currentRaidId = 0;
            _tracker.ClearRaid();
        }

        private void OnNetworkMessage(ModMessageContext context)
        {
            if (!string.Equals(
                    context.Channel,
                    DownedReviveProtocol.ChannelName,
                    StringComparison.Ordinal))
            {
                return;
            }

            _isHost = context.IsServer;
            _tracker.SetSessionActive(true);

            byte[] payload = ReadPayload(context);
            if (payload == null)
            {
                _tracker.ForceFallback("payload-unavailable");
                RefreshDiagnostics();
                return;
            }

            if (payload.Length < 5)
            {
                _tracker.ForceFallback("malformed-capability-message");
                RefreshDiagnostics();
                return;
            }

            using (MemoryStream stream = new MemoryStream(payload))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                byte messageType = reader.ReadByte();
                int protocolVersion = reader.ReadInt32();

                if (messageType == CompatibilityMessages.Capability)
                {
                    _tracker.RegisterCapability(
                        SenderId(context),
                        protocolVersion,
                        Time.realtimeSinceStartup);

                    if (_isHost)
                    {
                        BroadcastCapability();
                        BroadcastCompatibilityStatus();
                        if (_configSession.IsLocked)
                        {
                            BroadcastConfigSnapshot();
                        }
                    }
                }
                else if (messageType == CompatibilityMessages.Status)
                {
                    if (protocolVersion == (int)CompatibilityStatus.Fallback)
                    {
                        _tracker.ForceFallback("host-reported-fallback");
                    }
                }
                else if (messageType == CompatibilityMessages.ConfigSnapshot)
                {
                    if (!_isHost && stream.Position + 9 <= stream.Length)
                    {
                        DownedReviveConfig config = new DownedReviveConfig();
                        config.MaxDownsPerPlayer = protocolVersion;
                        config.DownDurationSeconds = reader.ReadSingle();
                        config.DownedInvulnerable = reader.ReadBoolean();
                        config.ReviveHealthPercent = reader.ReadSingle();
                        _replicatedConfig = config.Normalize();
                    }
                }
            }

            RefreshDiagnostics();
        }

        private bool TrySendCapabilityToServer()
        {
            try
            {
                return ModNetworkApi.SendToServer(
                    DownedReviveProtocol.ChannelName,
                    delegate(NetDataWriter writer)
                    {
                        writer.Put(CompatibilityMessages.Capability);
                        writer.Put(DownedReviveProtocol.CurrentVersion);
                    });
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    "[DuckovDownedRevive] Capability send failed: " +
                    exception.Message);
                return false;
            }
        }

        private void BroadcastCapability()
        {
            try
            {
                ModNetworkApi.Broadcast(
                    DownedReviveProtocol.ChannelName,
                    delegate(NetDataWriter writer)
                    {
                        writer.Put(CompatibilityMessages.Capability);
                        writer.Put(DownedReviveProtocol.CurrentVersion);
                    },
                    true);
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    "[DuckovDownedRevive] Capability broadcast failed: " +
                    exception.Message);
            }
        }

        private void BroadcastCompatibilityStatus()
        {
            CompatibilitySnapshot snapshot =
                _tracker.Evaluate(Time.realtimeSinceStartup);
            if (snapshot.Status != CompatibilityStatus.Fallback)
            {
                return;
            }

            try
            {
                ModNetworkApi.Broadcast(
                    DownedReviveProtocol.ChannelName,
                    delegate(NetDataWriter writer)
                    {
                        writer.Put(CompatibilityMessages.Status);
                        writer.Put((int)CompatibilityStatus.Fallback);
                    },
                    true);
                _fallbackBroadcast = true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    "[DuckovDownedRevive] Fallback broadcast failed: " +
                    exception.Message);
            }
        }

        private void BroadcastConfigSnapshot()
        {
            DownedReviveConfig config = _configSession.Locked;
            if (config == null)
            {
                return;
            }

            try
            {
                ModNetworkApi.Broadcast(
                    DownedReviveProtocol.ChannelName,
                    delegate(NetDataWriter writer)
                    {
                        writer.Put(CompatibilityMessages.ConfigSnapshot);
                        writer.Put(config.MaxDownsPerPlayer);
                        writer.Put(config.DownDurationSeconds);
                        writer.Put(config.DownedInvulnerable);
                        writer.Put(config.ReviveHealthPercent);
                    },
                    true);
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    "[DuckovDownedRevive] Config broadcast failed: " +
                    exception.Message);
            }
        }

        private void RefreshDiagnostics()
        {
            CompatibilitySnapshot snapshot =
                _tracker.Evaluate(Time.realtimeSinceStartup);
            _diagnosticText = CompatibilityDiagnosticsFormatter.Format(snapshot);

            if (snapshot.Status != _lastLoggedStatus)
            {
                Debug.Log("[DuckovDownedRevive] " +
                    _diagnosticText.Replace("\n", " | "));
                _lastLoggedStatus = snapshot.Status;
            }

            if (_isHost &&
                snapshot.Status == CompatibilityStatus.Fallback &&
                !_fallbackBroadcast)
            {
                BroadcastCompatibilityStatus();
            }
        }

        private static string PeerId(NetPeer peer)
        {
            return peer == null ? "unknown-peer" : peer.Id.ToString();
        }

        private static string SenderId(ModMessageContext context)
        {
            return context.Sender == null
                ? "unknown-peer"
                : context.Sender.Id.ToString();
        }

        private static byte[] ReadPayload(ModMessageContext context)
        {
            try
            {
                PropertyInfo payloadProperty = typeof(ModMessageContext)
                    .GetProperty("Payload", BindingFlags.Public | BindingFlags.Instance);
                if (payloadProperty == null)
                {
                    return null;
                }

                object payload = payloadProperty.GetValue(context, null);
                if (payload == null)
                {
                    return null;
                }

                MethodInfo toArray = payload.GetType().GetMethod(
                    "ToArray",
                    Type.EmptyTypes);
                return toArray == null
                    ? null
                    : toArray.Invoke(payload, null) as byte[];
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void LoadSettings()
        {
            DownedReviveConfig config = DownedReviveConfig.CreateDefault();
            config.MaxDownsPerPlayer = PlayerPrefs.GetInt(
                PrefMaxDowns,
                config.MaxDownsPerPlayer);
            config.DownDurationSeconds = PlayerPrefs.GetFloat(
                PrefDownDuration,
                config.DownDurationSeconds);
            config.DownedInvulnerable = PlayerPrefs.GetInt(
                PrefInvulnerable,
                config.DownedInvulnerable ? 1 : 0) != 0;
            config.ReviveHealthPercent = PlayerPrefs.GetFloat(
                PrefReviveHealth,
                config.ReviveHealthPercent);
            _configSession.SetPending(config.Normalize());
        }

        private void SaveSettings(DownedReviveConfig config)
        {
            _configSession.SetPending(config.Normalize());
            DownedReviveConfig saved = _configSession.Pending;
            PlayerPrefs.SetInt(PrefMaxDowns, saved.MaxDownsPerPlayer);
            PlayerPrefs.SetFloat(PrefDownDuration, saved.DownDurationSeconds);
            PlayerPrefs.SetInt(
                PrefInvulnerable,
                saved.DownedInvulnerable ? 1 : 0);
            PlayerPrefs.SetFloat(
                PrefReviveHealth,
                saved.ReviveHealthPercent);
            PlayerPrefs.Save();
        }

        private void DrawConfigPanel()
        {
            float top = _overlayVisible ? 216.0f : 16.0f;
            Rect panel = new Rect(16, top, 560, 222);
            GUI.Box(panel, "Duckov Downed Revive - Host Rules");

            DownedReviveConfig config = _isHost
                ? _configSession.Pending
                : (_replicatedConfig ?? _configSession.EffectiveForCurrentRaid());
            bool changed = false;

            GUI.Label(new Rect(30, top + 28, 240, 22),
                "Downs per player: " + config.MaxDownsPerPlayer);
            if (_isHost)
            {
                if (GUI.Button(new Rect(330, top + 26, 32, 24), "-"))
                {
                    config.MaxDownsPerPlayer--;
                    changed = true;
                }
                if (GUI.Button(new Rect(368, top + 26, 32, 24), "+"))
                {
                    config.MaxDownsPerPlayer++;
                    changed = true;
                }
            }

            GUI.Label(new Rect(30, top + 58, 260, 22),
                "Down countdown: " +
                config.DownDurationSeconds.ToString("0") + " sec");
            if (_isHost)
            {
                if (GUI.Button(new Rect(330, top + 56, 32, 24), "-5"))
                {
                    config.DownDurationSeconds -= 5.0f;
                    changed = true;
                }
                if (GUI.Button(new Rect(368, top + 56, 32, 24), "+5"))
                {
                    config.DownDurationSeconds += 5.0f;
                    changed = true;
                }
            }

            GUI.Label(new Rect(30, top + 88, 260, 22),
                "Downed invulnerable: " +
                (config.DownedInvulnerable ? "ON" : "OFF"));
            if (_isHost &&
                GUI.Button(new Rect(330, top + 86, 70, 24), "Toggle"))
            {
                config.DownedInvulnerable = !config.DownedInvulnerable;
                changed = true;
            }

            GUI.Label(new Rect(30, top + 118, 260, 22),
                "Revive health: " +
                config.ReviveHealthPercent.ToString("0") + "%");
            if (_isHost)
            {
                if (GUI.Button(new Rect(330, top + 116, 32, 24), "-"))
                {
                    config.ReviveHealthPercent--;
                    changed = true;
                }
                if (GUI.Button(new Rect(368, top + 116, 32, 24), "+"))
                {
                    config.ReviveHealthPercent++;
                    changed = true;
                }
            }

            if (changed)
            {
                SaveSettings(config);
            }

            string stateText;
            if (!_isHost)
            {
                stateText = _replicatedConfig == null
                    ? "Host role not confirmed. Values are read-only."
                    : "Host-locked values for this session are read-only.";
            }
            else if (_configSession.IsLocked)
            {
                stateText = "Changes apply on the next raid.";
            }
            else
            {
                stateText = "Values will lock when the next raid starts.";
            }

            GUI.Label(new Rect(30, top + 150, 510, 22), stateText);
            GUI.Label(new Rect(30, top + 176, 510, 22),
                "F9 closes this panel. Settings persist through PlayerPrefs.");
        }

        private void UpdateHostRole()
        {
            try
            {
                FieldInfo backendField = typeof(ModNetworkApi).GetField(
                    "_backend",
                    BindingFlags.NonPublic | BindingFlags.Static);
                if (backendField == null)
                {
                    return;
                }

                object backend = backendField.GetValue(null);
                if (backend == null)
                {
                    return;
                }

                PropertyInfo isServerProperty = backend.GetType().GetProperty(
                    "IsServer",
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Instance);
                if (isServerProperty != null)
                {
                    _isHost = Convert.ToBoolean(
                        isServerProperty.GetValue(backend, null));
                }
            }
            catch (Exception)
            {
            }
        }

        private void UpdateObservedRaid()
        {
            uint raidId = _currentRaidId != 0
                ? _currentRaidId
                : ReadActiveRaidId();
            if (raidId == 0)
            {
                _tracker.ClearRaid();
                if (_observedRaidId != 0)
                {
                    _observedRaidId = 0;
                    _configSession.EndRaid();
                    _ledger.EndRaid();
                }
                return;
            }

            _tracker.SetRaid(raidId);
            if (!_isHost || _observedRaidId == raidId)
            {
                return;
            }

            DownedReviveConfig locked = _configSession.LockForRaid();
            _ledger.BeginRaid(raidId, locked.MaxDownsPerPlayer);
            _ledger.RegisterPlayers(_roster.PlayerIds);
            _observedRaidId = raidId;
            BroadcastConfigSnapshot();
        }

        private static uint ReadActiveRaidId()
        {
            RaidUtilities.RaidInfo raid = RaidUtilities.CurrentRaid;
            return raid.valid ? raid.ID : 0;
        }
    }
}
