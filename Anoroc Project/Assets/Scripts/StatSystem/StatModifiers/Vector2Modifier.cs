using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace StatSystem.StatModifiers
{
    public class Vector2Modifier : StatModifier<Vector2>
    {
        public Vector2Modifier() : this(Vector2.zero) { }

        public Vector2Modifier(Vector2 value, int order = 0, UnityEngine.Object source = null) : base(value, order, source) { }

        public override VisualElement CreateGUI(Action onChange)
        {
            throw new NotImplementedException();
        }

        protected override void AddToClone(StatModifier<Vector2> type)
        {
            
        }
    }
}
