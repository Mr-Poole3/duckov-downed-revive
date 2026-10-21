using System;
using System.Collections.Generic;

namespace DuckovDownedRevive.Core
{
    public sealed class DownedReviveSession
    {
        private sealed class PlayerRuntime
        {
            public PlayerLifeState LifeState;
        }

        private sealed class DownRecord
        {
            public string DownId;
            public string PlayerId;
            public DownRecordState State;
            public float RemainingSeconds;
            public string RescuerId;
            public float RescueProgressSeconds;
            public float LastUpdatedAt;
            public string TerminalReason;
        }

        private readonly Dictionary<string, PlayerRuntime> _players =
            new Dictionary<string, PlayerRuntime>(StringComparer.Ordinal);
        private readonly Dictionary<string, DownRecord> _downs =
            new Dictionary<string, DownRecord>(StringComparer.Ordinal);
        private readonly List<DownTransition> _transitions =
            new List<DownTransition>();

        private DownedReviveConfig _config;
        private DownOpportunityLedger _ledger;
        private uint _raidId;
        private int _nextDownSequence;

        public DownedReviveSession()
        {
            _config = DownedReviveConfig.CreateDefault();
            _ledger = new DownOpportunityLedger();
        }

        public bool IsRaidActive { get; private set; }

        public uint RaidId
        {
            get { return _raidId; }
        }

        public DownedReviveConfig Config
        {
            get { return _config.Clone(); }
        }

        public void StartRaid(
            uint raidId,
            DownedReviveConfig config,
            IEnumerable<string> playerIds,
            double now)
        {
            _config = config == null
                ? DownedReviveConfig.CreateDefault()
                : config.Normalize();
            _raidId = raidId;
            _nextDownSequence = 0;
            _players.Clear();
            _downs.Clear();
            _transitions.Clear();
            _ledger = new DownOpportunityLedger();
            _ledger.BeginRaid(raidId, _config.MaxDownsPerPlayer);
            IsRaidActive = true;

            if (playerIds != null)
            {
                foreach (string playerId in playerIds)
                {
                    AddPlayer(playerId);
                }
            }
        }

        public void EndRaid()
        {
            IsRaidActive = false;
            _raidId = 0;
            _players.Clear();
            _downs.Clear();
            _transitions.Clear();
            _ledger.EndRaid();
        }

        public bool AddPlayer(string playerId)
        {
            if (!IsRaidActive || string.IsNullOrEmpty(playerId))
            {
                return false;
            }

            if (_players.ContainsKey(playerId))
            {
                return false;
            }

            _players.Add(playerId, new PlayerRuntime
            {
                LifeState = PlayerLifeState.Alive
            });
            _ledger.RegisterPlayer(playerId);
            return true;
        }

        public PlayerLifeState GetLifeState(string playerId)
        {
            PlayerRuntime state;
            return _players.TryGetValue(Normalize(playerId), out state)
                ? state.LifeState
                : PlayerLifeState.Disconnected;
        }

        public int RemainingDowns(string playerId)
        {
            return _ledger.Remaining(Normalize(playerId));
        }

        public DownEntryResult TryEnterDown(
            string playerId,
            double now,
            out DownRecordSnapshot snapshot)
        {
            Tick(now);
            snapshot = null;

            string normalized = Normalize(playerId);
            DownEntryResult eligibility = CanEnterDown(normalized);
            if (eligibility != DownEntryResult.Accepted)
            {
                return eligibility;
            }

            PlayerRuntime player;
            _players.TryGetValue(normalized, out player);

            int remaining;
            if (!_ledger.TryConsume(normalized, out remaining))
            {
                return DownEntryResult.NoOpportunity;
            }

            player.LifeState = PlayerLifeState.Downed;
            CancelRescuesOwnedBy(normalized, now, "rescuer-downed");

            _nextDownSequence++;
            DownRecord record = new DownRecord();
            record.DownId = normalized + ":" + _nextDownSequence;
            record.PlayerId = normalized;
            record.State = DownRecordState.Countdown;
            record.RemainingSeconds = _config.DownDurationSeconds;
            record.LastUpdatedAt = (float)now;
            _downs.Add(normalized, record);

            _transitions.Add(new DownTransition(
                DownTransitionKind.EnteredDown,
                normalized,
                record.DownId,
                null,
                "eligible-damage"));

            FinalizeUnrescuableDowns(now, "last-rescuer-downed");
            snapshot = Snapshot(record);
            return DownEntryResult.Accepted;
        }

