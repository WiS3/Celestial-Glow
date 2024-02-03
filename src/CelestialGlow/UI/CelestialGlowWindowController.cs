using BepInEx.Logging;
using CelestialGlow.Classes;
using CelestialGlow.UI.VisualEelements;
using KSP.Game;
using KSP.UI.Binding;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;
using Logger = BepInEx.Logging.Logger;

namespace CelestialGlow.UI;

/// <summary>
/// Controller for the CelestialGlowWindow UI.
/// </summary>
public class CelestialGlowWindowController : MonoBehaviour
{
    private static readonly ManualLogSource _LOGGER = Logger.CreateLogSource("CelestialGlow");
    // The UIDocument component of the window game object
    private UIDocument _window;

    // The elements of the window that we need to access
    private VisualElement _rootElement;
    private VisualElement _bodiesSettingsElement;

    private Slider _ambientIntensitySlider;
    private Slider _globalAmbientDayIntensity;
    private Slider _globalAmbientNightIntensity;
    private Slider _globalAmbientScaledIntensity;

    private Button _closeButton;

    public ListView celestialBodySettingsListView;

    // The backing field for the IsWindowOpen property
    private bool _isWindowOpen = false;
    public static GameInstance Game = GameManager.Instance.Game;

    /// <summary>
    /// The state of the window. Setting this value will open or close the window.
    /// </summary>
    public bool IsWindowOpen
    {
        get => _isWindowOpen;
        set
        {
            _isWindowOpen = value;

            // Set the display style of the root element to show or hide the window
            _rootElement.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
            // Alternatively, you can deactivate the window game object to close the window and stop it from updating,
            // which is useful if you perform expensive operations in the window update loop. However, this will also
            // mean you will have to re-register any event handlers on the window elements when re-enabled in OnEnable.
            // gameObject.SetActive(value);

            // Update the Flight AppBar button state
            GameObject.Find(CelestialGlowPlugin.ToolbarFlightButtonID)
                ?.GetComponent<UIValue_WriteBool_Toggle>()
                ?.SetValue(value);
        }
    }

    private static void OnVisualElementPointerEnter(PointerEnterEvent evt)
    {
        Game.Input.Flight.Disable();
        Game.Input.MapView.Disable();
        Game.Input.Audio.Disable();
    }
    private static void OnVisualElementPointerLeave(PointerLeaveEvent evt)
    {
        Game.Input.Flight.Enable();
        Game.Input.MapView.Enable();
        Game.Input.Audio.Enable();
    }

    /// <summary>
    /// Runs when the window is first created, and every time the window is re-enabled.
    /// </summary>
    private void OnEnable()
    {
        _window = GetComponent<UIDocument>();
        _rootElement = _window.rootVisualElement[0];
        _rootElement.style.display = DisplayStyle.None;
        _rootElement.CenterByDefault();
        _rootElement.RegisterCallback<PointerEnterEvent>(OnVisualElementPointerEnter);
        _rootElement.RegisterCallback<PointerLeaveEvent>(OnVisualElementPointerLeave);
        _closeButton = _rootElement.Q<Button>("close-button");
        _closeButton.clicked += () => IsWindowOpen = false;

        InitGlobalSettings();
        InitBodiesSettings();
    }

