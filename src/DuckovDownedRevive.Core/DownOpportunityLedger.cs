using System;
using System.Collections.Generic;

namespace DuckovDownedRevive.Core
{
    public sealed class DownOpportunityLedger
    {
        private readonly Dictionary<string, int> _remaining =
            new Dictionary<string, int>(StringComparer.Ordinal);

        private uint _raidId;
        private int _maximumPerPlayer;

        public bool IsActive { get; private set; }

        public uint RaidId
        {
            get { return _raidId; }
        }

        public bool BeginRaid(uint raidId, int maximumPerPlayer)
        {
            if (IsActive && _raidId == raidId)
            {
                return false;
            }

            _raidId = raidId;
            _maximumPerPlayer = maximumPerPlayer < 0 ? 0 : maximumPerPlayer;
            _remaining.Clear();
            IsActive = true;
            return true;
        }

        public void EndRaid()
        {
            IsActive = false;
            _remaining.Clear();
        }

        public void RegisterPlayer(string playerId)
        {
            string normalized = Normalize(playerId);
            if (_remaining.ContainsKey(normalized))
            {
                return;
            }

            _remaining.Add(normalized, _maximumPerPlayer);
        }

        public void RegisterPlayers(IEnumerable<string> playerIds)
        {
            if (playerIds == null)
            {
                return;
            }

            foreach (string playerId in playerIds)
            {
                RegisterPlayer(playerId);
            }
        }

        public bool TryConsume(string playerId, out int remaining)
        {
            string normalized = Normalize(playerId);
            if (!_remaining.TryGetValue(normalized, out remaining))
            {
                remaining = 0;
                return false;
            }

            if (remaining <= 0)
            {
                return false;
            }

            remaining--;
            _remaining[normalized] = remaining;
            return true;
        }

        public int Remaining(string playerId)
        {
            string normalized = Normalize(playerId);
            int remaining;
            return _remaining.TryGetValue(normalized, out remaining)
                ? remaining
                : 0;
        }

        private static string Normalize(string playerId)
        {
            return string.IsNullOrEmpty(playerId) ? "unknown-player" : playerId;
        }
    }
}
