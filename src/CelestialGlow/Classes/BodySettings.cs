using BepInEx.Configuration;
using UnityEngine;

namespace CelestialGlow.Classes
{
    [Serializable]
    public class BodySettings
    {
        public BodySettings(string name, bool overrideGlobal, float dayIntensity, float nightIntensity, float scaledIntensity)
        {
            _name = name;
            _overrideGlobal = Configuration.Instance.BindBoolConfigValue($"{_name}_overrideGlobal", overrideGlobal, "");
            _ambientDayIntensity = Configuration.Instance.BindFloatConfigValue($"{_name}_ambientDayIntensity", dayIntensity, "");
            _ambientNightIntensity = Configuration.Instance.BindFloatConfigValue($"{_name}_ambientNightIntensity", nightIntensity, "");
            _ambientScaledIntensity = Configuration.Instance.BindFloatConfigValue($"{_name}_ambientScaledIntensity", scaledIntensity, "");
        }

        private string _name;
        private Color _defaultAmbientDayColor;
        private Color _defaultAmbientNightColor;
        private Color _defaultAmbientScaledColor;

        private ConfigEntry<bool> _overrideGlobal;
        private ConfigEntry<float> _ambientDayIntensity;
        private ConfigEntry<float> _ambientNightIntensity;
        private ConfigEntry<float> _ambientScaledIntensity;

        public string name
        {
            get { return _name; }
            private set
            {
                _name = value;
            }
        }

        public Color ambientDayColor
        {
            get { return Color.white * _ambientDayIntensity.Value; }
        }

        public Color ambientNightColor
        {
            get { return Color.white * _ambientNightIntensity.Value; }
        }

        public Color ambientScaledColor
        {
            get { return Color.white * _ambientScaledIntensity.Value; }
        }

        public Color defaultAmbientDayColor
        {
            get { return _defaultAmbientDayColor; }
            set
            {
                _defaultAmbientDayColor = value;
            }
        }

        public Color defaultAmbientNightColor
        {
            get { return _defaultAmbientNightColor; }
            set
            {
                _defaultAmbientNightColor = value;
            }
        }

        public Color defaultAmbientScaledColor
        {
            get { return _defaultAmbientScaledColor; }
            set
            {
                _defaultAmbientScaledColor = value;
            }
        }

        public bool overrideGlobal
        {
            get { return _overrideGlobal.Value; }
            set
            {
                _overrideGlobal.Value = value;
            }
        }

        public float ambientDayIntensity
        {
            get { return _ambientDayIntensity.Value; }
            set
            {
                _ambientDayIntensity.Value = value;
            }
        }

        public float ambientNightIntensity
        {
            get { return _ambientNightIntensity.Value; }
            set
            {
                _ambientNightIntensity.Value = value;
            }
        }
        public float ambientScaledIntensity
        {
            get { return _ambientScaledIntensity.Value; }
            set
            {
                _ambientScaledIntensity.Value = value;
            }
        }
    }
}