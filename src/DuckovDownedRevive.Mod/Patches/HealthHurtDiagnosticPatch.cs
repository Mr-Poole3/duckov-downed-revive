using HarmonyLib;
using UnityEngine;

namespace DuckovDownedRevive.Mod.Patches
{
    [HarmonyPatch(typeof(Health), "Hurt")]
    internal static class HealthHurtDiagnosticPatch
    {
        private static void Prefix(Health __instance, DamageInfo damageInfo)
        {
            DownedReviveModBehaviour behaviour =
                DownedReviveModBehaviour.Instance;
            if (behaviour == null || __instance == null)
            {
                return;
            }

            CharacterMainControl character = __instance.TryGetCharacter();
            if (character == null ||
                !character.IsMainCharacter)
            {
                return;
            }

            float currentHealth = __instance.CurrentHealth;
            bool potentiallyLethal = currentHealth <= 0.0f ||
                damageInfo.finalDamage >= currentHealth;
            if (!potentiallyLethal)
            {
                return;
            }

            string sourceName = damageInfo.fromCharacter == null
                ? "none"
                : damageInfo.fromCharacter.name;
            Debug.Log(
                "[DuckovDownedRevive][Probe] role=" +
                (behaviour.IsHost ? "host" : "client") +
                " target=" + character.name +
                " source=" + sourceName +
                " damageType=" + damageInfo.damageType +
                " current=" + currentHealth.ToString("0.00") +
                " incoming=" + damageInfo.finalDamage.ToString("0.00") +
                " explosion=" + damageInfo.isExplosion +
                " buffEffect=" + damageInfo.isFromBuffOrEffect);
        }
    }
}
