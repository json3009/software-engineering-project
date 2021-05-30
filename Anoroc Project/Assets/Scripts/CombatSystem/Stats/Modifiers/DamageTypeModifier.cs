using System;
using System.Linq;
using Scripts.CombatSystem;
using StatSystem;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Utilities;

namespace CombatSystem.Stats.Modifiers
{
    [Serializable]
    public class DamageTypeModifier : StatModifier<SerializableGUID?>
    {
        [SerializeField] private DamageDefinition def;

        [NonSerialized] VisualElement valueField;

        public DamageTypeModifier()
        {
        }

        public DamageTypeModifier(DamageType value, DamageDefinition def, int order = 0, UnityEngine.Object source = null) : this(value.ID, def, order, source)
        {
        }

        public DamageTypeModifier(SerializableGUID value, DamageDefinition def, int order = 0, UnityEngine.Object source = null) : base(value, order, source)
        {
            if (!def.TryGetType(value, out _))
                throw new ArgumentException($"ID [{value}] is not part of the given DamageDefinition Asset [{def}]!");

            this.def = def;
        }

        public DamageDefinition Definition => def;

        public override VisualElement CreateGUI(Action onChange)
        {
            #if UNITY_EDITOR
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            
            ObjectField objectField = new ObjectField() { objectType = typeof(DamageDefinition), value = def };
            objectField.RegisterValueChangedCallback((e) => { def = (DamageDefinition)e.newValue; UpdateDamageTypePopup(onChange); onChange?.Invoke(); });
            objectField.style.width = 100;

            valueField = new VisualElement();
            valueField.style.flexGrow = 1;
            UpdateDamageTypePopup(onChange);

            row.Add(objectField);
            row.Add(valueField);
            
            return row;
            #else
            return new VisualElement();
            #endif
        }

        protected override void AddToClone(StatModifier<SerializableGUID?> obj)
        {
            if(!(obj is DamageTypeModifier copy)) return;

            copy.def = def;
            copy.valueField = valueField;
        }

        #if UNITY_EDITOR
        private void UpdateDamageTypePopup(Action onChange)
        {
            valueField.Clear();
            PopupField<DamageType> typeField;
            if (def == null)
            {
                typeField = new PopupField<DamageType>();
                typeField.SetEnabled(false);
            }
            else
            {
                if(!_value.HasValue)
                    _value = def.Types.FirstOrDefault()?.ID;

                typeField = new PopupField<DamageType>(def.Types, 0, (e) => e.Name, (e) => e.Name);
                typeField.RegisterValueChangedCallback((e) => { _value = e.newValue.ID; onChange?.Invoke(); });

                if (_value.HasValue && def.TryGetType(_value.Value, out DamageType damageType))
                {
                    typeField.value = damageType;
                }
            }
            typeField.style.flexGrow = 1;
            valueField.Add(typeField);
        }
        #endif

        public override string ToString()
        {
            return def.TryGetType(_value.Value, out DamageType type) ? type.Name : "UNKNOWN TYPE";
        }
    }
}