        public DownEntryResult CanEnterDown(string playerId)
        {
            if (!IsRaidActive)
            {
                return DownEntryResult.RaidInactive;
            }

            string normalized = Normalize(playerId);
            PlayerRuntime player;
            if (!_players.TryGetValue(normalized, out player) ||
                player.LifeState != PlayerLifeState.Alive ||
                _downs.ContainsKey(normalized))
            {
                return DownEntryResult.PlayerUnavailable;
            }

            if (!HasLivingTeammate(normalized))
            {
                return DownEntryResult.NoTeammate;
            }

            return _ledger.Remaining(normalized) > 0
                ? DownEntryResult.Accepted
                : DownEntryResult.NoOpportunity;
        }

        public DownRecordSnapshot GetDown(string playerId)
        {
            DownRecord record;
            return _downs.TryGetValue(Normalize(playerId), out record)
                ? Snapshot(record)
                : null;
        }

        public DownTransition[] DrainTransitions()
        {
            DownTransition[] transitions = _transitions.ToArray();
            _transitions.Clear();
            return transitions;
        }

        public RescueStartResult TryStartRescue(
            string targetPlayerId,
            string rescuerPlayerId,
            float distanceMeters,
            double now)
        {
            Tick(now);

            string target = Normalize(targetPlayerId);
            string rescuer = Normalize(rescuerPlayerId);
            if (string.Equals(target, rescuer, StringComparison.Ordinal))
            {
                return RescueStartResult.SamePlayer;
            }

            PlayerRuntime rescuerRuntime;
            if (!_players.TryGetValue(rescuer, out rescuerRuntime) ||
                rescuerRuntime.LifeState != PlayerLifeState.Alive)
            {
                return RescueStartResult.RescuerUnavailable;
            }

            DownRecord targetRecord;
            if (!_downs.TryGetValue(target, out targetRecord) ||
                (targetRecord.State != DownRecordState.Countdown &&
                 targetRecord.State != DownRecordState.RescueActive))
            {
                return RescueStartResult.TargetNotDowned;
            }

            if (distanceMeters > 2.5f)
            {
                return RescueStartResult.OutOfRange;
            }

            if (!string.IsNullOrEmpty(targetRecord.RescuerId))
            {
                return RescueStartResult.AlreadyBeingRescued;
            }

            CancelRescuesOwnedBy(rescuer, now, "rescuer-switched-target");

            targetRecord.State = DownRecordState.RescueActive;
            targetRecord.RescuerId = rescuer;
            targetRecord.RescueProgressSeconds = 0.0f;
            targetRecord.LastUpdatedAt = (float)now;
            _transitions.Add(new DownTransition(
                DownTransitionKind.RescueStarted,
                target,
                targetRecord.DownId,
                rescuer,
                "valid-rescue"));
            return RescueStartResult.Accepted;
        }

        public void NotifyMovementInput(string rescuerPlayerId, double now)
        {
            Tick(now);
            CancelRescuesOwnedBy(
                Normalize(rescuerPlayerId),
                now,
                "movement-input");
        }

        public void UpdateRescueDistance(
            string targetPlayerId,
            string rescuerPlayerId,
            float distanceMeters,
            double now)
        {
            Tick(now);
            if (distanceMeters <= 2.5f)
            {
                return;
            }

            DownRecord target;
            if (_downs.TryGetValue(Normalize(targetPlayerId), out target) &&
                string.Equals(
                    target.RescuerId,
                    Normalize(rescuerPlayerId),
                    StringComparison.Ordinal))
            {
                CancelRescueInternal(target, now, "out-of-range");
            }
        }

        public void CancelRescue(
            string targetPlayerId,
            string rescuerPlayerId,
            string reason,
            double now)
        {
            Tick(now);
            DownRecord target;
            if (_downs.TryGetValue(Normalize(targetPlayerId), out target) &&
                string.Equals(
                    target.RescuerId,
                    Normalize(rescuerPlayerId),
                    StringComparison.Ordinal))
            {
                CancelRescueInternal(target, now, reason);
            }
        }

        public void MarkDead(string playerId, double now, string reason)
        {
            Tick(now);
            string normalized = Normalize(playerId);
            CancelRescuesOwnedBy(normalized, now, reason);

            DownRecord record;
            if (_downs.TryGetValue(normalized, out record) &&
                (record.State == DownRecordState.Countdown ||
                 record.State == DownRecordState.RescueActive))
            {
                FinalizeRecord(record, reason);
            }

            PlayerRuntime player;
            if (_players.TryGetValue(normalized, out player))
            {
                player.LifeState = PlayerLifeState.Dead;
            }

            FinalizeUnrescuableDowns(now, "last-rescuer-dead");
        }

