namespace DuckovDownedRevive.Core
{
    public static class DownedReviveProtocol
    {
        public const int CurrentVersion = 2;
        public const string ChannelName = "duckov.downed-revive.v2";
        public const double DefaultHandshakeTimeoutSeconds = 10.0;
    }

    public enum CompatibilityStatus
    {
        Inactive = 0,
        AwaitingPeers = 1,
        AwaitingHandshake = 2,
        Compatible = 3,
        Fallback = 4
    }

    public sealed class PeerCompatibilitySnapshot
    {
        public PeerCompatibilitySnapshot(
            string peerId,
            bool connected,
            int? protocolVersion,
            bool compatible,
            double secondsSinceConnected)
        {
            PeerId = peerId;
            Connected = connected;
            ProtocolVersion = protocolVersion;
            Compatible = compatible;
            SecondsSinceConnected = secondsSinceConnected;
        }

        public string PeerId { get; private set; }

        public bool Connected { get; private set; }

        public int? ProtocolVersion { get; private set; }

        public bool Compatible { get; private set; }

        public double SecondsSinceConnected { get; private set; }
    }

    public sealed class CompatibilitySnapshot
    {
        public CompatibilitySnapshot(
            CompatibilityStatus status,
            string reason,
            int localProtocolVersion,
            uint raidId,
            bool sessionActive,
            PeerCompatibilitySnapshot[] peers)
        {
            Status = status;
            Reason = reason;
            LocalProtocolVersion = localProtocolVersion;
            RaidId = raidId;
            SessionActive = sessionActive;
            Peers = peers;
        }

        public CompatibilityStatus Status { get; private set; }

        public string Reason { get; private set; }

        public int LocalProtocolVersion { get; private set; }

        public uint RaidId { get; private set; }

        public bool SessionActive { get; private set; }

        public PeerCompatibilitySnapshot[] Peers { get; private set; }
    }
}
