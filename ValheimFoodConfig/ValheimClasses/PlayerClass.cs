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
                    ApplyMeadConfigurations();
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
}