    public void InitGlobalSettings()
    {
        _ambientIntensitySlider = _rootElement.Q<Slider>("ambient-intensity-slider");
        _globalAmbientDayIntensity = _rootElement.Q<Slider>("global-day-intensity-slider");
        _globalAmbientNightIntensity = _rootElement.Q<Slider>("global-night-intensity-slider");
        _globalAmbientScaledIntensity = _rootElement.Q<Slider>("global-scaled-intensity-slider");

        _ambientIntensitySlider.value = CelestialGlowPlugin.Instance.GetAmbientIntensity();
        _globalAmbientDayIntensity.value = CelestialGlowPlugin.Instance.GetGlobalAmbientDayIntensity();
        _globalAmbientNightIntensity.value = CelestialGlowPlugin.Instance.GetGlobalAmbientNightIntensity();
        _globalAmbientScaledIntensity.value = CelestialGlowPlugin.Instance.GetGlobalAmbientScaledIntensity();

        _ambientIntensitySlider.RegisterValueChangedCallback(OnAmbientIntensityChangedEvent);
        _globalAmbientDayIntensity.RegisterValueChangedCallback(OnGlobalDayIntensityChangedEvent);
        _globalAmbientNightIntensity.RegisterValueChangedCallback(OnGlobalNightIntensityChangedEvent);
        _globalAmbientScaledIntensity.RegisterValueChangedCallback(OnGlobalScaledIntensityChangedEvent);
    }
    private void InitBodiesSettings()
    {
        _bodiesSettingsElement = _rootElement.Q<VisualElement>(name: "bodies-settings");

        Func<VisualElement> makeItem = () =>
        {
            var bodySettingsVisualElement = new BodySettingsVisualElement();
            var foldout = bodySettingsVisualElement.Q<CustomFoldout>(name: "foldout");
            foldout.Q<Toggle>(name = "customToggle").RegisterValueChangedCallback(evt =>
            {
                var i = (int)foldout.userData;
                var bodySettings = CelestialGlowPlugin.Instance.bodySettingsList[i];
                bodySettings.overrideGlobal = evt.newValue;
                CelestialGlowPlugin.Instance.UpdateBodyAmbientLighting(bodySettings);
            });

            var ambientDaySlider = bodySettingsVisualElement.Q<Slider>(name: "ambientDaySlider");
            var ambientNightSlider = bodySettingsVisualElement.Q<Slider>(name: "ambientNightSlider");
            var ambientScaledSlider = bodySettingsVisualElement.Q<Slider>(name: "ambientScaledSlider");

            ambientDaySlider.RegisterValueChangedCallback(evt =>
            {
                var i = (int)ambientDaySlider.userData;
                var bodySettings = CelestialGlowPlugin.Instance.bodySettingsList[i];
                bodySettings.ambientDayIntensity = evt.newValue;
                CelestialGlowPlugin.Instance.UpdateBodyAmbientLighting(bodySettings);
            });
            ambientNightSlider.RegisterValueChangedCallback(evt =>
            {
                var i = (int)ambientNightSlider.userData;
                var bodySettings = CelestialGlowPlugin.Instance.bodySettingsList[i];
                bodySettings.ambientNightIntensity = evt.newValue;
                CelestialGlowPlugin.Instance.UpdateBodyAmbientLighting(bodySettings);
            });
            ambientScaledSlider.RegisterValueChangedCallback(evt =>
            {
                var i = (int)ambientScaledSlider.userData;
                var bodySettings = CelestialGlowPlugin.Instance.bodySettingsList[i];
                bodySettings.ambientScaledIntensity = evt.newValue;
                CelestialGlowPlugin.Instance.UpdateBodyAmbientLighting(bodySettings);
            });
            return bodySettingsVisualElement;
        };

        Action<VisualElement, int> bindItem = (e, i) => BindItem(e as BodySettingsVisualElement, i);

        celestialBodySettingsListView = new ListView() {name = "celestial-bodies-listview" };
        celestialBodySettingsListView.itemsSource = CelestialGlowPlugin.Instance.bodySettingsList;
        celestialBodySettingsListView.makeItem = makeItem;
        celestialBodySettingsListView.bindItem = bindItem;
        celestialBodySettingsListView.selectionType = SelectionType.None;
        celestialBodySettingsListView.reorderable = false;
        celestialBodySettingsListView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
        celestialBodySettingsListView.style.flexGrow = 1f;
        celestialBodySettingsListView.showBorder = true;
        _bodiesSettingsElement.Add(celestialBodySettingsListView);
    }

    private void BindItem(BodySettingsVisualElement elem, int i)
    {   
        BodySettings bodySettings = CelestialGlowPlugin.Instance.bodySettingsList[i];

        var foldout = elem.Q<CustomFoldout>(name = "foldout");
        foldout.AddToClassList(bodySettings.name.ToLower());
        foldout.userData = i;
        foldout.text = bodySettings.name;
        foldout.customValue = bodySettings.overrideGlobal;
        var ambientDaySlider = elem.Q<Slider>(name: "ambientDaySlider");
        var ambientNightSlider = elem.Q<Slider>(name: "ambientNightSlider");
        var ambientScaledSlider = elem.Q<Slider>(name: "ambientScaledSlider");
        ambientDaySlider.userData = i;
        ambientDaySlider.value = bodySettings.ambientDayIntensity;
        
        ambientNightSlider.userData = i;
        ambientNightSlider.value = bodySettings.ambientNightIntensity;

        ambientScaledSlider.userData = i;
        ambientScaledSlider.value = bodySettings.ambientScaledIntensity;
    }

    private void OnAmbientIntensityChangedEvent(ChangeEvent<float> evt)
    {
        CelestialGlowPlugin.Instance.SetAmbientIntensity(evt.newValue);
    }

    private void OnGlobalDayIntensityChangedEvent(ChangeEvent<float> evt)
    {
        CelestialGlowPlugin.Instance.SetGlobalAmbientDayIntensity(evt.newValue);
    }
    private void OnGlobalNightIntensityChangedEvent(ChangeEvent<float> evt)
    {
        CelestialGlowPlugin.Instance.SetGlobalAmbientNightIntensity(evt.newValue);
    }
    private void OnGlobalScaledIntensityChangedEvent(ChangeEvent<float> evt)
    {
        CelestialGlowPlugin.Instance.SetGlobalAmbientScaledIntensity(evt.newValue);
    }
}
