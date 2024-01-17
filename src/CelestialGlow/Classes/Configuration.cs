using BepInEx;
using BepInEx.Configuration;
using SpaceWarp;

namespace CelestialGlow.Classes
{
    public class Configuration
    {
        public static Configuration Instance;
        private ConfigFile config;
        public Configuration(ConfigFile config)
        {
            Instance = this;
            this.config = config;
            InitGlobalConfig();
        }

        public static readonly float defaultAmbientLightIntensity = 1f;
        public static readonly float defaultGlobalAmbientDayIntensity = 0.025f;
        public static readonly float defaultGlobalAmbientNightIntensity = 0.025f;
        public static readonly float defaultGlobalAmbientScaledIntensity = 0.005f;

        public ConfigEntry<float> ambientLightIntensity;
        public ConfigEntry<float> globalAmbientDayIntensity;
        public ConfigEntry<float> globalAmbientNightIntensity;
        public ConfigEntry<float> globalAmbientScaledIntensity;

        public ConfigEntry<float> BindFloatConfigValue(string name, float _default, string desc)
        {
            return config.Bind(
                MyPluginInfo.PLUGIN_NAME,
                name,
                _default,
                desc
            );
        }
        public ConfigEntry<bool> BindBoolConfigValue(string name, bool _default, string desc)
        {
            return config.Bind(
                MyPluginInfo.PLUGIN_NAME,
                name,
                _default,
                desc
            );
        }

        private void InitGlobalConfig()
        {
            ambientLightIntensity = BindFloatConfigValue("ambientLightIntensity",defaultAmbientLightIntensity,"");
            globalAmbientDayIntensity = BindFloatConfigValue("global_ambientDayIntensity",defaultGlobalAmbientDayIntensity,"");
            globalAmbientNightIntensity = BindFloatConfigValue("global_ambientNightIntensity",defaultGlobalAmbientNightIntensity,"");
            globalAmbientScaledIntensity = BindFloatConfigValue("global_ambientScaledIntensity",defaultGlobalAmbientScaledIntensity,"");
        }
    }
}
