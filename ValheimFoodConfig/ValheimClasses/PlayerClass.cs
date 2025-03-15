using System.Collections.Generic;
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
                    if (configEntry.Definition.Key.Equals(Constants.Health))
                    {
                        item.m_itemData.m_shared.m_food = configEntry.Value;
                    }
                    else if (configEntry.Definition.Key.Equals(Constants.Stamina))
                    {
                        item.m_itemData.m_shared.m_foodStamina = configEntry.Value;
                    }
                    else if (configEntry.Definition.Key.Equals(Constants.Duration))
                    {
                        item.m_itemData.m_shared.m_foodBurnTime = configEntry.Value;
                    }
                    else if (configEntry.Definition.Key.Equals(Constants.HealthRegen))
                    {
                        item.m_itemData.m_shared.m_foodRegen = configEntry.Value;
                    }
                    else if (configEntry.Definition.Key.Equals(Constants.Eitr))
                    {
                        item.m_itemData.m_shared.m_foodEitr = configEntry.Value;
                    }
                }
            }
        }
    }
}