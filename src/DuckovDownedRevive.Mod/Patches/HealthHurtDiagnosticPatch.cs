using System.Collections.Generic;
using DuckovDownedRevive.Core;
using HarmonyLib;
using UnityEngine;

namespace DuckovDownedRevive.Patches
{
    [HarmonyPatch(typeof(Health), "Hurt")]
    internal static class HealthHurtDiagnosticPatch
    {
        private sealed class PendingGuard
        {
            public float OriginalHealth;
            public float GuardedHealth;
            public DamageInfo DamageInfo;
            public CharacterMainControl Character;
        }

        private static readonly Dictionary<Health, PendingGuard> Pending =
            new Dictionary<Health, PendingGuard>();

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
                    : HealthStateAccessor.GetCurrentHealth(__instance)
                        .ToString("0.00")) +
                " damageValue=" + damageInfo.damageValue.ToString("0.00") +
                " finalDamage=" + damageInfo.finalDamage.ToString("0.00") +
                " explosion=" + damageInfo.isExplosion +
                " buffEffect=" + damageInfo.isFromBuffOrEffect;
            ProbeDiagnostics.RecordHealthPrefix(line);

            if (character == null)
            {
                return;
            }

            ModBehaviour behaviour = ModBehaviour.Instance;
            if (behaviour == null ||
                Pending.ContainsKey(__instance) ||
                TerminalDamageBypass.Active)
            {
                return;
            }

            float originalHealth =
                HealthStateAccessor.GetCurrentHealth(__instance);
            float guardedHealth;
            if (!behaviour.TryGuardHostLethalDamage(
                __instance,
                character,
                damageInfo,
                out guardedHealth))
            {
                return;
            }

            Pending[__instance] = new PendingGuard
            {
                OriginalHealth = originalHealth,
                GuardedHealth = guardedHealth,
                DamageInfo = damageInfo,
                Character = character
            };
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

            PendingGuard pending;
            if (__instance == null ||
                !Pending.TryGetValue(__instance, out pending))
            {
                return;
            }

            Pending.Remove(__instance);
            float healthAfter =
                HealthStateAccessor.GetCurrentHealth(__instance);
            float actualDamage = LethalDamageGuard.ActualDamageApplied(
                pending.GuardedHealth,
                healthAfter);
            bool lethal = LethalDamageGuard.IsLethal(
                pending.OriginalHealth,
                actualDamage);
            ModBehaviour behaviour = ModBehaviour.Instance;

            if (lethal &&
                behaviour != null &&
                behaviour.TryEnterHostDown(
                    __instance,
                    pending.Character,
                    pending.DamageInfo))
            {
                ProbeDiagnostics.RecordSystem(
                    "[DuckovDownedRevive][Guard] DOWN_ACCEPTED" +
                    " frame=" + Time.frameCount +
                    " original=" +
                    pending.OriginalHealth.ToString("0.00") +
                    " actualDamage=" + actualDamage.ToString("0.00") +
                    " guarded=" + pending.GuardedHealth.ToString("0.00") +
                    " after=" + healthAfter.ToString("0.00"));
                return;
            }

            float restoredHealth = LethalDamageGuard.RemainingHealth(
                pending.OriginalHealth,
                actualDamage);
            HealthStateAccessor.SetCurrentHealth(
                __instance,
                restoredHealth);
            ProbeDiagnostics.RecordSystem(
                "[DuckovDownedRevive][Guard] RESTORED" +
                " frame=" + Time.frameCount +
                " lethal=" + lethal +
                " original=" + pending.OriginalHealth.ToString("0.00") +
                " actualDamage=" + actualDamage.ToString("0.00") +
                " restored=" + restoredHealth.ToString("0.00"));
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
