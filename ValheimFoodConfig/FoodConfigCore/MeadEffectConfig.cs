using BepInEx.Configuration;

namespace ValheimFoodConfig
{
    public class MeadEffectConfig
    {
        public ConfigEntry<float> Duration;        // m_ttl
        public ConfigEntry<float> HealthOverTime;  // m_healthOverTime
        public ConfigEntry<float> StaminaOverTime; // m_staminaOverTime
    }
}