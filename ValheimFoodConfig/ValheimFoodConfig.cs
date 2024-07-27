using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using Jotunn.Managers;
using UnityEngine.SceneManagement;
using HarmonyLib;
using BepInEx.Configuration;
using ServerSync;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ValheimFoodConfig {
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]

    public class ValheimFoodConfig : BaseUnityPlugin {
        private const string PluginGuid = "com.ValheimFoodConfig";
        private const string PluginName = "ValheimFoodConfig";
        private const string PluginVersion = "2.1.1";
        private static readonly ManualLogSource LOG = BepInEx.Logging.Logger.CreateLogSource(PluginName);
        private ConfigSync _configSync = new ConfigSync(PluginGuid) { DisplayName = PluginName, CurrentVersion = PluginVersion, MinimumRequiredVersion = PluginVersion };
        private static Dictionary<string, List<ConfigEntry<float>>> _configEntries = new Dictionary<string, List<ConfigEntry<float>>>();
        private const string Health = "Health";
        private const string Stamina = "Stamina";
        private const string Duration = "Duration";
        private const string HealthRegen = "HealthRegen";
        private const string Eitr = "Eitr";

        void Awake() {
            PrefabManager.OnVanillaPrefabsAvailable += LoadConfigurations;
            Assembly assembly = Assembly.GetExecutingAssembly();
            Harmony harmony = new Harmony(PluginGuid);
            harmony.PatchAll(assembly);
        }

        public void LoadConfigurations()
        {
            if (_configEntries.Count > 0) return;
            
                LOG.LogMessage("loading configs");
                Dictionary<string, UnityEngine.Object> itemDrops = PrefabManager.Cache.GetPrefabs(typeof(ItemDrop));
                foreach (var entry in itemDrops) {
                    ItemDrop item = ((ItemDrop)entry.Value);

                    if (item.m_itemData.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Consumable &&
                        item.m_itemData.m_shared.m_consumeStatusEffect != null)
                    {
                        if (entry.Key.Equals("MeadHealthMedium"))
                        {
                            LOG.LogMessage(item.m_itemData.m_shared.m_food);
                            LOG.LogMessage(item.m_itemData.m_shared.m_foodStamina);
                            LOG.LogMessage(item.m_itemData.m_shared.m_foodBurnTime);
                            LOG.LogMessage(item.m_itemData.m_shared.m_foodRegen);
                            LOG.LogMessage(item.m_itemData.m_shared.m_foodEitr);
                            LOG.LogMessage(item.m_itemData.m_shared.m_consumeStatusEffect);
                            LOG.LogMessage(item.m_itemData.m_shared.m_consumeStatusEffect.GetDuration());
                            LOG.LogMessage(item.m_itemData.m_shared.m_consumeStatusEffect.m_category);
                            LOG.LogMessage(item.m_itemData.m_shared.m_consumeStatusEffect.m_name);
                        }
                        
                        

                        // [Message:ValheimFoodConfig] Loading configuration
                        // [Message:ValheimFoodConfig] MeadTasty
                        // [Message:ValheimFoodConfig] Pukeberries
                        // [Message:ValheimFoodConfig] MeadEitrLingering
                        // [Message:ValheimFoodConfig] MeadEitrMinor
                        // [Message:ValheimFoodConfig] MeadHealthMinor
                        // [Message:ValheimFoodConfig] MeadStaminaMinor 
                        // [Message:ValheimFoodConfig] MeadStaminaLingering
                        // [Message:ValheimFoodConfig] BarleyWine
                        // [Message:ValheimFoodConfig] MeadHealthMajor
                        // [Message:ValheimFoodConfig] MeadHealthLingering
                        // [Message:ValheimFoodConfig] MeadFrostResist
                        // [Message:ValheimFoodConfig] MeadHealthMedium
                        // [Message:ValheimFoodConfig] MeadStaminaMedium
                        // [Message:ValheimFoodConfig] MeadPoisonResist
                        // [Message:ValheimFoodConfig] RottenMeat
                        // [Message:ValheimFoodConfig] finished loading configs   
                    }

                    // must check m_foodStamina value since many objects have m_food values
                    if (item.m_itemData.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Consumable && item.m_itemData.m_shared.m_foodStamina > 0) {
                        List<ConfigEntry<float>> listConfig = new List<ConfigEntry<float>>();
                        listConfig.Add(config($"{entry.Key}", Health, item.m_itemData.m_shared.m_food, $"Sets health value of {entry.Key}."));
                        listConfig.Add(config($"{entry.Key}", Stamina, item.m_itemData.m_shared.m_foodStamina, $"Sets stamina value of {entry.Key}."));
                        listConfig.Add(config($"{entry.Key}", Duration, item.m_itemData.m_shared.m_foodBurnTime, $"Sets duration value of {entry.Key}."));
                        listConfig.Add(config($"{entry.Key}", HealthRegen, item.m_itemData.m_shared.m_foodRegen, $"Sets regen value of {entry.Key}."));
                        listConfig.Add(config($"{entry.Key}", Eitr, item.m_itemData.m_shared.m_foodEitr, $"Sets eitr value of {entry.Key}."));
                        _configEntries.Add(entry.Key, listConfig);
                    }
                }

                config("General", "Lock Configuration", true, new ConfigDescription("[Server Only] The configuration is locked and may not be changed by clients once it has been synced from the server. Only valid for server config, will have no effect on clients."));
                LOG.LogMessage("done loading configs");
            
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description, bool synchronizedSetting = true) {
            ConfigDescription extendedDescription =
                new ConfigDescription(
                    description.Description +
                    (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                    description.AcceptableValues, description.Tags);
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, extendedDescription);
            SyncedConfigEntry<T> syncedConfigEntry = _configSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, string description,
            bool synchronizedSetting = true) {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        }

        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(typeof(Player), nameof(Player.Awake))]
        public static class Patch_Player_Awake {
            private static void Postfix() {
                if (SceneManager.GetActiveScene().name.Equals("main")) {
                    UpdateFoodPrefabs();
                }
            }
        }
        
        [HarmonyPatch(typeof(StatusEffect), nameof(StatusEffect.ModifyHealthRegen))]
        public static class Patch_ModifyHealthRegen {
            private static void Prefix(StatusEffect __instance, ref float regenMultiplier) {
                LOG.LogMessage("I GET HERE");
                LOG.LogMessage(__instance != null);
                if (__instance.m_name.Equals("m_poison"))
                {
                    
                }
            }
            
            private static void Postfix() {
                LOG.LogMessage("POSTFIX");
            }
        }
        
        [HarmonyPatch(typeof(StatusEffect), nameof(StatusEffect.ModifyStaminaRegen))]
        public static class Patch_ModifyStaminaRegen {
            private static void Prefix(StatusEffect __instance, ref float staminaRegen) {
                LOG.LogMessage("ModifyStaminaRegen Prefix staminaRegen " + staminaRegen);
                LOG.LogMessage(__instance.m_name);
            }
            
            private static void Postfix() {
                LOG.LogMessage("ModifyStaminaRegen Postfix");
            }
        }
        
        private static void UpdateFoodPrefabs() {
            foreach (KeyValuePair<string, List<ConfigEntry<float>>> entry in _configEntries) {
                ItemDrop item = PrefabManager.Cache.GetPrefab<ItemDrop>(entry.Key);
                if (item != null) {
                    foreach (ConfigEntry<float> configEntry in entry.Value) {
                        if (configEntry.Definition.Key.Equals(Health)) {
                            item.m_itemData.m_shared.m_food = configEntry.Value;
                        } else if (configEntry.Definition.Key.Equals(Stamina)) {
                            item.m_itemData.m_shared.m_foodStamina = configEntry.Value;
                        } else if (configEntry.Definition.Key.Equals(Duration)) {
                            item.m_itemData.m_shared.m_foodBurnTime = configEntry.Value;
                        } else if (configEntry.Definition.Key.Equals(HealthRegen)) {
                            item.m_itemData.m_shared.m_foodRegen = configEntry.Value;
                        } else if (configEntry.Definition.Key.Equals(Eitr)) {
                            item.m_itemData.m_shared.m_foodEitr = configEntry.Value;
                        }
                    }
                }
            }
        }
    }
}
