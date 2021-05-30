using System.Linq;
using CombatSystem.Stats.Modifiers;
using StatSystem;
using Utilities;

namespace CombatSystem.Stats.Attributes
{
    public class DamageTypeAttribute : StatAttribute<SerializableGUID?, DamageTypeModifier>
    {
        public DamageTypeAttribute()
        {
        }

        public DamageTypeAttribute(params DamageTypeModifier[] modifiers) : base(null, modifiers)
        {
        }

        protected override void CalculateValue()
        {
            /*var distinctModifiers = _modifiers
                .Where((e) => e != null)
                .Distinct();

            _modifiers.Clear();
            _modifiers.AddRange(distinctModifiers);*/

            _value = _modifiers.LastOrDefault()?.Value;
        }
    }

}