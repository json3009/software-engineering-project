using System.Linq;
using StatSystem.StatModifiers;
using UnityEngine;

namespace StatSystem.StatAttributes
{
    public class PositionAttribute : StatAttribute<Vector3, PositionModifier>
    {
        public PositionAttribute() : this(Vector3.zero) { }
        public PositionAttribute(Vector3 baseValue, params PositionModifier[] modifiers) : base(baseValue, modifiers){}

        public new Vector3 Value
        {
            get
            {
                RecalculateValue();
                return _value;
            }
        }

        protected override void CalculateValue()
        {
            _value = BaseValue + _modifiers.LastOrDefault()?.Value ?? Vector3.zero;
        }
    }
}
