using BepInEx;
using BepInEx.Logging;
using JetBrains.Annotations;
using SpaceWarp;
using SpaceWarp.API.Assets;
using SpaceWarp.API.Mods;
using SpaceWarp.API.UI.Appbar;
using CelestialGlow.UI;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;
using KSP.Game;
using KSP.Rendering;
using KSP.Messages;
using KSP.Sim.Definitions;
using CelestialGlow.Classes;
using KSP.Sim.impl;

namespace CelestialGlow;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
public class CelestialGlowPlugin : BaseSpaceWarpPlugin
{
    private static readonly ManualLogSource _LOGGER = BepInEx.Logging.Logger.CreateLogSource("CelestialGlow");

    // Useful in case some other mod wants to use this mod a dependency
    [PublicAPI] public const string ModGuid = MyPluginInfo.PLUGIN_GUID;
    [PublicAPI] public const string ModName = MyPluginInfo.PLUGIN_NAME;
    [PublicAPI] public const string ModVer = MyPluginInfo.PLUGIN_VERSION;

    /// Singleton instance of the plugin class
    [PublicAPI] public static CelestialGlowPlugin Instance { get; set; }

    public Configuration configuration;
    public MessageCenter MessageCenter => GameManager.Instance?.Game?.Messages;
    // AppBar button IDs
    internal const string ToolbarFlightButtonID = "BTN-CelestialGlowFlight";
    internal const string ToolbarKscButtonID = "BTN-CelestialGlowKSC";

    public List<BodySettings> bodySettingsList;
    Dictionary<string, CelestialBodyLightingData> bodyLightingDict;

    public CelestialGlowWindowController celestialGlowWindowController;

    /// <summary>
    /// Runs when the mod is first initialized.
    /// </summary>
    public override void OnInitialized()
    {
        base.OnInitialized();
        Instance = this;
        configuration = new Configuration(Config);

        // Load the UI from the asset bundle
        var CelestialGlowWindowUxml = AssetManager.GetAsset<VisualTreeAsset>(
            // The case-insensitive path to the asset in the bundle is composed of:
            // - The mod GUID:
            $"{ModGuid}/" +
            // - The name of the asset bundle:
            "celestialglow/" +
            // - The path to the asset in your Unity project (without the "Assets/" part)
            "ui/CelestialGlowWindow.uxml"
        );

        var windowOptions = new WindowOptions
        {
            // The ID of the window. It should be unique to your mod.
            WindowId = "CelestialGlow_Window",
            // The transform of parent game object of the window.
            // If null, it will be created under the main canvas.
            Parent = null,
            // Whether or not the window can be hidden with F2.
            IsHidingEnabled = true,
            // Whether to disable game input when typing into text fields.
            DisableGameInputForTextFields = true,
            MoveOptions = new MoveOptions
            {
                // Whether or not the window can be moved by dragging.
                IsMovingEnabled = true,
                // Whether or not the window can only be moved within the screen bounds.
                CheckScreenBounds = true
            }
        };

        bodySettingsList = new List<BodySettings>();
        bodyLightingDict = new Dictionary<string, CelestialBodyLightingData>();

        var celestialGlowWindow = Window.Create(windowOptions, CelestialGlowWindowUxml);
        celestialGlowWindowController = celestialGlowWindow.gameObject.AddComponent<CelestialGlowWindowController>();

        // Register Flight AppBar button
        Appbar.RegisterAppButton(
            "Celestial Glow",
            ToolbarFlightButtonID,
            AssetManager.GetAsset<Texture2D>($"{ModGuid}/images/icon.png"),
            isOpen => celestialGlowWindowController.IsWindowOpen = isOpen
        );

        // Register KSC AppBar Button
        Appbar.RegisterKSCAppButton(
            "Celestial Glow",
            ToolbarKscButtonID,
            AssetManager.GetAsset<Texture2D>($"{ModGuid}/images/icon.png"),
            () => celestialGlowWindowController.IsWindowOpen = !celestialGlowWindowController.IsWindowOpen
        );

        SubscribeToMessages();
    }

    public void SubscribeToMessages() => _ = Subscribe();
    private async Task Subscribe()
    {
        await Task.Delay(100);

        MessageCenter.PersistentSubscribe<KSP.Messages.CelestialBodyDataAddedMessage>(OnCelestialBodyDataAddedMessage);
        _LOGGER.LogInfo("Subscribed to CelestialBodyDataAddedMessage");

        MessageCenter.PersistentSubscribe<KSP.Messages.CelestialBodyDataRemovedMessage>(OnCelestialBodyDataRemovedMessage);
        _LOGGER.LogInfo("Subscribed to CelestialBodyDataRemovedMessage");

        MessageCenter.PersistentSubscribe<CelestialBodiesLoadedMessage>(OnCelestialBodiesLoadedMessage);
        _LOGGER.LogInfo("Subscribed to CelestialBodiesLoadedMessage");
    }

