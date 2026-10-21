using System;

namespace DuckovDownedRevive.Core
{
    public static class LethalDamageGuard
    {
        public const float HealthEpsilon = 0.0001f;

        public static bool ShouldGuard(
            float currentHealth,
            float incomingDamage)
        {
            if (currentHealth <= HealthEpsilon ||
                float.IsNaN(currentHealth) ||
                float.IsNaN(incomingDamage))
            {
                return false;
            }

            return incomingDamage > 0.0f &&
                incomingDamage + HealthEpsilon >= currentHealth;
        }

        public static float GuardedHealth(
            float currentHealth,
            float maximumHealth,
            float incomingDamage)
        {
            float damageScale = Math.Max(0.0f, incomingDamage) * 10.0f;
            float maximumScale = Math.Max(0.0f, maximumHealth) * 2.0f;
            return currentHealth + damageScale + maximumScale + 100.0f;
        }

        public static float ActualDamageApplied(
            float guardedHealth,
            float healthAfterHurt)
        {
            float damage = guardedHealth - healthAfterHurt;
            return damage > 0.0f ? damage : 0.0f;
        }

        public static bool IsLethal(
            float originalHealth,
            float actualDamage)
        {
            if (originalHealth <= 0.0f)
            {
                return true;
            }

            return actualDamage + HealthEpsilon >= originalHealth;
        }

        public static float RemainingHealth(
            float originalHealth,
            float actualDamage)
        {
            float remaining = originalHealth - actualDamage;
            return remaining > 0.0f ? remaining : 0.0f;
        }
    }
}
