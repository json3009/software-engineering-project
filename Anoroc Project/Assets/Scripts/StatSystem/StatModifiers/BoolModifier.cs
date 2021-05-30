using System;
using UnityEngine.UIElements;

namespace StatSystem.StatModifiers
{
    public class BoolModifier : StatModifier<bool>
    {
        public BoolModifier()
        {
        }
        

        public override VisualElement CreateGUI(Action onChange)
        {
            Toggle toggle = new Toggle
            {
                value = _value
            };

            toggle.RegisterValueChangedCallback((e) => {
                _value = e.newValue;
                onChange?.Invoke();
            });

            return toggle;
        }

        protected override void AddToClone(StatModifier<bool> obj)
        {
            
        }
    }
}
