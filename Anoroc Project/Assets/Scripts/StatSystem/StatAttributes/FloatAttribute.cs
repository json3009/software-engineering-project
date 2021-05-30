using System;
using StatSystem.StatModifiers;

namespace StatSystem.StatAttributes
{
    [Serializable]
    public class FloatAttribute : StatAttribute<float, FloatModifier>
    {
        public FloatAttribute() : this(0) { }

        public FloatAttribute(float baseValue, params FloatModifier[] modifiers) : base(baseValue, modifiers)
        {
        }

        protected override void CalculateValue()
        {
            float newValue = BaseValue;

            int i = 0;
            FloatModifier modifier;
            for (; i < _modifiers.Count; i++)
            {
                modifier = (FloatModifier) _modifiers[i];
                
                if (modifier.Type == FloatModifier.FloatModifierType.Constant)
                    newValue = modifier.Value;
                else if (modifier.Type == FloatModifier.FloatModifierType.Flat)
                    newValue += modifier.Value;
                else
                    break;
            }

            float percentAdditive = 1;
            for (; i < _modifiers.Count; i++)
            {
                modifier = (FloatModifier) _modifiers[i];

                if (modifier.Type == FloatModifier.FloatModifierType.Percent)
                    percentAdditive += modifier.Value;
                else
                    break;
            }
            newValue *= percentAdditive;

            for (; i < _modifiers.Count; i++)
            {
                modifier = (FloatModifier) _modifiers[i];

                if (modifier.Type == FloatModifier.FloatModifierType.Multiply)
                    newValue *= modifier.Value;
            }

            _value = (float)Math.Round(newValue, 4);
            _hasChanged = false;
        }
    }
}
