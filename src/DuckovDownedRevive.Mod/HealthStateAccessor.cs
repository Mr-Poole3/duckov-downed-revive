using System.Reflection;

namespace DuckovDownedRevive
{
    internal static class HealthStateAccessor
    {
        private static readonly FieldInfo CurrentHealthField =
            typeof(Health).GetField(
                "_currentHealth",
                BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool CanSetCurrentHealth
        {
            get { return CurrentHealthField != null; }
        }

        public static float GetCurrentHealth(Health health)
        {
            if (health == null)
            {
                return 0.0f;
            }

            return CurrentHealthField == null
                ? health.CurrentHealth
                : (float)CurrentHealthField.GetValue(health);
        }

        public static void SetCurrentHealth(Health health, float value)
        {
            if (health == null)
            {
                return;
            }

            if (CurrentHealthField == null)
            {
                health.SetHealth(value);
                return;
            }

            CurrentHealthField.SetValue(health, value);
        }
    }
}
