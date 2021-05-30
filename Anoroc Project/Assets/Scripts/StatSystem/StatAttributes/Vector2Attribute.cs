using StatSystem.StatModifiers;
using UnityEngine;

namespace StatSystem.StatAttributes
{
    public class Vector2Attribute : StatAttribute<Vector2, Vector2Modifier>
    {
        public Vector2Attribute() : this(Vector2.zero) { }

        public Vector2Attribute(Vector2 baseValue, params Vector2Modifier[] modifiers) : base(baseValue, modifiers) { }


        protected override void CalculateValue()
        {
            Vector2 newValue = BaseValue;

            foreach (var t in _modifiers)
                newValue += t.Value;

            _value = newValue;
        }
    }
}
