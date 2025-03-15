using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Jotunn;
using Jotunn.Managers;
using ServerSync;

namespace ValheimFoodConfig
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(Main.ModGuid)]
    public class ValheimFoodConfig : BaseUnityPlugin
    {
        private const string PluginGuid = "com.ValheimFoodConfig";
        private const string PluginName = "ValheimFoodConfig";
        private const string PluginVersion = "2.1.2";
        public static readonly ManualLogSource LOG = BepInEx.Logging.Logger.CreateLogSource(PluginName);

        private ConfigSync _configSync = new ConfigSync(PluginGuid)
        {
            DisplayName = PluginName,
            CurrentVersion = PluginVersion,
            MinimumRequiredVersion = PluginVersion
        };

        void Awake()
        {
            ValheimFoodConfigData.Initialize(Config, _configSync);
            PrefabManager.OnVanillaPrefabsAvailable += ValheimFoodConfigData.LoadConfigurations;
            Assembly assembly = Assembly.GetExecutingAssembly();
            Harmony harmony = new Harmony(PluginGuid);
            harmony.PatchAll(assembly);
        }
    }
}