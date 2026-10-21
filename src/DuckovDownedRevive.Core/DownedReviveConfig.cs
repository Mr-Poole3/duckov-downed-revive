using System;

namespace DuckovDownedRevive.Core
{
    public sealed class DownedReviveConfig
    {
        public const int MinimumDowns = 0;
        public const int MaximumDowns = 10;
        public const float MinimumDownDurationSeconds = 1.0f;
        public const float MaximumDownDurationSeconds = 120.0f;
        public const float MinimumReviveHealthPercent = 1.0f;
        public const float MaximumReviveHealthPercent = 100.0f;

        public int MaxDownsPerPlayer;
        public float DownDurationSeconds;
        public bool DownedInvulnerable;
        public float ReviveHealthPercent;

        public static DownedReviveConfig CreateDefault()
        {
            return new DownedReviveConfig
            {
                MaxDownsPerPlayer = 1,
                DownDurationSeconds = 30.0f,
                DownedInvulnerable = true,
                ReviveHealthPercent = 10.0f
            };
        }

        public DownedReviveConfig Clone()
        {
            DownedReviveConfig clone = new DownedReviveConfig();
            clone.MaxDownsPerPlayer = MaxDownsPerPlayer;
            clone.DownDurationSeconds = DownDurationSeconds;
            clone.DownedInvulnerable = DownedInvulnerable;
            clone.ReviveHealthPercent = ReviveHealthPercent;
            return clone;
        }

        public DownedReviveConfig Normalize()
        {
            DownedReviveConfig normalized = new DownedReviveConfig();
            normalized.MaxDownsPerPlayer = Clamp(
                MaxDownsPerPlayer,
                MinimumDowns,
                MaximumDowns);
            normalized.DownDurationSeconds = Clamp(
                DownDurationSeconds,
                MinimumDownDurationSeconds,
                MaximumDownDurationSeconds);
            normalized.DownedInvulnerable = DownedInvulnerable;
            normalized.ReviveHealthPercent = Clamp(
                ReviveHealthPercent,
                MinimumReviveHealthPercent,
                MaximumReviveHealthPercent);
            return normalized;
        }

        public static bool ValuesEqual(DownedReviveConfig left, DownedReviveConfig right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            return left.MaxDownsPerPlayer == right.MaxDownsPerPlayer &&
                Math.Abs(left.DownDurationSeconds - right.DownDurationSeconds) < 0.001f &&
                left.DownedInvulnerable == right.DownedInvulnerable &&
                Math.Abs(left.ReviveHealthPercent - right.ReviveHealthPercent) < 0.001f;
        }

        private static int Clamp(int value, int minimum, int maximum)
        {
            if (value < minimum)
            {
                return minimum;
            }

            return value > maximum ? maximum : value;
        }

        private static float Clamp(float value, float minimum, float maximum)
        {
            if (float.IsNaN(value) || value < minimum)
            {
                return minimum;
            }

            return value > maximum ? maximum : value;
        }
    }
}
