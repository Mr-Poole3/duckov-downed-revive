using System.Text;

namespace DuckovDownedRevive.Core
{
    public static class CompatibilityDiagnosticsFormatter
    {
        public static string Format(CompatibilitySnapshot snapshot)
        {
            if (snapshot == null)
            {
                return "Downed Revive diagnostics unavailable.";
            }

            StringBuilder builder = new StringBuilder();
            builder.Append("Downed Revive | status=");
            builder.Append(snapshot.Status);
            builder.Append(" | protocol=");
            builder.Append(snapshot.LocalProtocolVersion);
            builder.Append(" | raid=");
            builder.Append(snapshot.RaidId == 0
                ? "none"
                : snapshot.RaidId.ToString());
            builder.Append(" | reason=");
            builder.Append(snapshot.Reason);
            builder.Append(" | peers=");
            builder.Append(snapshot.Peers.Length);

            for (int index = 0; index < snapshot.Peers.Length; index++)
            {
                PeerCompatibilitySnapshot peer = snapshot.Peers[index];
                builder.Append("\n");
                builder.Append(peer.PeerId);
                builder.Append(": ");
                builder.Append(peer.Compatible ? "compatible" : "incompatible");
                builder.Append(", version=");
                builder.Append(peer.ProtocolVersion.HasValue
                    ? peer.ProtocolVersion.Value.ToString()
                    : "pending");
                builder.Append(", waited=");
                builder.Append(peer.SecondsSinceConnected.ToString("0.0"));
                builder.Append("s");
            }

            return builder.ToString();
        }
    }
}
