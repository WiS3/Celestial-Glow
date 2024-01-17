using UnityEngine;
using UnityEngine.UIElements;

namespace CelestialGlow.UI
{

    public class CustomFoldout : BindableElement, INotifyValueChanged<bool>
    {
        public new class UxmlFactory : UxmlFactory<CustomFoldout, UxmlTraits> { }
        public new class UxmlTraits : BindableElement.UxmlTraits
        {
            private UxmlStringAttributeDescription m_customText = new UxmlStringAttributeDescription
            {
                name = "customText"
            };

            private UxmlBoolAttributeDescription m_customValue = new UxmlBoolAttributeDescription
            {
                name = "customValue",
                defaultValue = true
            };

            private UxmlStringAttributeDescription m_Text = new UxmlStringAttributeDescription
            {
                name = "text"
            };

            private UxmlBoolAttributeDescription m_Value = new UxmlBoolAttributeDescription
            {
                name = "value",
                defaultValue = true
            };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                if (ve is CustomFoldout foldout)
                {
                    foldout.text = m_Text.GetValueFromBag(bag, cc);
                    foldout.customText = m_customText.GetValueFromBag(bag, cc);
                    foldout.SetCustomValueWithoutNotify(m_customValue.GetValueFromBag(bag, cc));
                    foldout.SetValueWithoutNotify(m_Value.GetValueFromBag(bag, cc));
                }
            }
        }

        private Toggle m_Toggle;
        private Toggle m_customToggle;

        private VisualElement m_Container;

        [SerializeField]
        private bool m_Value;
        private bool m_customValue;

        public static readonly string ussClassName = "unity-foldout";
        public static readonly string toggleUssClassName = ussClassName + "__toggle";
        public static readonly string contentUssClassName = ussClassName + "__content";
        public static readonly string inputUssClassName = ussClassName + "__input";
        public static readonly string checkmarkUssClassName = ussClassName + "__checkmark";
        public static readonly string textUssClassName = ussClassName + "__text";

        internal static readonly string toggleInspectorUssClassName = toggleUssClassName + "--inspector";
        internal static readonly string ussFoldoutDepthClassName = ussClassName + "--depth-";
        internal static readonly int ussFoldoutMaxDepth = 4;

        internal Toggle toggle => m_Toggle;
        internal Toggle custom_toggle => m_customToggle;

        public override VisualElement contentContainer => m_Container;

        public string text
        {
            get
            {
                return m_Toggle.text;
            }
            set
            {
                m_Toggle.text = value;
            }
        }

        public bool value
        {
            get
            {
                return m_Value;
            }
            set
            {
                if (m_Value == value)
                {
                    return;
                }

                using ChangeEvent<bool> changeEvent = ChangeEvent<bool>.GetPooled(m_Value, value);
                changeEvent.target = this;
                SetValueWithoutNotify(value);
                SendEvent(changeEvent);
            }
        }

        public string customText
        {
            get
            {
                return m_customToggle.label;
            }
            set
            {
                m_customToggle.label = value;
            }
        }

        public bool customValue
        {
            get
            {
                return m_customValue;
            }
            set
            {
                if (m_customValue == value)
                {
                    return;
                }

                using ChangeEvent<bool> changeEvent = ChangeEvent<bool>.GetPooled(m_customValue, value);
                changeEvent.target = this;
                SetCustomValueWithoutNotify(value);
                SendEvent(changeEvent);
            }
        }

        public void SetValueWithoutNotify(bool newValue)
        {
            m_Value = newValue;
            m_Toggle.SetValueWithoutNotify(m_Value);
            contentContainer.style.display = ((!newValue) ? DisplayStyle.None : DisplayStyle.Flex);
        }

        public void SetCustomValueWithoutNotify(bool newValue)
        {
            m_customValue = newValue;
            m_Container.SetEnabled(newValue);
            m_customToggle.SetValueWithoutNotify(m_customValue);
        }

        public CustomFoldout()
        {
            AddToClassList(ussClassName);


            m_Toggle = new Toggle();
            m_Container = new VisualElement
            {
                name = "unity-content"
            };
            m_Toggle.RegisterValueChangedCallback(delegate (ChangeEvent<bool> evt)
            {
                value = m_Toggle.value;
                evt.StopPropagation();
            });

            m_Toggle.AddToClassList(toggleUssClassName);

            m_customToggle = new Toggle() { name = "customToggle" };
            m_customToggle.style.justifyContent = Justify.FlexEnd;
            m_customToggle.RegisterValueChangedCallback(delegate (ChangeEvent<bool> evt)
            {
                customValue = m_customToggle.value;
                m_Container.SetEnabled(customValue);
                evt.StopPropagation();
            });
            m_Toggle.Add(m_customToggle);

            base.hierarchy.Add(m_Toggle);
            m_Container.AddToClassList(contentUssClassName);
            base.hierarchy.Add(m_Container);
            SetValueWithoutNotify(newValue: true);
        }
    }
}
