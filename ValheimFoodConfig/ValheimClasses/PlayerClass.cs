using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Managers;
using UnityEngine.SceneManagement;

namespace ValheimFoodConfig
{
    public class PlayerClass
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
                }
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
        
    /// <summary>
    /// Credit for this method goes to the creators of valheimPlus
    /// https://github.com/valheimPlus/ValheimPlus/blob/development/ValheimPlus/GameClasses/Player.cs
    /// </summary>
    [HarmonyPatch(typeof(Player), nameof(Player.GetTotalFoodValue))]
    public static class Player_GetTotalFoodValue_Transpiler
    {
        private static FieldInfo field_Food_m_health = AccessTools.Field(typeof(Player.Food), nameof(Player.Food.m_health));
        private static FieldInfo field_Food_m_stamina = AccessTools.Field(typeof(Player.Food), nameof(Player.Food.m_stamina));
        private static FieldInfo field_Food_m_item = AccessTools.Field(typeof(Player.Food), nameof(Player.Food.m_item));
        private static FieldInfo field_ItemData_m_shared = AccessTools.Field(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.m_shared));
        private static FieldInfo field_SharedData_m_food = AccessTools.Field(typeof(ItemDrop.ItemData.SharedData), nameof(ItemDrop.ItemData.SharedData.m_food));
        private static FieldInfo field_SharedData_m_foodStamina = AccessTools.Field(typeof(ItemDrop.ItemData.SharedData), nameof(ItemDrop.ItemData.SharedData.m_foodStamina));
        
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

                    if (loads_health || loads_stamina)
                    {
                        il[i].operand = field_Food_m_item;
                        il.Insert(++i, new CodeInstruction(OpCodes.Ldfld, field_ItemData_m_shared));
                        il.Insert(++i, new CodeInstruction(OpCodes.Ldfld, loads_health ? field_SharedData_m_food : field_SharedData_m_foodStamina));
                    }
                }
            }

            return il.AsEnumerable();
        }
    }
    }
}