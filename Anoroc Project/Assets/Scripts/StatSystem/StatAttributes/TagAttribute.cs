using System.Linq;
using StatSystem.StatModifiers;

namespace StatSystem.StatAttributes
{
    public class TagAttribute : StatAttribute<string, TagModifier>
    {
        public TagAttribute() : this("") { }

        public TagAttribute(string baseValue, params TagModifier[] modifiers) : base(baseValue, modifiers){}

        protected override void CalculateValue()
        {
            _value = _modifiers.LastOrDefault()?.Value ?? BaseValue;
        }
    }
}
