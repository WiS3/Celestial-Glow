using UnityEngine.UIElements;

namespace CelestialGlow.UI.VisualEelements
{
    internal class BodySettingsVisualElement : VisualElement
    {
        public BodySettingsVisualElement()
        {
            var root = new VisualElement() { name = "body-settings" };
            root.style.flexGrow = 1;

            var foldout = new CustomFoldout() { name = "foldout", value = false, customText = "Override Global?" };

            var container = new VisualElement();
            container.style.flexGrow = 1;
            container.style.flexDirection = FlexDirection.Column;
            container.style.paddingLeft = 15f;
            container.style.paddingRight = 15f;

            // Day
            var ambientDaySlider = new Slider { name = "ambientDaySlider", lowValue = 0f, highValue = 3f };
            ambientDaySlider.label = "Day Intensity";
            ambientDaySlider.lowValue = 0f;
            ambientDaySlider.highValue = 3f;
            ambientDaySlider.showInputField = true;
            ambientDaySlider.style.flexGrow = 1f;

            // Night
            var ambientNightSlider = new Slider { name = "ambientNightSlider", lowValue = 0f, highValue = 3f };
            ambientNightSlider.label = "Night Intensity";
            ambientNightSlider.style.flexGrow = 1f;
            ambientNightSlider.lowValue = 0f;
            ambientNightSlider.highValue = 3f;
            ambientNightSlider.showInputField = true;

            // Scaled
            var ambientScaledSlider = new Slider { name = "ambientScaledSlider", lowValue = 0f, highValue = 3f };
            ambientScaledSlider.label = "Scaled Intensity";
            ambientScaledSlider.lowValue = 0f;
            ambientScaledSlider.style.flexGrow = 1f;
            ambientScaledSlider.highValue = 1f;
            ambientScaledSlider.showInputField = true;

            container.Add(ambientDaySlider);
            container.Add(ambientNightSlider);
            container.Add(ambientScaledSlider);
            foldout.Add(container);
            root.Add(foldout);
            Add(root);
        }
    }
}
