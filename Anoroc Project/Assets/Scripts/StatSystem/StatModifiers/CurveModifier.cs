using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace StatSystem.StatModifiers
{
    public class CurveModifier : StatModifier<AnimationCurve>
    {
        public CurveModifier()
        {
        }

        public CurveModifier(AnimationCurve value, int order = 0, UnityEngine.Object source = null) : base(value, order, source)
        {
        }

        public override VisualElement CreateGUI(Action onChange)
        {
            #if UNITY_EDITOR
            CurveField field = new CurveField() { value = _value };
            field.RegisterValueChangedCallback((e) =>
            {
                _value = e.newValue;
                onChange?.Invoke();
            });

            return field;
            #else
            return new VisualElement();
            #endif
        }

        protected override void AddToClone(StatModifier<AnimationCurve> obj)
        {
            
        }
    }
}
