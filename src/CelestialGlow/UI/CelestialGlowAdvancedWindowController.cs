using BepInEx.Logging;
using CelestialGlow.Classes;
using KSP.UI.Binding;
using Moq;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;
using Logger = BepInEx.Logging.Logger;

namespace CelestialGlow.UI;

/// <summary>
/// Controller for the CelestialGlowWindow UI.
/// </summary>
public class CelestialGlowAdvancedWindowController : MonoBehaviour
{
    private static readonly ManualLogSource _LOGGER = Logger.CreateLogSource("CelestialGlow");
    // The UIDocument component of the window game object
    private UIDocument _window;
    // The elements of the window that we need to access
    private VisualElement _rootElement;
    private VisualElement _contentElement;
    private Button _closeButton;
    public ListView _cblsListView;

    // The backing field for the IsWindowOpen property
    private bool _isWindowOpen = false;

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
        }
    }

    public void Awake()
    {
        _LOGGER.LogInfo("Awake called from CelestialGlowWindowController");
    }

    /// <summary>
    /// Runs when the window is first created, and every time the window is re-enabled.
    /// </summary>
    private void OnEnable()
    {
        // Get the UIDocument component from the game object
        _window = GetComponent<UIDocument>();

        // Get the root element of the window.
        // Since we're cloning the UXML tree from a VisualTreeAsset, the actual root element is a TemplateContainer,
        // so we need to get the first child of the TemplateContainer to get our actual root VisualElement.
        _rootElement = _window.rootVisualElement[0];
        _rootElement.style.display = DisplayStyle.None;
        _rootElement.CenterByDefault();

        _contentElement = _rootElement.Q<VisualElement>("content");

        _closeButton = _rootElement.Q<Button>("close-button");
        _closeButton.clicked += () => IsWindowOpen = false;

        Func<VisualElement> makeItem = () =>
        {
            var cblsVisualElement = new CGBodySettings();
            var foldout = cblsVisualElement.Q<CustomFoldout>(name: "foldout");
            foldout.Q<Toggle>(name = "customToggle").RegisterValueChangedCallback(evt =>
            {
                var i = (int)foldout.userData;
                var cbData = CelestialGlowPlugin.Instance.cgDataList[i];
                cbData.overrideGlobal = evt.newValue;
                CelestialGlowPlugin.Instance.UpdateBodyCelestialLighting(cbData);
                CelestialGlowPlugin.Instance.UpdateMapBodyCelestialLighting(cbData);
            });

            var ambientDaySlider = cblsVisualElement.Q<Slider>(name: "ambientDaySlider");
            ambientDaySlider.RegisterValueChangedCallback(evt =>
            {
                var i = (int)ambientDaySlider.userData;
                var cbData = CelestialGlowPlugin.Instance.cgDataList[i];
                cbData.dayIntensity = evt.newValue;
                //_LOGGER.LogInfo($"Changed dayIntensity value for body {cbData.name}");
                CelestialGlowPlugin.Instance.UpdateBodyCelestialLighting(cbData);
                CelestialGlowPlugin.Instance.UpdateMapBodyCelestialLighting(cbData);

            });
            var ambientNightSlider = cblsVisualElement.Q<Slider>(name: "ambientNightSlider");
            ambientNightSlider.RegisterValueChangedCallback(evt =>
            {
                var i = (int)ambientNightSlider.userData;
                var cbData = CelestialGlowPlugin.Instance.cgDataList[i];
                cbData.nightIntensity = evt.newValue;
                //_LOGGER.LogInfo($"Changed nightIntensity value for body {cbData.name}");
                CelestialGlowPlugin.Instance.UpdateBodyCelestialLighting(cbData);
                CelestialGlowPlugin.Instance.UpdateMapBodyCelestialLighting(cbData);
            });
            return cblsVisualElement;
        };

        Action<VisualElement, int> bindItem = (e, bodyName) => BindItem(e as CGBodySettings, bodyName);

        _cblsListView = new ListView();
        _cblsListView.name = "celestial-bodies-listview";
        _cblsListView.itemsSource = CelestialGlowPlugin.Instance.cgDataList;
        _cblsListView.makeItem = makeItem;
        _cblsListView.bindItem = bindItem;
        _cblsListView.selectionType = SelectionType.None;
        _cblsListView.reorderable = false;
        _cblsListView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
        _cblsListView.style.flexGrow = 1f; // Fills the window, at least until the toggle below.
        _cblsListView.showBorder = true;
        _contentElement.Add(_cblsListView);
    }

    // The ListView calls this if a new item becomes visible when the item first appears on the screen, 
    // when a user scrolls, or when the dimensions of the scroller are changed.
    
    private void BindItem(CGBodySettings elem, int i)
    {
        CGData cbData = CelestialGlowPlugin.Instance.cgDataList[i];

        var foldout = elem.Q<CustomFoldout>(name: "foldout");
        foldout.userData = i;
        foldout.text = cbData.name;
        foldout.customValue = cbData.overrideGlobal;
        var ambientDaySlider = elem.Q<Slider>(name: "ambientDaySlider");
        ambientDaySlider.userData = i;
        ambientDaySlider.value = cbData.dayIntensity;
        var ambientNightSlider = elem.Q<Slider>(name: "ambientNightSlider");
        ambientNightSlider.userData = i;
        ambientNightSlider.value = cbData.nightIntensity;
    }
}
