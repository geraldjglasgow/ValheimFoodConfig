using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using Jotunn.Managers;
using ServerSync;

namespace ValheimFoodConfig
{
    public static class ValheimFoodConfigData
    {
        public static Dictionary<string, List<ConfigEntry<float>>> ConfigEntries { get; }
            = new Dictionary<string, List<ConfigEntry<float>>>();
        
        public static ConfigEntry<bool> DisableFoodDegradation { get; private set; }

        private static ConfigFile _configFile;
        private static ConfigSync _configSync;

        public static void Initialize(ConfigFile configFile, ConfigSync configSync)
        {
            _configFile = configFile;
            _configSync = configSync;
        }

        private static ConfigEntry<T> CreateConfigEntry<T>(
            string group,
            string name,
            T defaultValue,
            string description,
            bool synchronizedSetting = true)
        {
            return CreateConfigEntry(
                group,
                name,
                defaultValue,
                new ConfigDescription(description),
                synchronizedSetting
            );
        }

        public static ConfigEntry<T> CreateConfigEntry<T>(
            string group,
            string name,
            T defaultValue,
            ConfigDescription description,
            bool synchronizedSetting = true)
        {
            if (_configFile == null || _configSync == null)
            {
                throw new Exception(
                    "ValheimFoodConfigData not initialized. Call Initialize() first in your plugin's Awake().");
            }

            var extendedDesc = new ConfigDescription(
                description.Description +
                (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                description.AcceptableValues,
                description.Tags);

            ConfigEntry<T> configEntry = _configFile.Bind(group, name, defaultValue, extendedDesc);

            SyncedConfigEntry<T> syncedConfigEntry = _configSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        public static void LoadConfigurations()
        {
            LoadFoodConfigurations();
            LoadDisableFoodDegradationConfiguration();
        }

        private static void LoadDisableFoodDegradationConfiguration()
        {
            DisableFoodDegradation = CreateConfigEntry(
                "0_Modifiers",
                "Disable Food Degradation",
                false,
                "Set to true to disable food degradation.");
        }
        
        public static void LoadFoodConfigurations()
        {
            if (ConfigEntries.Count > 0) return;

            ValheimFoodConfig.LOG.LogMessage("Loading configs...");
            Dictionary<string, UnityEngine.Object> itemDrops = PrefabManager.Cache.GetPrefabs(typeof(ItemDrop));

            foreach (var entry in itemDrops)
            {
                ItemDrop itemDrop = (ItemDrop)entry.Value;

                if (itemDrop.m_itemData.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Consumable &&
                    itemDrop.m_itemData.m_shared.m_foodStamina > 0)
                {
                    var listConfig = new List<ConfigEntry<float>>();

                    listConfig.Add(CreateConfigEntry(
                        entry.Key, Constants.Health,
                        itemDrop.m_itemData.m_shared.m_food,
                        $"Sets health value of {entry.Key}."));

                    listConfig.Add(CreateConfigEntry(
                        entry.Key, Constants.Stamina,
                        itemDrop.m_itemData.m_shared.m_foodStamina,
                        $"Sets stamina value of {entry.Key}."));

                    listConfig.Add(CreateConfigEntry(
                        entry.Key, Constants.Duration,
                        itemDrop.m_itemData.m_shared.m_foodBurnTime,
                        $"Sets duration value of {entry.Key}."));

                    listConfig.Add(CreateConfigEntry(
                        entry.Key, Constants.HealthRegen,
                        itemDrop.m_itemData.m_shared.m_foodRegen,
                        $"Sets regen value of {entry.Key}."));

                    listConfig.Add(CreateConfigEntry(
                        entry.Key, Constants.Eitr,
                        itemDrop.m_itemData.m_shared.m_foodEitr,
                        $"Sets eitr value of {entry.Key}."));

                    ConfigEntries.Add(entry.Key, listConfig);
                }
            }

            CreateConfigEntry(
                "General", "Lock Configuration", true,
                "[Server Only] The configuration is locked and may not be changed by clients once it has been synced from the server. Only valid for server config, will have no effect on clients."
            );

            ValheimFoodConfig.LOG.LogMessage("Finished loading configs");
        }
    }
}