using System;
using System.Collections.Generic;
using DuckovDownedRevive.Core;

namespace DuckovDownedRevive.Tests
{
    internal static class Program
    {
        private static int _assertions;
        private static readonly List<string> Failures = new List<string>();

        private static int Main()
        {
            Run("session inactive", SessionInactive);
            Run("matching capabilities", MatchingCapabilities);
            Run("pending handshake", PendingHandshake);
            Run("handshake timeout", HandshakeTimeout);
            Run("protocol mismatch", ProtocolMismatch);
            Run("protocol mismatch remains sticky", ProtocolMismatchRemainsSticky);
            Run("peer disconnect", PeerDisconnect);
            Run("late peer fallback", LatePeerFallback);
            Run("duplicate messages", DuplicateMessages);
            Run("diagnostic formatting", DiagnosticFormatting);
            Run("configuration defaults and clamping", ConfigurationDefaultsAndClamping);
            Run("configuration changes apply next raid", ConfigurationChangesApplyNextRaid);
            Run("independent down opportunities", IndependentDownOpportunities);
            Run("raid reset boundaries", RaidResetBoundaries);
            Run("player roster identity", PlayerRosterIdentity);
            Run("down entry consumes opportunity", DownEntryConsumesOpportunity);
            Run("down without teammate", DownWithoutTeammate);
            Run("down countdown timeout", DownCountdownTimeout);
            Run("rescue pauses and resumes countdown", RescuePausesAndResumesCountdown);
            Run("rescue completes revival", RescueCompletesRevival);
            Run("rescue ownership is exclusive", RescueOwnershipIsExclusive);
            Run("last rescuer downed finalizes target", LastRescuerDownedFinalizesTarget);
            Run("downed disconnect finalizes", DownedDisconnectFinalizes);
            Run("observed non-lethal hit stays alive", ObservedNonLethalHitStaysAlive);
            Run("observed lethal hit is confirmed", ObservedLethalHitIsConfirmed);
            Run("already dead health is ignored", AlreadyDeadHealthIsIgnored);

            Console.WriteLine("Assertions: " + _assertions);
            if (Failures.Count == 0)
            {
                Console.WriteLine("RESULT: PASS");
                return 0;
            }

            Console.WriteLine("RESULT: FAIL");
            for (int index = 0; index < Failures.Count; index++)
            {
                Console.WriteLine(" - " + Failures[index]);
            }

            return 1;
        }

        private static void SessionInactive()
        {
            CompatibilityTracker tracker = new CompatibilityTracker();
            CompatibilitySnapshot snapshot = tracker.Evaluate(1);
            Equal(CompatibilityStatus.Inactive, snapshot.Status, "status");
            Equal("co-op-session-inactive", snapshot.Reason, "reason");
        }

        private static void MatchingCapabilities()
        {
            CompatibilityTracker tracker = NewActiveTracker();
            tracker.RegisterPeer("host", 1);
            tracker.RegisterPeer("client-a", 1);
            tracker.RegisterCapability("host", DownedReviveProtocol.CurrentVersion, 2);
            tracker.RegisterCapability("client-a", DownedReviveProtocol.CurrentVersion, 2);

            CompatibilitySnapshot snapshot = tracker.Evaluate(3);
            Equal(CompatibilityStatus.Compatible, snapshot.Status, "status");
            Equal(2, snapshot.Peers.Length, "peer count");
            True(snapshot.Peers[0].Compatible, "peer sorted and compatible");
        }

        private static void PendingHandshake()
        {
            CompatibilityTracker tracker = NewActiveTracker();
            tracker.RegisterPeer("client-a", 1);
            CompatibilitySnapshot snapshot = tracker.Evaluate(5);
            Equal(CompatibilityStatus.AwaitingHandshake, snapshot.Status, "status");
            Equal("awaiting-peer-capability", snapshot.Reason, "reason");
        }

