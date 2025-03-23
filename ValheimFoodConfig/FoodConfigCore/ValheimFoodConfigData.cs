using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using Jotunn.Managers;
using ServerSync;
using Object = UnityEngine.Object;

namespace ValheimFoodConfig
{
    public static class ValheimFoodConfigData
    {
        public static Dictionary<string, List<ConfigEntry<float>>> ConfigEntries { get; }
            = new Dictionary<string, List<ConfigEntry<float>>>();

        public static readonly Dictionary<string, MeadEffectConfig> MeadConfigs
            = new Dictionary<string, MeadEffectConfig>();

        private const string ModifiersGroup = "0_Modifiers";
        public static ConfigEntry<float> HealthModifier { get; private set; }
        public static ConfigEntry<float> StaminaModifier { get; private set; }
        public static ConfigEntry<float> DurationModifier { get; private set; }
        public static ConfigEntry<float> HealthRegenModifier { get; private set; }
        public static ConfigEntry<float> EitrModifier { get; private set; }
        public static ConfigEntry<bool> DisableFoodDegradation { get; private set; }

        private static ConfigFile _configFile;
        private static ConfigSync _configSync;

        public static void Initialize(ConfigFile configFile, ConfigSync configSync)
        {
            _configFile = configFile;
            _configSync = configSync;
            HealthModifier = CreateConfigEntry(ModifiersGroup, "Health Modifier", 1f,
                "Percentage to modify health value of all foods.");
            StaminaModifier = CreateConfigEntry(ModifiersGroup, "Stamina Modifier", 1f,
                "Percentage to modify stamina value of all foods.");
            DurationModifier = CreateConfigEntry(ModifiersGroup, "Duration Modifier", 1f,
                "Percentage to modify duration of all foods.");
            HealthRegenModifier = CreateConfigEntry(ModifiersGroup, "Health Regen Modifier", 1f,
                "Percentage to modify health regeneration of all foods.");
            EitrModifier = CreateConfigEntry(ModifiersGroup, "Eitr Modifier", 1f,
                "Percentage to modify eitr value of all foods.");
            DisableFoodDegradation = CreateConfigEntry(ModifiersGroup, "Disable Food Degradation", false,
                "Set to true to disable food degradation.");
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
            LoadMeadConfiguration();
        }

        private static void LoadMeadConfiguration()
        {
            foreach (var meadName in Constants.MeadNames)
            {
                var itemDrop = PrefabManager.Cache.GetPrefab<ItemDrop>(meadName);
                if (itemDrop == null) continue;
                var effect = itemDrop.m_itemData.m_shared.m_consumeStatusEffect;
                float duration = 600f;
                float heathOverTime = 0f;
                float staminaOverTime = 0f;
                if (effect is SE_Stats stats)
                {
                    duration = stats.m_ttl;
                    heathOverTime = stats.m_healthOverTime;
                    staminaOverTime = stats.m_staminaOverTime;
                }

                string group = $"0Meads_{meadName}";
                var configObj = new MeadEffectConfig
                {
                    Duration = CreateConfigEntry(
                        group,
                        "Duration",
                        duration,
                        $"How many seconds {meadName} effect lasts"
                    ),

                    HealthOverTime = CreateConfigEntry(
                        group,
                        "HealthOverTime",
                        heathOverTime,
                        $"How much total health is restored over time by {meadName}"
                    ),
                    StaminaOverTime = CreateConfigEntry(
                        group,
                        "StaminaOverTime",
                        staminaOverTime,
                        $"How much total stamina is restored over time by {meadName}"
                    )
                };

                MeadConfigs[meadName] = configObj;
            }
        }

        private static void LoadFoodConfigurations()
        {
            if (ConfigEntries.Count > 0) return;

            ValheimFoodConfig.LOG.LogMessage("Loading configs...");
            Dictionary<string, Object> itemDrops = PrefabManager.Cache.GetPrefabs(typeof(ItemDrop));

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