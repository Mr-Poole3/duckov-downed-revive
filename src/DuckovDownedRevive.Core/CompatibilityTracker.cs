using System;
using System.Collections.Generic;

namespace DuckovDownedRevive.Core
{
    public sealed class CompatibilityTracker
    {
        private sealed class PeerState
        {
            public string PeerId;
            public double ConnectedAt;
            public int? ProtocolVersion;
        }

        private readonly Dictionary<string, PeerState> _peers =
            new Dictionary<string, PeerState>(StringComparer.Ordinal);

        private bool _sessionActive;
        private bool _forcedFallback;
        private string _forcedFallbackReason;
        private uint _raidId;

        public CompatibilityTracker()
        {
            HandshakeTimeoutSeconds =
                DownedReviveProtocol.DefaultHandshakeTimeoutSeconds;
        }

        public double HandshakeTimeoutSeconds { get; set; }

        public void SetSessionActive(bool active)
        {
            _sessionActive = active;
            if (!active)
            {
                _raidId = 0;
            }
        }

        public void SetRaid(uint raidId)
        {
            _raidId = raidId;
        }

        public void ClearRaid()
        {
            _raidId = 0;
        }

        public void RegisterPeer(string peerId, double now)
        {
            string normalized = NormalizePeerId(peerId);
            PeerState state;
            if (_peers.TryGetValue(normalized, out state))
            {
                return;
            }

            _peers.Add(normalized, new PeerState
            {
                PeerId = normalized,
                ConnectedAt = now,
                ProtocolVersion = null
            });
        }

        public void RegisterCapability(string peerId, int protocolVersion, double now)
        {
            string normalized = NormalizePeerId(peerId);
            PeerState state;
            if (!_peers.TryGetValue(normalized, out state))
            {
                state = new PeerState
                {
                    PeerId = normalized,
                    ConnectedAt = now
                };
                _peers.Add(normalized, state);
            }

            state.ProtocolVersion = protocolVersion;
        }

        public void DisconnectPeer(string peerId)
        {
            _peers.Remove(NormalizePeerId(peerId));
        }

        public void ForceFallback(string reason)
        {
            _forcedFallback = true;
            _forcedFallbackReason = string.IsNullOrEmpty(reason)
                ? "forced-fallback"
                : reason;
        }

        public void Reset()
        {
            _peers.Clear();
            _sessionActive = false;
            _forcedFallback = false;
            _forcedFallbackReason = null;
            _raidId = 0;
        }

        public CompatibilitySnapshot Evaluate(double now)
        {
            if (!_sessionActive)
            {
                return CreateSnapshot(
                    CompatibilityStatus.Inactive,
                    "co-op-session-inactive",
                    now);
            }

            if (_forcedFallback)
            {
                return CreateSnapshot(
                    CompatibilityStatus.Fallback,
                    _forcedFallbackReason ?? "forced-fallback",
                    now);
            }

            if (_peers.Count == 0)
            {
                return CreateSnapshot(
                    CompatibilityStatus.AwaitingPeers,
                    "no-connected-peers",
                    now);
            }

            bool anyPending = false;
            foreach (PeerState peer in _peers.Values)
            {
                if (peer.ProtocolVersion.HasValue &&
                    peer.ProtocolVersion.Value != DownedReviveProtocol.CurrentVersion)
                {
                    ForceFallback("protocol-mismatch");
                    return CreateSnapshot(
                        CompatibilityStatus.Fallback,
                        _forcedFallbackReason,
                        now);
                }

                if (!peer.ProtocolVersion.HasValue)
                {
                    anyPending = true;
                    if (now - peer.ConnectedAt >= HandshakeTimeoutSeconds)
                    {
                        ForceFallback("handshake-timeout");
                        return CreateSnapshot(
                            CompatibilityStatus.Fallback,
                            _forcedFallbackReason,
                            now);
                    }
                }
            }

            if (anyPending)
            {
                return CreateSnapshot(
                    CompatibilityStatus.AwaitingHandshake,
                    "awaiting-peer-capability",
                    now);
            }

            return CreateSnapshot(
                CompatibilityStatus.Compatible,
                "all-peers-compatible",
                now);
        }

        private CompatibilitySnapshot CreateSnapshot(
            CompatibilityStatus status,
            string reason,
            double now)
        {
            List<PeerCompatibilitySnapshot> peers =
                new List<PeerCompatibilitySnapshot>(_peers.Count);
            foreach (PeerState peer in _peers.Values)
            {
                bool compatible = peer.ProtocolVersion.HasValue &&
                    peer.ProtocolVersion.Value ==
                    DownedReviveProtocol.CurrentVersion;
                double elapsed = now - peer.ConnectedAt;
                if (elapsed < 0)
                {
                    elapsed = 0;
                }

                peers.Add(new PeerCompatibilitySnapshot(
                    peer.PeerId,
                    true,
                    peer.ProtocolVersion,
                    compatible,
                    elapsed));
            }

            peers.Sort(delegate(PeerCompatibilitySnapshot left,
                PeerCompatibilitySnapshot right)
            {
                return string.CompareOrdinal(left.PeerId, right.PeerId);
            });

            return new CompatibilitySnapshot(
                status,
                reason,
                DownedReviveProtocol.CurrentVersion,
                _raidId,
                _sessionActive,
                peers.ToArray());
        }

        private static string NormalizePeerId(string peerId)
        {
            return string.IsNullOrEmpty(peerId) ? "unknown-peer" : peerId;
        }
    }
}