        public void MarkDisconnected(string playerId, double now)
        {
            Tick(now);
            string normalized = Normalize(playerId);
            CancelRescuesOwnedBy(normalized, now, "rescuer-disconnected");

            DownRecord record;
            if (_downs.TryGetValue(normalized, out record) &&
                (record.State == DownRecordState.Countdown ||
                 record.State == DownRecordState.RescueActive))
            {
                FinalizeRecord(record, "downed-player-disconnected");
            }

            PlayerRuntime player;
            if (_players.TryGetValue(normalized, out player))
            {
                player.LifeState = PlayerLifeState.Disconnected;
            }

            FinalizeUnrescuableDowns(now, "last-rescuer-disconnected");
        }

        public void Tick(double now)
        {
            if (!IsRaidActive)
            {
                return;
            }

            List<DownRecord> records = new List<DownRecord>(_downs.Values);
            foreach (DownRecord record in records)
            {
                if (record.State != DownRecordState.Countdown &&
                    record.State != DownRecordState.RescueActive)
                {
                    continue;
                }

                float delta = (float)now - record.LastUpdatedAt;
                if (delta <= 0.0f)
                {
                    continue;
                }

                record.LastUpdatedAt = (float)now;
                if (record.State == DownRecordState.RescueActive)
                {
                    record.RescueProgressSeconds += delta;
                    if (record.RescueProgressSeconds >= 3.0f)
                    {
                        ReviveRecord(record);
                    }
                }
                else
                {
                    record.RemainingSeconds -= delta;
                    if (record.RemainingSeconds <= 0.0f)
                    {
                        record.RemainingSeconds = 0.0f;
                        FinalizeRecord(record, "timeout");
                    }
                }
            }
        }

        private bool HasLivingTeammate(string playerId)
        {
            foreach (KeyValuePair<string, PlayerRuntime> pair in _players)
            {
                if (!string.Equals(pair.Key, playerId, StringComparison.Ordinal) &&
                    pair.Value.LifeState == PlayerLifeState.Alive)
                {
                    return true;
                }
            }

            return false;
        }

        private void FinalizeUnrescuableDowns(double now, string reason)
        {
            List<DownRecord> records = new List<DownRecord>(_downs.Values);
            foreach (DownRecord record in records)
            {
                if ((record.State == DownRecordState.Countdown ||
                     record.State == DownRecordState.RescueActive) &&
                    !HasLivingTeammate(record.PlayerId))
                {
                    FinalizeRecord(record, reason);
                }
            }
        }

        private void CancelRescuesOwnedBy(
            string rescuerPlayerId,
            double now,
            string reason)
        {
            List<DownRecord> records = new List<DownRecord>(_downs.Values);
            foreach (DownRecord record in records)
            {
                if (string.Equals(
                    record.RescuerId,
                    rescuerPlayerId,
                    StringComparison.Ordinal))
                {
                    CancelRescueInternal(record, now, reason);
                }
            }
        }

        private void CancelRescueInternal(
            DownRecord record,
            double now,
            string reason)
        {
            string rescuer = record.RescuerId;
            record.State = DownRecordState.Countdown;
            record.RescuerId = null;
            record.RescueProgressSeconds = 0.0f;
            record.LastUpdatedAt = (float)now;
            _transitions.Add(new DownTransition(
                DownTransitionKind.RescueInterrupted,
                record.PlayerId,
                record.DownId,
                rescuer,
                reason));
        }

        private void ReviveRecord(DownRecord record)
        {
            record.State = DownRecordState.Revived;
            record.RescuerId = null;
            record.RescueProgressSeconds = 3.0f;

            PlayerRuntime player;
            if (_players.TryGetValue(record.PlayerId, out player))
            {
                player.LifeState = PlayerLifeState.Alive;
            }

            _transitions.Add(new DownTransition(
                DownTransitionKind.Revived,
                record.PlayerId,
                record.DownId,
                null,
                "rescue-complete"));
        }

        private void FinalizeRecord(DownRecord record, string reason)
        {
            if (record.State == DownRecordState.Finalized ||
                record.State == DownRecordState.Revived)
            {
                return;
            }

            record.State = DownRecordState.Finalized;
            record.RescuerId = null;
            record.RescueProgressSeconds = 0.0f;
            record.TerminalReason = reason;

            PlayerRuntime player;
            if (_players.TryGetValue(record.PlayerId, out player))
            {
                player.LifeState = PlayerLifeState.Dead;
            }

            _transitions.Add(new DownTransition(
                DownTransitionKind.Finalized,
                record.PlayerId,
                record.DownId,
                null,
                reason));
        }

        private static DownRecordSnapshot Snapshot(DownRecord record)
        {
            return new DownRecordSnapshot(
                record.DownId,
                record.PlayerId,
                record.State,
                record.RemainingSeconds,
                record.RescuerId,
                record.RescueProgressSeconds,
                record.TerminalReason);
        }

        private static string Normalize(string playerId)
        {
            return string.IsNullOrEmpty(playerId) ? "unknown-player" : playerId;
        }
    }
}
