using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace StatSystem.StatModifiers
{
    public class TagModifier : StatModifier<string>
    {
        public TagModifier() : this("") { }
        
        public TagModifier(string value, int order = 0, UnityEngine.Object source = null) : base(value, order, source) { }

        public override VisualElement CreateGUI(Action onChange)
        {
            #if UNITY_EDITOR
            TagField tagField = new TagField("Tag") { value = Value };
            tagField.RegisterValueChangedCallback((e) => { _value = e.newValue; onChange?.Invoke(); });

            return tagField;
            #else
            return new VisualElement();
            #endif
        }

        protected override void AddToClone(StatModifier<string> obj)
        {
            
        }
    }
}
