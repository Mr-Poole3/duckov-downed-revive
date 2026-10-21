namespace DuckovDownedRevive.Core
{
    public enum PlayerLifeState
    {
        Alive = 0,
        Downed = 1,
        Dead = 2,
        Disconnected = 3
    }

    public enum DownRecordState
    {
        Countdown = 0,
        RescueActive = 1,
        Revived = 2,
        Finalized = 3
    }

    public enum DownEntryResult
    {
        Accepted = 0,
        RaidInactive = 1,
        PlayerUnavailable = 2,
        NoOpportunity = 3,
        NoTeammate = 4
    }

    public enum RescueStartResult
    {
        Accepted = 0,
        TargetNotDowned = 1,
        RescuerUnavailable = 2,
        SamePlayer = 3,
        OutOfRange = 4,
        AlreadyBeingRescued = 5
    }

    public enum DownTransitionKind
    {
        EnteredDown = 0,
        RescueStarted = 1,
        RescueInterrupted = 2,
        Revived = 3,
        Finalized = 4
    }

    public sealed class DownTransition
    {
        public DownTransition(
            DownTransitionKind kind,
            string playerId,
            string downId,
            string relatedPlayerId,
            string reason)
        {
            Kind = kind;
            PlayerId = playerId;
            DownId = downId;
            RelatedPlayerId = relatedPlayerId;
            Reason = reason;
        }

        public DownTransitionKind Kind { get; private set; }

        public string PlayerId { get; private set; }

        public string DownId { get; private set; }

        public string RelatedPlayerId { get; private set; }

        public string Reason { get; private set; }
    }

    public sealed class DownRecordSnapshot
    {
        public DownRecordSnapshot(
            string downId,
            string playerId,
            DownRecordState state,
            float remainingSeconds,
            string rescuerId,
            float rescueProgressSeconds,
            string terminalReason)
        {
            DownId = downId;
            PlayerId = playerId;
            State = state;
            RemainingSeconds = remainingSeconds;
            RescuerId = rescuerId;
            RescueProgressSeconds = rescueProgressSeconds;
            TerminalReason = terminalReason;
        }

        public string DownId { get; private set; }

        public string PlayerId { get; private set; }

        public DownRecordState State { get; private set; }

        public float RemainingSeconds { get; private set; }

        public string RescuerId { get; private set; }

        public float RescueProgressSeconds { get; private set; }

        public string TerminalReason { get; private set; }
    }
}
