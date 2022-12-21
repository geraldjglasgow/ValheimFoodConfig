using System;
using System.Collections.Generic;
using BepInEx;
using IniParser;
using BepInEx.Logging;
using System.IO;
using IniParser.Model;
using Jotunn.Managers;

namespace ValheimFoodConfig {
    [BepInPlugin(pluginGUID, pluginName, pluginVersion)]
    [BepInDependency("com.jotunn.jotunn", BepInDependency.DependencyFlags.HardDependency)]
    
    public class ValheimFoodConfig : BaseUnityPlugin {
        const string pluginGUID = "com.ValheimFoodConfig";
        const string pluginName = "ValheimFoodConfig";
        const string pluginVersion = "1.1.0";
        public static ManualLogSource logger;
        public static string FILE_NAME = String.Format("{0}.cfg", pluginName);
        public static string configPath = Path.GetDirectoryName(Paths.BepInExConfigPath) + Path.DirectorySeparatorChar + FILE_NAME;
        public static string SECTION_FOOD = "Food";
        public static string SECTION_CUSTOM_FOOD = "Custom Food";
        public static string HEALTH_SUFFIX = ".Health";
        public static string DURATION_SUFFIX = ".Duration";
        public static string STAMINA_SUFFIX = ".Stamina";
        public static string HEALTHREGEN_SUFFIX = ".HealthRegen";
        public static string EITR_SUFFIX = ".Eitr";
        public static List<string> FOOD_PREFAB_NAMES = new List<string>
            { "Raspberry", "Blueberries", "Blueberries", "Cloudberry", "Honey", "Carrot", "Onion", "Mushroom", "MushroomYellow", "MushroomBlue", "MushroomJotunPuffs", "MushroomMagecap", "NeckTailGrilled", "CookedMeat", "FishCooked", "CookedWolfMeat",
            "CookedDeerMeat", "SerpentMeatCooked", "CookedLoxMeat", "CookedBugMeat", "CookedChickenMeat", "CookedHareMeat", "WolfJerky", "BoarJerky", "WolfMeatSkewer", "Sausages", "MeatPlatter", "HoneyGlazedChicken", "MisthareSupreme", "MinceMeatSauce",
            "SerpentStew", "DeerStew", "TurnipStew", "CarrotSoup", "BlackSoup", "OnionSoup", "SeekerAspic", "LoxPie", "MushroomOmelette", "FishWraps", "FishAndBread", "Salad", "BloodPudding", "YggdrasilPorridge", "Bread", "QueensJam",
            "MagicallyStuffedShroom", "Eyescream", "ShocklateSmoothie", "CookedEgg" };
        void Awake() {
            logger = Logger;
            PrefabManager.OnVanillaPrefabsAvailable += LoadConfig;
            PrefabManager.OnVanillaPrefabsAvailable += UpdateVanillaPrefabValues;
            ZoneManager.OnVanillaLocationsAvailable += UpdateCustomPrefabValues;
        }

        private void LoadConfig() {
            if (!File.Exists(configPath)) {
                CreateConfigFile();
            }
        }

        private void UpdateVanillaPrefabValues() {
            var parser = new FileIniDataParser();
            IniData data = parser.ReadFile(configPath);
            IEnumerator<KeyData> enumerator = data[SECTION_FOOD].GetEnumerator();
            while (enumerator.MoveNext()) {
                KeyData keyData = enumerator.Current;
                try {
                    string[] tokens = keyData.KeyName.Split('.');
                    if (tokens[1].Equals("Health")) {
                        PrefabManager.Cache.GetPrefab<ItemDrop>(tokens[0]).m_itemData.m_shared.m_food = float.Parse(keyData.Value);
                    }
                    if (tokens[1].Equals("Stamina")) {
                        PrefabManager.Cache.GetPrefab<ItemDrop>(tokens[0]).m_itemData.m_shared.m_foodStamina = float.Parse(keyData.Value);
                    }
                    if (tokens[1].Equals("Duration")) {
                        PrefabManager.Cache.GetPrefab<ItemDrop>(tokens[0]).m_itemData.m_shared.m_foodBurnTime = float.Parse(keyData.Value);
                    }
                    if (tokens[1].Equals("HealthRegen")) {
                        PrefabManager.Cache.GetPrefab<ItemDrop>(tokens[0]).m_itemData.m_shared.m_foodRegen = float.Parse(keyData.Value);
                    }
                    if (tokens[1].Equals("Eitr")) {
                        PrefabManager.Cache.GetPrefab<ItemDrop>(tokens[0]).m_itemData.m_shared.m_foodEitr = float.Parse(keyData.Value);
                    }
                } catch (Exception e) {
                    logger.LogInfo($"Loading config for {keyData.KeyName} failed. {e.Message} {e.StackTrace}");
                }
            }

            logger.LogInfo("Finished updating vanilla prefabs");
        }