        private static void HandshakeTimeout()
        {
            CompatibilityTracker tracker = NewActiveTracker();
            tracker.HandshakeTimeoutSeconds = 5;
            tracker.RegisterPeer("client-a", 1);
            CompatibilitySnapshot snapshot = tracker.Evaluate(6);
            Equal(CompatibilityStatus.Fallback, snapshot.Status, "status");
            Equal("handshake-timeout", snapshot.Reason, "reason");
        }

        private static void ProtocolMismatch()
        {
            CompatibilityTracker tracker = NewActiveTracker();
            tracker.RegisterPeer("client-a", 1);
            tracker.RegisterCapability(
                "client-a",
                DownedReviveProtocol.CurrentVersion + 1,
                2);
            CompatibilitySnapshot snapshot = tracker.Evaluate(3);
            Equal(CompatibilityStatus.Fallback, snapshot.Status, "status");
            Equal("protocol-mismatch", snapshot.Reason, "reason");
        }

        private static void ProtocolMismatchRemainsSticky()
        {
            CompatibilityTracker tracker = NewActiveTracker();
            tracker.RegisterPeer("client-a", 1);
            tracker.RegisterPeer("client-b", 1);
            tracker.RegisterCapability("client-a", DownedReviveProtocol.CurrentVersion, 2);
            tracker.RegisterCapability("client-b", 99, 2);
            Equal(CompatibilityStatus.Fallback, tracker.Evaluate(3).Status, "mismatch");

            tracker.DisconnectPeer("client-b");
            CompatibilitySnapshot snapshot = tracker.Evaluate(4);
            Equal(CompatibilityStatus.Fallback, snapshot.Status, "sticky fallback");
            Equal("protocol-mismatch", snapshot.Reason, "sticky reason");
        }

        private static void PeerDisconnect()
        {
            CompatibilityTracker tracker = NewActiveTracker();
            tracker.RegisterPeer("client-a", 1);
            tracker.RegisterCapability("client-a", DownedReviveProtocol.CurrentVersion, 2);
            tracker.DisconnectPeer("client-a");
            CompatibilitySnapshot snapshot = tracker.Evaluate(3);
            Equal(CompatibilityStatus.AwaitingPeers, snapshot.Status, "status");
            Equal(0, snapshot.Peers.Length, "peer count");
        }

        private static void LatePeerFallback()
        {
            CompatibilityTracker tracker = NewActiveTracker();
            tracker.RegisterPeer("client-a", 1);
            tracker.RegisterCapability("client-a", DownedReviveProtocol.CurrentVersion, 2);
            Equal(CompatibilityStatus.Compatible, tracker.Evaluate(3).Status, "before join");

            tracker.RegisterPeer("late-client", 4);
            tracker.RegisterCapability("late-client", 7, 5);
            Equal(CompatibilityStatus.Fallback, tracker.Evaluate(6).Status, "after join");
        }

        private static void DuplicateMessages()
        {
            CompatibilityTracker tracker = NewActiveTracker();
            tracker.RegisterPeer("client-a", 1);
            tracker.RegisterPeer("client-a", 4);
            tracker.RegisterCapability("client-a", DownedReviveProtocol.CurrentVersion, 2);
            tracker.RegisterCapability("client-a", DownedReviveProtocol.CurrentVersion, 3);
            CompatibilitySnapshot snapshot = tracker.Evaluate(5);
            Equal(CompatibilityStatus.Compatible, snapshot.Status, "status");
            Equal(1, snapshot.Peers.Length, "deduplicated peer count");
            Equal(4.0, snapshot.Peers[0].SecondsSinceConnected, "deadline not extended");
        }

        private static void DiagnosticFormatting()
        {
            CompatibilityTracker tracker = NewActiveTracker();
            tracker.SetRaid(42);
            tracker.RegisterPeer("client-a", 1);
            tracker.RegisterCapability("client-a", DownedReviveProtocol.CurrentVersion, 2);
            string text = CompatibilityDiagnosticsFormatter.Format(tracker.Evaluate(3));
            Contains(text, "status=Compatible", "status text");
            Contains(text, "protocol=2", "protocol text");
            Contains(text, "raid=42", "raid text");
            Contains(text, "client-a: compatible", "peer text");
        }