    private void OnCelestialBodyDataAddedMessage(MessageCenterMessage msg)
    {
        CelestialBodyDataAddedMessage message = msg as CelestialBodyDataAddedMessage;
        string bodyName = message.AddedBodyName;
        //_LOGGER.LogInfo($"CelestialBodyDataAddedMessage: {bodyName}, {(message.LightingData != null ? "GotLightingData" : "NoLightingData")}");
        if (bodyName != "Kerbol" && bodyLightingDict.TryAdd(bodyName, message.LightingData))
        {
            BodySettings bodySettings = bodySettingsList.Find(body => body.name == bodyName);
            if (bodySettings != null)
            {
                bodySettings.defaultAmbientDayColor = message.LightingData.ambientDay;
                bodySettings.defaultAmbientNightColor = message.LightingData.ambientNight;
                bodySettings.defaultAmbientScaledColor = message.LightingData.ambientScaled;
                UpdateBodyAmbientLighting(bodySettings);
                //_LOGGER.LogInfo($"Something has gone horribly wrong.. bodySettings is NULL");
            }
        }
    }
    private void OnCelestialBodyDataRemovedMessage(MessageCenterMessage msg)
    {
        CelestialBodyDataRemovedMessage message = msg as CelestialBodyDataRemovedMessage;
        string bodyName = message.RemovedBodyName;
        bodyLightingDict.Remove(bodyName);
    }
    public void OnCelestialBodiesLoadedMessage(MessageCenterMessage msg)
    {
        CelestialBodiesLoadedMessage message = msg as CelestialBodiesLoadedMessage;
        if (message != null)
        {
            Dictionary<string, CelestialBodyCore> celestialBodies = GameManager.Instance.Game.CelestialBodies.GetAllBodiesData();
            foreach (CelestialBodyCore v in celestialBodies.Values)
            {
                if (v.data.bodyName != "Kerbol")
                {
                    //_LOGGER.LogInfo($"Body: {v.data.bodyName}");
                    AddBodySettings(v.data.bodyName);
                }
            }
        }
    }

    private void AddBodySettings(string bodyName)
    {
        BodySettings bodySettings = new BodySettings(bodyName, false, Configuration.defaultGlobalAmbientDayIntensity, Configuration.defaultGlobalAmbientNightIntensity, Configuration.defaultGlobalAmbientScaledIntensity);
        bodySettingsList.Add(bodySettings);
    }

    public float GetAmbientIntensity()
    {
        return configuration.ambientLightIntensity.Value;
    }
    public void SetAmbientIntensity(float value)
    {
        configuration.ambientLightIntensity.Value = value;
        UpdateAmbientLighting();
    }

    public float GetGlobalAmbientDayIntensity()
    {
        return configuration.globalAmbientDayIntensity.Value;
    }
    public float GetGlobalAmbientNightIntensity()
    {
        return configuration.globalAmbientNightIntensity.Value;
    }
    public float GetGlobalAmbientScaledIntensity()
    {
        return configuration.globalAmbientScaledIntensity.Value;
    }

    public void SetGlobalAmbientDayIntensity(float value)
    {
        configuration.globalAmbientDayIntensity.Value = value;
        UpdateBodiesAmbientLighting();
    }
    public void SetGlobalAmbientNightIntensity(float value)
    {
        configuration.globalAmbientNightIntensity.Value = value;
        UpdateBodiesAmbientLighting();
    }
    public void SetGlobalAmbientScaledIntensity(float value)
    {
        configuration.globalAmbientScaledIntensity.Value = value;
        UpdateBodiesAmbientLighting();
    }

    public void UpdateBodyAmbientLighting(BodySettings bodySettings)
    {
        if (bodyLightingDict.ContainsKey(bodySettings.name))
        {
            CelestialBodyLightingData bodyLighting = bodyLightingDict[bodySettings.name];
            if (bodySettings.overrideGlobal)
            {
                bodyLighting.ambientDay = bodySettings.ambientDayColor;
                bodyLighting.ambientNight = bodySettings.ambientNightColor;
                bodyLighting.ambientScaled = bodySettings.ambientScaledColor;
            }
            else
            {
                bodyLighting.ambientDay = Color.white * configuration.globalAmbientDayIntensity.Value;
                bodyLighting.ambientNight = Color.white * configuration.globalAmbientNightIntensity.Value;
                bodyLighting.ambientScaled = Color.white * configuration.globalAmbientScaledIntensity.Value;
            }
        }
    }
    private void UpdateBodiesAmbientLighting()
    {
        foreach(BodySettings bodySettings in bodySettingsList)
        {
            UpdateBodyAmbientLighting(bodySettings);
        }
    }
    private void UpdateAmbientLighting()
    {
        RenderSettings.ambientIntensity = configuration.ambientLightIntensity.Value;
    }
}
