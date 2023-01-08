using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using Jotunn.Managers;
using UnityEngine.SceneManagement;
using HarmonyLib;
using BepInEx.Configuration;
using ServerSync;
using System.Reflection;

namespace ValheimFoodConfig {
    [BepInPlugin(pluginGUID, pluginName, pluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]

    public class ValheimFoodConfig : BaseUnityPlugin {
        const string pluginGUID = "com.ValheimFoodConfig";
        const string pluginName = "ValheimFoodConfig";
        const string pluginVersion = "2.0.0";
        public static readonly ManualLogSource LOG = BepInEx.Logging.Logger.CreateLogSource(pluginName);
        ConfigSync configSyncs = new ConfigSync(pluginGUID) { DisplayName = pluginName, CurrentVersion = pluginVersion, MinimumRequiredVersion = pluginVersion };
        static public Dictionary<string, List<ConfigEntry<float>>> configs = new Dictionary<string, List<ConfigEntry<float>>>();

        void Awake() {
            PrefabManager.OnVanillaPrefabsAvailable += readConfigs;
            Assembly assembly = Assembly.GetExecutingAssembly();
            Harmony harmony = new Harmony(pluginGUID);
            harmony.PatchAll(assembly);
        }

        public void readConfigs() {
            if (configs.Count == 0) {
                LOG.LogMessage("started loading configs");
                Dictionary<string, UnityEngine.Object> itemDrops = PrefabManager.Cache.GetPrefabs(typeof(ItemDrop));
                foreach (KeyValuePair<string, UnityEngine.Object> entry in itemDrops) {
                    ItemDrop item = ((ItemDrop)entry.Value);
                    // must check m_foodStamina value since many objects have m_food values
                    if (item.m_itemData.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Consumable && item.m_itemData.m_shared.m_foodStamina > 0) {
                        List<ConfigEntry<float>> listConfig = new List<ConfigEntry<float>>();
                        listConfig.Add(config($"{entry.Key}", "Health", item.m_itemData.m_shared.m_food, $"Sets health value of {entry.Key}."));
                        listConfig.Add(config($"{entry.Key}", "Stamina", item.m_itemData.m_shared.m_foodStamina, $"Sets stamina value of {entry.Key}."));
                        listConfig.Add(config($"{entry.Key}", "Duration", item.m_itemData.m_shared.m_foodBurnTime, $"Sets duration value of {entry.Key}."));
                        listConfig.Add(config($"{entry.Key}", "HealthRegen", item.m_itemData.m_shared.m_foodRegen, $"Sets regen value of {entry.Key}."));
                        listConfig.Add(config($"{entry.Key}", "Eitr", item.m_itemData.m_shared.m_foodEitr, $"Sets eitr value of {entry.Key}."));
                        configs.Add(entry.Key, listConfig);
                    }
                    LOG.LogMessage($"{configs.Count}");
                }

                config("General", "Lock Configuration", true, new ConfigDescription("[Server Only] The configuration is locked and may not be changed by clients once it has been synced from the server. Only valid for server config, will have no effect on clients."));
                LOG.LogMessage("finished loading configs");
            }
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description, bool synchronizedSetting = true) {
            ConfigDescription extendedDescription =
                new ConfigDescription(
                    description.Description +
                    (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                    description.AcceptableValues, description.Tags);
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, extendedDescription);
            SyncedConfigEntry<T> syncedConfigEntry = configSyncs.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, string description,
            bool synchronizedSetting = true) {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        }

        private static void updateFoodValues() {
            foreach (KeyValuePair<string, List<ConfigEntry<float>>> entry in configs) {
                ItemDrop item = PrefabManager.Cache.GetPrefab<ItemDrop>(entry.Key);
                if (item != null) {
                    foreach (ConfigEntry<float> configEntry in entry.Value) {
                        if (configEntry.Definition.Key.Equals("Health")) {
                            item.m_itemData.m_shared.m_food = configEntry.Value;
                        } else if (configEntry.Definition.Key.Equals("Stamina")) {
                            item.m_itemData.m_shared.m_foodStamina = configEntry.Value;
                        } else if (configEntry.Definition.Key.Equals("Duration")) {
                            item.m_itemData.m_shared.m_foodBurnTime = configEntry.Value;
                        } else if (configEntry.Definition.Key.Equals("HealthRegen")) {
                            item.m_itemData.m_shared.m_foodRegen = configEntry.Value;
                        } else if (configEntry.Definition.Key.Equals("Eitr")) {
                            item.m_itemData.m_shared.m_foodEitr = configEntry.Value;
                        }
                    }
                }
            }
        }

        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(typeof(Player), nameof(Player.Awake))]
        public static class Patch_Player_Awake {
            private static void Postfix() {
                if (SceneManager.GetActiveScene().name.Equals("main")) {
                    updateFoodValues();
                }
            }
        }
    }
}
