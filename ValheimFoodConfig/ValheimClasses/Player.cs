using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ValheimFoodConfig
{
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(Player), nameof(Player.Awake))]
    public static class Patch_Player_Awake
    {
        private static void Postfix()
        {
            if (SceneManager.GetActiveScene().name.Equals("main"))
            {
                UpdateFoodPrefabs();
                ApplyMeadConfigurations();
            }
        }

        private static void UpdateFoodPrefabs()
        {
            foreach (KeyValuePair<string, List<ConfigEntry<float>>> entry in ValheimFoodConfigData.ConfigEntries)
            {
                var item = PrefabManager.Cache.GetPrefab<ItemDrop>(entry.Key);
                if (item == null) continue;

                foreach (ConfigEntry<float> configEntry in entry.Value)
                {
                    float baseValue = configEntry.Value;
                    float newValue = baseValue;

                    if (configEntry.Definition.Key.Equals(Constants.Health))
                    {
                        newValue *= ValheimFoodConfigData.HealthModifier.Value;
                        item.m_itemData.m_shared.m_food = newValue;
                    }
                    else if (configEntry.Definition.Key.Equals(Constants.Stamina))
                    {
                        newValue *= ValheimFoodConfigData.StaminaModifier.Value;
                        item.m_itemData.m_shared.m_foodStamina = newValue;
                    }
                    else if (configEntry.Definition.Key.Equals(Constants.Duration))
                    {
                        newValue *= ValheimFoodConfigData.DurationModifier.Value;
                        item.m_itemData.m_shared.m_foodBurnTime = newValue;
                    }
                    else if (configEntry.Definition.Key.Equals(Constants.HealthRegen))
                    {
                        newValue *= ValheimFoodConfigData.HealthRegenModifier.Value;
                        item.m_itemData.m_shared.m_foodRegen = newValue;
                    }
                    else if (configEntry.Definition.Key.Equals(Constants.Eitr))
                    {
                        newValue *= ValheimFoodConfigData.EitrModifier.Value;
                        item.m_itemData.m_shared.m_foodEitr = newValue;
                    }
                }
            }
        }

        private static void ApplyMeadConfigurations()
        {
            Dictionary<string, Object> itemDrops = PrefabManager.Cache.GetPrefabs(typeof(ItemDrop));

            foreach (var meadName in Constants.MeadNames)
            {
                if (!itemDrops.TryGetValue(meadName, out Object meadObj))
                    continue;

                var meadDrop = meadObj as ItemDrop;
                if (meadDrop == null) continue;

                if (!ValheimFoodConfigData.MeadConfigs.TryGetValue(meadName, out MeadEffectConfig meadConfig))
                    continue;

                var effect = meadDrop.m_itemData.m_shared.m_consumeStatusEffect;
                if (effect == null)
                    continue;


                if (effect is SE_Stats statsEffect)
                {
                    statsEffect.m_ttl = meadConfig.Duration.Value;
                    statsEffect.m_healthOverTime = meadConfig.HealthOverTime.Value;
                    statsEffect.m_staminaOverTime = meadConfig.StaminaOverTime.Value;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.GetTotalFoodValue))]
    public static class Player_GetTotalFoodValue_Transpiler
    {
        private static readonly FieldInfo field_Food_m_health =
            AccessTools.Field(typeof(Player.Food), nameof(Player.Food.m_health));

        private static readonly FieldInfo field_Food_m_stamina =
            AccessTools.Field(typeof(Player.Food), nameof(Player.Food.m_stamina));

        private static readonly FieldInfo field_Food_m_eitr =
            AccessTools.Field(typeof(Player.Food), nameof(Player.Food.m_eitr));

        private static readonly FieldInfo field_Food_m_item =
            AccessTools.Field(typeof(Player.Food), nameof(Player.Food.m_item));

        private static readonly FieldInfo field_ItemData_m_shared =
            AccessTools.Field(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.m_shared));

        private static readonly FieldInfo field_SharedData_m_food = AccessTools.Field(
            typeof(ItemDrop.ItemData.SharedData),
            nameof(ItemDrop.ItemData.SharedData.m_food));

        private static readonly FieldInfo field_SharedData_m_foodStamina =
            AccessTools.Field(typeof(ItemDrop.ItemData.SharedData),
                nameof(ItemDrop.ItemData.SharedData.m_foodStamina));

        private static readonly FieldInfo field_SharedData_m_foodEitr =
            AccessTools.Field(typeof(ItemDrop.ItemData.SharedData),
                nameof(ItemDrop.ItemData.SharedData.m_foodEitr));

        /*
         * https://github.com/Grantapher/ValheimPlus/blob/main/ValheimPlus/GameClasses/Player.cs
         * I believe this is the only way to stop food degradation. I am using a method from ValheimPlus, since they
         * figured this out first.
         */
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            if (!ValheimFoodConfigData.DisableFoodDegradation.Value) return instructions;

            List<CodeInstruction> il = instructions.ToList();

            if (ValheimFoodConfigData.DisableFoodDegradation.Value)
            {
                for (int i = 0; i < il.Count; ++i)
                {
                    bool loads_health = il[i].LoadsField(field_Food_m_health);
                    bool loads_stamina = il[i].LoadsField(field_Food_m_stamina);
                    bool loads_eitr = il[i].LoadsField(field_Food_m_eitr);

                    if (loads_health || loads_stamina || loads_eitr)
                    {
                        il[i].operand = field_Food_m_item;
                        il.Insert(++i, new CodeInstruction(OpCodes.Ldfld, field_ItemData_m_shared));
                        if (loads_health)
                        {
                            il.Insert(++i, new CodeInstruction(OpCodes.Ldfld, field_SharedData_m_food));
                        }
                        else if (loads_stamina)
                        {
                            il.Insert(++i, new CodeInstruction(OpCodes.Ldfld, field_SharedData_m_foodStamina));
                        }
                        else
                        {
                            il.Insert(++i, new CodeInstruction(OpCodes.Ldfld, field_SharedData_m_foodEitr));
                        }
                    }
                }
            }

            return il.AsEnumerable();
        }
    }
}