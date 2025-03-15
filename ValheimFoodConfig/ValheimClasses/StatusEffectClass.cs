using HarmonyLib;

namespace ValheimFoodConfig
{
    public class StatusEffectClass
    {
        [HarmonyPatch(typeof(StatusEffect), nameof(StatusEffect.ModifyStaminaRegen))]
        public static class Patch_ModifyStaminaRegen
        {
            private static void Prefix(StatusEffect __instance, ref float staminaRegen)
            {
                ValheimFoodConfig.LOG.LogMessage("ModifyStaminaRegen Prefix staminaRegen " + staminaRegen);
                ValheimFoodConfig.LOG.LogMessage(__instance.m_name);
            }

            private static void Postfix()
            {
                ValheimFoodConfig.LOG.LogMessage("ModifyStaminaRegen Postfix");
            }
        }
    }
    
    [HarmonyPatch(typeof(StatusEffect), nameof(StatusEffect.ModifyHealthRegen))]
    public static class Patch_ModifyHealthRegen
    {
        private static void Prefix(StatusEffect __instance, ref float regenMultiplier)
        {
            ValheimFoodConfig.LOG.LogMessage("I GET HERE");
            ValheimFoodConfig.LOG.LogMessage(__instance != null);
            if (__instance.m_name.Equals("m_poison"))
            {
            }
        }

        private static void Postfix()
        {
            ValheimFoodConfig.LOG.LogMessage("POSTFIX");
        }
    }
}