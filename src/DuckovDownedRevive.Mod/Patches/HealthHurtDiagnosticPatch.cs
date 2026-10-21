using HarmonyLib;
using UnityEngine;

namespace DuckovDownedRevive.Patches
{
    [HarmonyPatch(typeof(Health), "Hurt")]
    internal static class HealthHurtDiagnosticPatch
    {
        private static void Prefix(Health __instance, DamageInfo damageInfo)
        {
            CharacterMainControl character = PlayerCharacter(__instance);
            string target = character == null
                ? (__instance == null ? "none" : __instance.name)
                : character.name;
            string line =
                "[DuckovDownedRevive][Probe][Health] PRE" +
                " frame=" + Time.frameCount +
                " role=" + Role(ModBehaviour.Instance) +
                " player=" + (character != null) +
                " target=" + target +
                " source=" + SourceName(damageInfo) +
                " damageType=" + damageInfo.damageType +
                " current=" + (__instance == null
                    ? "n/a"
                    : __instance.CurrentHealth.ToString("0.00")) +
                " damageValue=" + damageInfo.damageValue.ToString("0.00") +
                " finalDamage=" + damageInfo.finalDamage.ToString("0.00") +
                " explosion=" + damageInfo.isExplosion +
                " buffEffect=" + damageInfo.isFromBuffOrEffect;
            ProbeDiagnostics.RecordHealthPrefix(line);

            if (character == null)
            {
                return;
            }
        }

        private static void Postfix(
            Health __instance,
            DamageInfo damageInfo,
            bool __result)
        {
            CharacterMainControl character = PlayerCharacter(__instance);
            ProbeDiagnostics.RecordHealthPostfix(
                "[DuckovDownedRevive][Probe][Health] POST" +
                " frame=" + Time.frameCount +
                " role=" + Role(ModBehaviour.Instance) +
                " player=" + (character != null) +
                " target=" + (character == null
                    ? (__instance == null ? "none" : __instance.name)
                    : character.name) +
                " result=" + __result +
                " current=" + (__instance == null
                    ? "n/a"
                    : __instance.CurrentHealth.ToString("0.00")) +
                " dead=" + (__instance != null && __instance.IsDead) +
                " source=" + SourceName(damageInfo));
        }

        internal static CharacterMainControl PlayerCharacter(Health health)
        {
            if (health == null)
            {
                return null;
            }

            CharacterMainControl character = health.TryGetCharacter();
            return character != null && character.IsMainCharacter
                ? character
                : null;
        }

        internal static string SourceName(DamageInfo damageInfo)
        {
            return damageInfo.fromCharacter == null
                ? "none"
                : damageInfo.fromCharacter.name;
        }

        internal static string Role(ModBehaviour behaviour)
        {
            return behaviour != null && behaviour.IsHost ? "host" : "client";
        }
    }

    [HarmonyPatch(typeof(DamageReceiver), "Hurt")]
    internal static class DamageReceiverHurtDiagnosticPatch
    {
        private static void Prefix(
            DamageReceiver __instance,
            DamageInfo damageInfo)
        {
            Health health = __instance == null ? null : __instance.health;
            CharacterMainControl character =
                HealthHurtDiagnosticPatch.PlayerCharacter(health);
            string target = character == null
                ? (health == null ? "none" : health.name)
                : character.name;
            string line =
                "[DuckovDownedRevive][Probe][Receiver] PRE" +
                " frame=" + Time.frameCount +
                " role=" + HealthHurtDiagnosticPatch.Role(
                    ModBehaviour.Instance) +
                " player=" + (character != null) +
                " target=" + target +
                " source=" + HealthHurtDiagnosticPatch.SourceName(damageInfo) +
                " damageType=" + damageInfo.damageType +
                " current=" + (health == null
                    ? "n/a"
                    : health.CurrentHealth.ToString("0.00")) +
                " damageValue=" + damageInfo.damageValue.ToString("0.00") +
                " finalDamage=" + damageInfo.finalDamage.ToString("0.00");
            ProbeDiagnostics.RecordReceiverPrefix(line);

            if (character == null)
            {
                return;
            }
        }

        private static void Postfix(
            DamageReceiver __instance,
            DamageInfo damageInfo,
            bool __result)
        {
            Health health = __instance == null ? null : __instance.health;
            CharacterMainControl character =
                HealthHurtDiagnosticPatch.PlayerCharacter(health);
            ProbeDiagnostics.RecordReceiverPostfix(
                "[DuckovDownedRevive][Probe][Receiver] POST" +
                " frame=" + Time.frameCount +
                " role=" + HealthHurtDiagnosticPatch.Role(
                    ModBehaviour.Instance) +
                " player=" + (character != null) +
                " target=" + (character == null
                    ? (health == null ? "none" : health.name)
                    : character.name) +
                " result=" + __result +
                " current=" + (health == null
                    ? "n/a"
                    : health.CurrentHealth.ToString("0.00")) +
                " dead=" + (health != null && health.IsDead) +
                " source=" + HealthHurtDiagnosticPatch.SourceName(damageInfo));
        }
    }
}