        private static void ConfigurationDefaultsAndClamping()
        {
            DownedReviveConfig config = DownedReviveConfig.CreateDefault();
            Equal(1, config.MaxDownsPerPlayer, "default downs");
            Equal(30.0f, config.DownDurationSeconds, "default duration");
            Equal(true, config.DownedInvulnerable, "default invulnerability");
            Equal(10.0f, config.ReviveHealthPercent, "default revive health");

            config.MaxDownsPerPlayer = -5;
            config.DownDurationSeconds = float.NaN;
            config.ReviveHealthPercent = 500;
            DownedReviveConfig normalized = config.Normalize();
            Equal(0, normalized.MaxDownsPerPlayer, "clamped downs");
            Equal(1.0f, normalized.DownDurationSeconds, "clamped duration");
            Equal(100.0f, normalized.ReviveHealthPercent, "clamped revive health");
        }

        private static void ConfigurationChangesApplyNextRaid()
        {
            HostConfigSession session = new HostConfigSession();
            DownedReviveConfig original = session.Pending;
            DownedReviveConfig locked = session.LockForRaid();
            Equal(1, locked.MaxDownsPerPlayer, "locked defaults");

            DownedReviveConfig changed = session.Pending;
            changed.MaxDownsPerPlayer = 2;
            session.SetPending(changed);
            Equal(1, session.EffectiveForCurrentRaid().MaxDownsPerPlayer,
                "active raid remains locked");

            session.EndRaid();
            session.LockForRaid();
            Equal(2, session.EffectiveForCurrentRaid().MaxDownsPerPlayer,
                "next raid applies pending");
            True(original != session.Pending, "pending returns a copy");
        }

        private static void IndependentDownOpportunities()
        {
            DownOpportunityLedger ledger = new DownOpportunityLedger();
            ledger.BeginRaid(9, 2);
            ledger.RegisterPlayer("host");
            ledger.RegisterPlayer("client-a");

            int remaining;
            True(ledger.TryConsume("host", out remaining), "host first down");
            Equal(1, remaining, "host remaining");
            Equal(2, ledger.Remaining("client-a"), "client independent");
            True(ledger.TryConsume("host", out remaining), "host second down");
            True(!ledger.TryConsume("host", out remaining), "host limit");
            Equal(0, remaining, "host exhausted");
        }

        private static void RaidResetBoundaries()
        {
            DownOpportunityLedger ledger = new DownOpportunityLedger();
            True(ledger.BeginRaid(10, 1), "first raid begins");
            ledger.RegisterPlayer("host");
            int remaining;
            True(ledger.TryConsume("host", out remaining), "down consumed");
            True(!ledger.BeginRaid(10, 1), "same raid does not reset");
            Equal(0, ledger.Remaining("host"), "same raid remains consumed");

            True(ledger.BeginRaid(11, 1), "new raid begins");
            ledger.RegisterPlayer("host");
            Equal(1, ledger.Remaining("host"), "new raid resets");
        }

        private static void PlayerRosterIdentity()
        {
            PlayerRoster roster = new PlayerRoster();
            roster.AddOrUpdate("host", true);
            roster.AddOrUpdate("client-a", false);
            Equal("host", roster.LocalPlayerId, "local id");
            Equal(2, roster.Count, "roster count");
            True(roster.Contains("client-a"), "contains client");
            roster.Remove("host");
            True(roster.LocalPlayerId == null, "local removed");
        }

        private static DownedReviveSession NewLiveSession()
        {
            DownedReviveConfig config = DownedReviveConfig.CreateDefault();
            DownedReviveSession session = new DownedReviveSession();
            session.StartRaid(
                77,
                config,
                new string[] { "host", "client-a", "client-b" },
                1);
            return session;
        }

