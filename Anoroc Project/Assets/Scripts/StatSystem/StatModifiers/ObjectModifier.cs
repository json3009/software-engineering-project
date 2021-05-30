using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace StatSystem.StatModifiers
{
    public class ObjectModifier<T> : StatModifier<T> where T: Object
    {
        public ObjectModifier()
        {
        }

        public override VisualElement CreateGUI(Action onChange)
        {
            #if UNITY_EDITOR
            ObjectField field = new ObjectField() { objectType = typeof(T) };
            field.RegisterValueChangedCallback((e) => {
                _value = (T)e.newValue;
                onChange?.Invoke();
            });
            field.value = _value;

            return field;
            #else
            return new VisualElement();
            #endif
        }

        protected override void AddToClone(StatModifier<T> obj)
        {
            
        }
    }
}
