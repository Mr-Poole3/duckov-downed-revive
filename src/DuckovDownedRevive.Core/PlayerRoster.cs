using System;
using System.Collections.Generic;

namespace DuckovDownedRevive.Core
{
    public sealed class PlayerRoster
    {
        private readonly Dictionary<string, bool> _players =
            new Dictionary<string, bool>(StringComparer.Ordinal);

        public string LocalPlayerId { get; private set; }

        public string[] PlayerIds
        {
            get
            {
                string[] ids = new string[_players.Count];
                _players.Keys.CopyTo(ids, 0);
                Array.Sort(ids, StringComparer.Ordinal);
                return ids;
            }
        }

        public void AddOrUpdate(string playerId, bool isLocal)
        {
            string normalized = Normalize(playerId, isLocal);
            _players[normalized] = true;
            if (isLocal)
            {
                LocalPlayerId = normalized;
            }
        }

        public void Remove(string playerId)
        {
            string normalized = Normalize(playerId, false);
            _players.Remove(normalized);
            if (string.Equals(LocalPlayerId, normalized, StringComparison.Ordinal))
            {
                LocalPlayerId = null;
            }
        }

        public bool Contains(string playerId)
        {
            return _players.ContainsKey(Normalize(playerId, false));
        }

        public int Count
        {
            get { return _players.Count; }
        }

        public void Clear()
        {
            _players.Clear();
            LocalPlayerId = null;
        }

        private static string Normalize(string playerId, bool isLocal)
        {
            if (!string.IsNullOrEmpty(playerId))
            {
                return playerId;
            }

            return isLocal ? "local-player" : "unknown-player";
        }
    }
}