        private static void DownEntryConsumesOpportunity()
        {
            DownedReviveSession session = NewLiveSession();
            DownRecordSnapshot snapshot;
            DownEntryResult result = session.TryEnterDown("host", 2, out snapshot);
            Equal(DownEntryResult.Accepted, result, "entry result");
            Equal(PlayerLifeState.Downed, session.GetLifeState("host"), "life state");
            Equal(0, session.RemainingDowns("host"), "opportunity consumed");
            Equal(DownRecordState.Countdown, snapshot.State, "record state");
            Equal(30.0f, snapshot.RemainingSeconds, "initial duration");
        }

        private static void DownWithoutTeammate()
        {
            DownedReviveConfig config = DownedReviveConfig.CreateDefault();
            DownedReviveSession session = new DownedReviveSession();
            session.StartRaid(78, config, new string[] { "host" }, 1);
            DownRecordSnapshot snapshot;
            DownEntryResult result = session.TryEnterDown("host", 2, out snapshot);
            Equal(DownEntryResult.NoTeammate, result, "entry result");
            Equal(PlayerLifeState.Alive, session.GetLifeState("host"), "still alive");
            Equal(1, session.RemainingDowns("host"), "not consumed");
        }

        private static void DownCountdownTimeout()
        {
            DownedReviveSession session = NewLiveSession();
            DownRecordSnapshot snapshot;
            session.TryEnterDown("host", 2, out snapshot);
            session.Tick(32.1);
            Equal(DownRecordState.Finalized, session.GetDown("host").State, "state");
            Equal(PlayerLifeState.Dead, session.GetLifeState("host"), "player dead");
            Equal("timeout", session.GetDown("host").TerminalReason, "reason");
        }

        private static void RescuePausesAndResumesCountdown()
        {
            DownedReviveSession session = NewLiveSession();
            DownRecordSnapshot snapshot;
            session.TryEnterDown("host", 2, out snapshot);
            session.TryStartRescue("host", "client-a", 1.5f, 3);
            session.CancelRescue("host", "client-a", "movement-input", 5);
            session.Tick(7);

            DownRecordSnapshot resumed = session.GetDown("host");
            Equal(27.0f, resumed.RemainingSeconds, "countdown resumes from 27s");
            Equal(0.0f, resumed.RescueProgressSeconds, "progress reset");
            True(resumed.RescuerId == null, "owner cleared");
        }

        private static void RescueCompletesRevival()
        {
            DownedReviveSession session = NewLiveSession();
            DownRecordSnapshot snapshot;
            session.TryEnterDown("host", 2, out snapshot);
            Equal(RescueStartResult.Accepted,
                session.TryStartRescue("host", "client-a", 2.0f, 3),
                "rescue starts");
            session.Tick(6.01);
            Equal(DownRecordState.Revived, session.GetDown("host").State, "state");
            Equal(PlayerLifeState.Alive, session.GetLifeState("host"), "alive");
            Equal(3.0f, session.GetDown("host").RescueProgressSeconds, "progress");
        }

        private static void RescueOwnershipIsExclusive()
        {
            DownedReviveSession session = NewLiveSession();
            DownRecordSnapshot snapshot;
            session.TryEnterDown("host", 2, out snapshot);
            Equal(RescueStartResult.Accepted,
                session.TryStartRescue("host", "client-a", 2.0f, 3),
                "first rescuer");
            Equal(RescueStartResult.AlreadyBeingRescued,
                session.TryStartRescue("host", "client-b", 2.0f, 3.5),
                "second rescuer rejected");
        }

