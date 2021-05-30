using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace StatSystem.StatModifiers
{
    public class IntModifier : StatModifier<int>
    {


        public IntModifier(int value, int order = 0, UnityEngine.Object source = null) : base(value, order, source) { }

        public IntModifier() : this(1) { }

        public override VisualElement CreateGUI(Action onChange)
        {
            VisualElement root = new VisualElement();
            #if UNITY_EDITOR
            IntegerField intField = new IntegerField("Value") { value = Value };
            intField.RegisterValueChangedCallback((e) => { _value = e.newValue; onChange?.Invoke(); });

            root.Add(intField);
            #endif
            return root;
        }

        protected override void AddToClone(StatModifier<int> obj)
        {
            
        }
    }
}
