using StatSystem.StatModifiers;

namespace StatSystem.StatAttributes
{
    public class IntAttribute : StatAttribute<int, IntModifier>
    {
        public IntAttribute() : this(0) { }
        public IntAttribute(int baseValue, params IntModifier[] modifiers) : base(baseValue, modifiers) { }

        protected override void CalculateValue()
        {
            int newVal = BaseValue;
            foreach (var number in _modifiers)
            {
                newVal += number.Value;
            }
            _value = newVal;
        }
    }
}
