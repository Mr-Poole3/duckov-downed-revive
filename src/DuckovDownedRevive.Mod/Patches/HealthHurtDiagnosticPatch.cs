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
            if (character == null)
            {
                return;
            }

            ModBehaviour behaviour = ModBehaviour.Instance;
            Debug.LogWarning(
                "[DuckovDownedRevive][Probe][Health] PRE" +
                " frame=" + Time.frameCount +
                " role=" + Role(behaviour) +
                " target=" + character.name +
                " source=" + SourceName(damageInfo) +
                " damageType=" + damageInfo.damageType +
                " current=" + __instance.CurrentHealth.ToString("0.00") +
                " damageValue=" + damageInfo.damageValue.ToString("0.00") +
                " finalDamage=" + damageInfo.finalDamage.ToString("0.00") +
                " explosion=" + damageInfo.isExplosion +
                " buffEffect=" + damageInfo.isFromBuffOrEffect);
        }

        private static void Postfix(
            Health __instance,
            DamageInfo damageInfo,
            bool __result)
        {
            CharacterMainControl character = PlayerCharacter(__instance);
            if (character == null)
            {
                return;
            }

            Debug.LogWarning(
                "[DuckovDownedRevive][Probe][Health] POST" +
                " frame=" + Time.frameCount +
                " role=" + Role(ModBehaviour.Instance) +
                " target=" + character.name +
                " result=" + __result +
                " current=" + __instance.CurrentHealth.ToString("0.00") +
                " dead=" + __instance.IsDead +
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
            if (character == null)
            {
                return;
            }

            Debug.LogWarning(
                "[DuckovDownedRevive][Probe][Receiver] PRE" +
                " frame=" + Time.frameCount +
                " role=" + HealthHurtDiagnosticPatch.Role(
                    ModBehaviour.Instance) +
                " target=" + character.name +
                " source=" + HealthHurtDiagnosticPatch.SourceName(damageInfo) +
                " damageType=" + damageInfo.damageType +
                " current=" + health.CurrentHealth.ToString("0.00") +
                " damageValue=" + damageInfo.damageValue.ToString("0.00") +
                " finalDamage=" + damageInfo.finalDamage.ToString("0.00"));
        }

        private static void Postfix(
            DamageReceiver __instance,
            DamageInfo damageInfo,
            bool __result)
        {
            Health health = __instance == null ? null : __instance.health;
            CharacterMainControl character =
                HealthHurtDiagnosticPatch.PlayerCharacter(health);
            if (character == null)
            {
                return;
            }

            Debug.LogWarning(
                "[DuckovDownedRevive][Probe][Receiver] POST" +
                " frame=" + Time.frameCount +
                " role=" + HealthHurtDiagnosticPatch.Role(
                    ModBehaviour.Instance) +
                " target=" + character.name +
                " result=" + __result +
                " current=" + health.CurrentHealth.ToString("0.00") +
                " dead=" + health.IsDead +
                " source=" + HealthHurtDiagnosticPatch.SourceName(damageInfo));
        }
    }
}