        private void UpdateCustomPrefabValues() {
            var parser = new FileIniDataParser();
            IniData data = parser.ReadFile(configPath);
            IEnumerator<KeyData> enumerator = data[SECTION_CUSTOM_FOOD].GetEnumerator();
            while(enumerator.MoveNext()) {
                KeyData keyData = enumerator.Current;
                try {
                    string[] tokens = keyData.KeyName.Split('.');
                    if (tokens[1].Equals("Health")) {
                        PrefabManager.Cache.GetPrefab<ItemDrop>(tokens[0]).m_itemData.m_shared.m_food = float.Parse(keyData.Value);
                    }
                    if (tokens[1].Equals("Stamina")) {
                        PrefabManager.Cache.GetPrefab<ItemDrop>(tokens[0]).m_itemData.m_shared.m_foodStamina = float.Parse(keyData.Value);
                    }
                    if (tokens[1].Equals("Duration")) {
                        PrefabManager.Cache.GetPrefab<ItemDrop>(tokens[0]).m_itemData.m_shared.m_foodBurnTime = float.Parse(keyData.Value);
                    }
                    if (tokens[1].Equals("HealthRegen")) {
                        PrefabManager.Cache.GetPrefab<ItemDrop>(tokens[0]).m_itemData.m_shared.m_foodRegen = float.Parse(keyData.Value);
                    }
                    if (tokens[1].Equals("Eitr")) {
                        PrefabManager.Cache.GetPrefab<ItemDrop>(tokens[0]).m_itemData.m_shared.m_foodEitr = float.Parse(keyData.Value);
                    }
                } catch (Exception e) {
                    logger.LogInfo($"Loading config for {keyData.KeyName} failed. {e.Message} {e.StackTrace}");
                }
            }

            logger.LogInfo("Finished updating custom prefabs");
        }

        private void CreateConfigFile() {
            var parser = new FileIniDataParser();

            IniData data = new IniData();
            data.Sections.AddSection(SECTION_FOOD);
            data.Sections.AddSection(SECTION_CUSTOM_FOOD);
            foreach (String foodPrefabName in FOOD_PREFAB_NAMES) {
                var foodPrefab = PrefabManager.Cache.GetPrefab<ItemDrop>(foodPrefabName);
                data[SECTION_FOOD][String.Format("{0}{1}", foodPrefabName, HEALTH_SUFFIX)] = foodPrefab.m_itemData.m_shared.m_food.ToString();
                data[SECTION_FOOD][String.Format("{0}{1}", foodPrefabName, STAMINA_SUFFIX)] = foodPrefab.m_itemData.m_shared.m_foodStamina.ToString();
                data[SECTION_FOOD][String.Format("{0}{1}", foodPrefabName, DURATION_SUFFIX)] = foodPrefab.m_itemData.m_shared.m_foodBurnTime.ToString();
                data[SECTION_FOOD][String.Format("{0}{1}", foodPrefabName, HEALTHREGEN_SUFFIX)] = foodPrefab.m_itemData.m_shared.m_foodRegen.ToString();
                data[SECTION_FOOD][String.Format("{0}{1}", foodPrefabName, EITR_SUFFIX)] = foodPrefab.m_itemData.m_shared.m_foodEitr.ToString();
            }

            parser.WriteFile(configPath, data);
            logger.LogInfo(String.Format("Finished creating {0}", FILE_NAME));
        }
    }
}