        private static void LastRescuerDownedFinalizesTarget()
        {
            DownedReviveConfig config = DownedReviveConfig.CreateDefault();
            DownedReviveSession session = new DownedReviveSession();
            session.StartRaid(79, config, new string[] { "host", "client-a" }, 1);

            DownRecordSnapshot snapshot;
            session.TryEnterDown("host", 2, out snapshot);
            Equal(RescueStartResult.Accepted,
                session.TryStartRescue("host", "client-a", 2.0f, 3),
                "rescue starts");
            session.MarkDead("client-a", 4, "combat-death");
            Equal(DownRecordState.Finalized, session.GetDown("host").State,
                "target finalized");
            Equal("last-rescuer-dead",
                session.GetDown("host").TerminalReason,
                "reason");
        }

        private static void DownedDisconnectFinalizes()
        {
            DownedReviveSession session = NewLiveSession();
            DownRecordSnapshot snapshot;
            session.TryEnterDown("host", 2, out snapshot);
            session.MarkDisconnected("host", 3);
            Equal(DownRecordState.Finalized, session.GetDown("host").State, "state");
            Equal(PlayerLifeState.Disconnected,
                session.GetLifeState("host"),
                "disconnect state");
            Equal("downed-player-disconnected",
                session.GetDown("host").TerminalReason,
                "reason");
        }

        private static void ObservedNonLethalHitStaysAlive()
        {
            float original = 10.11f;
            float incoming = 11.79f;
            True(LethalDamageGuard.ShouldGuard(original, incoming),
                "candidate should be guarded");
            float guarded = LethalDamageGuard.GuardedHealth(
                original,
                45.0f,
                incoming);
            float actualDamage = LethalDamageGuard.ActualDamageApplied(
                guarded,
                guarded - 9.43f);
            True(!LethalDamageGuard.IsLethal(original, actualDamage),
                "observed damage was not lethal");
            Near(0.68f,
                LethalDamageGuard.RemainingHealth(original, actualDamage),
                0.001f,
                "remaining health");
        }

        private static void ObservedLethalHitIsConfirmed()
        {
            float original = 0.68f;
            float incoming = 11.79f;
            True(LethalDamageGuard.ShouldGuard(original, incoming),
                "candidate should be guarded");
            float guarded = LethalDamageGuard.GuardedHealth(
                original,
                45.0f,
                incoming);
            float actualDamage = LethalDamageGuard.ActualDamageApplied(
                guarded,
                guarded - 9.43f);
            True(LethalDamageGuard.IsLethal(original, actualDamage),
                "observed damage was lethal");
        }

        private static void AlreadyDeadHealthIsIgnored()
        {
            True(!LethalDamageGuard.ShouldGuard(0.0f, 11.79f),
                "dead health cannot enter down twice");
        }

        private static CompatibilityTracker NewActiveTracker()
        {
            CompatibilityTracker tracker = new CompatibilityTracker();
            tracker.SetSessionActive(true);
            return tracker;
        }

        private static void Run(string name, Action test)
        {
            try
            {
                test();
                Console.WriteLine("PASS " + name);
            }
            catch (Exception exception)
            {
                Failures.Add(name + ": " + exception.Message);
                Console.WriteLine("FAIL " + name + ": " + exception.Message);
            }
        }

        private static void Equal<T>(T expected, T actual, string label)
        {
            _assertions++;
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
            {
                throw new InvalidOperationException(
                    label + " expected <" + expected + "> but was <" + actual + ">");
            }
        }

        private static void True(bool condition, string label)
        {
            _assertions++;
            if (!condition)
            {
                throw new InvalidOperationException(label + " was false");
            }
        }

        private static void Contains(string text, string expected, string label)
        {
            _assertions++;
            if (text == null || text.IndexOf(expected, StringComparison.Ordinal) < 0)
            {
                throw new InvalidOperationException(
                    label + " did not contain <" + expected + ">");
            }
        }

        private static void Near(
            float expected,
            float actual,
            float tolerance,
            string label)
        {
            _assertions++;
            if (Math.Abs(expected - actual) > tolerance)
            {
                throw new InvalidOperationException(
                    label + " expected <" + expected + "> but was <" + actual + ">");
            }
        }
    }
}
