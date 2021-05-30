using System;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace StatSystem.StatModifiers
{
    [Serializable]
    public class FloatModifier : StatModifier<float>
    {
        /// <summary>
        /// Defines the way how the float modifier will behave.
        /// </summary>
        public enum FloatModifierType
        {
            /// <summary>
            /// Constant Number (overrites previous constant / BaseValue)
            /// </summary>
            Constant = 0,

            /// <summary>
            /// Flat Number (Additive [eg. 3 + 5])
            /// </summary>
            Flat = 100,

            /// <summary>
            /// Additive Percentage [3 * (0.5 + 0.2 + 1)]
            /// </summary>
            Percent = 200,

            /// <summary>
            /// Multiplicative Percentage [3 * (1.5 * 1.2)]
            /// </summary>
            Multiply = 300
        }


        [SerializeField] private FloatModifierType _type;

        public FloatModifierType Type => _type;

        public FloatModifier() : this(1, FloatModifierType.Flat) { }

        public FloatModifier(float value, FloatModifierType type, int order, UnityEngine.Object source) : base(value, order, source)
        {
            _type = type;
        }

        public FloatModifier(float value, FloatModifierType type) : this(value, type, (int)type, null) { }
        public FloatModifier(float value, FloatModifierType type, int order) : this(value, type, order, null) { }
        public FloatModifier(float value, FloatModifierType type, UnityEngine.Object source) : this(value, type, (int)type, source) { }

        protected override void AddToClone(StatModifier<float> obj)
        {
            if(!(obj is FloatModifier copy)) return;
            
            copy._type = _type;
        }

        public override bool Equals(object obj)
        {
            return obj is FloatModifier modifier &&
                   base.Equals(obj) &&
                   Value.Equals(modifier.Value) &&
                   Order == modifier.Order &&
                   EqualityComparer<object>.Default.Equals(Source, modifier.Source) &&
                   _type == modifier._type;
        }

        public override int GetHashCode()
        {
            int hashCode = -831987731;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + Value.GetHashCode();
            hashCode = hashCode * -1521134295 + Order.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<object>.Default.GetHashCode(Source);
            hashCode = hashCode * -1521134295 + _type.GetHashCode();
            return hashCode;
        }

        public override VisualElement CreateGUI(Action onChange)
        {
            VisualElement root = new VisualElement();
            #if UNITY_EDITOR
            root.style.flexDirection = FlexDirection.Row;

            var typeField = new EnumField(FloatModifierType.Flat) { value = _type };
            typeField.RegisterValueChangedCallback((e) => { _type = (FloatModifierType)e.newValue; _order = (int)_type; onChange?.Invoke(); });
            typeField.style.width = 100;

            var floatField = new FloatField() { value = _value };
            floatField.RegisterValueChangedCallback((e) => { _value = e.newValue; onChange?.Invoke(); });
            floatField.style.flexGrow = 1;

            root.Add(typeField);
            root.Add(floatField);

            return root;
            #else
            return new VisualElement();
            #endif
        }

        public override string ToString()
        {
            return Type switch
            {
                FloatModifierType.Constant => $"CONST {Value:+#;-#;+0;F2}",
                FloatModifierType.Flat => $"{Value:+#;-#;+0;F2}",
                FloatModifierType.Percent => $"{(Value*100):+#;-#;+0;F2} %",
                FloatModifierType.Multiply => $"* {Value:+#;-#;+0;F2}",
                _ => throw new NotImplementedException(),
            };
        }
    }
}
