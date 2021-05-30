using System;
using StatSystem;
using UnityEngine;

namespace CharacterSystem
{
    /// <summary>
    /// The CharacterStat class represents a status for a character. A stat value can be anywhere between the Min and Max.
    /// </summary>
    /// <typeparam name="TBaseType">The Value Type for the Stat.</typeparam>
    [Serializable]
    public class CharacterStat<TBaseType> : ICharacterStat<TBaseType> where TBaseType : IComparable<TBaseType>, IEquatable<TBaseType>
    {

        public event Action OnBelowMinimum;
        public event Action OnAboveMaximum;
        public event Action OnChange;

        // max stat
        [SerializeField] private TBaseType _max;

        // min stat
        [SerializeField] private TBaseType _min;

        // current stat
        [SerializeField] private TBaseType _value;


        private IStatAttribute<TBaseType> _attribute;
        public CharacterStat(TBaseType minStat, TBaseType maxStat, TBaseType value)
        {
            _min = minStat;
            _max = maxStat;

            SetCurrentStat(value);
        }

        /// <inheritdoc cref="ICharacterStat{T}.Min"/>
        public TBaseType Min
        {
            get => _min;
            set { _min = value; SetCurrentStat(_value); }
        }
        
        /// <inheritdoc cref="ICharacterStat{T}.Max"/>
        public TBaseType Max
        {
            get => _max;
            set { _max = value; SetCurrentStat(_value); }
        }

        /// <inheritdoc cref="ICharacterStat{T}.Value"/>
        public TBaseType Value
        {
            get => _value;
            set => SetCurrentStat(value);
        }
        
        object ICharacterStat.Min { get => Min; set => Min = (TBaseType)value; }
        object ICharacterStat.Value { get => Value; set => Value = (TBaseType)value; }
        object ICharacterStat.Max { get => Max; set => Max = (TBaseType)value; }

        /// <inheritdoc />
        public void Clamp()
        {
            SetCurrentStat(_value);
        }

        /// <summary>
        /// Set the value of the stat.
        /// </summary>
        /// <param name="value">The new value.</param>
        public void SetCurrentStat(TBaseType value)
        {
            if (value.CompareTo(_min) <= 0)
            {
                _value = _min;
                OnBelowMinimum?.Invoke();
            }
            else if (value.CompareTo(_max) >= 0)
            {
                _value = _max;
                OnAboveMaximum?.Invoke();
            }
            else
                _value = value;
            
            OnChange?.Invoke();
        }

        /// <summary>
        /// Bind a <see cref="IStatAttribute{TBaseType}">StatAttribute</see> to this stat. Will auto update both stats such that both will always have the same value.
        /// </summary>
        /// <param name="attr">The attribute to bind.</param>
        public void RegisterStatAttribute(IStatAttribute<TBaseType> attr)
        {
            if(attr == null) return;
            
            if(_attribute != null)
                UnregisterAttribute();

            attr.BaseValue = _max;
            Max = attr.GetValue<TBaseType>();
            Value = Max;
            
            attr.OnChange += AttributeOnChange;
            _attribute = attr;
        }

        /// <summary>
        /// Unbind <see cref="IStatAttribute{TBaseType}">StatAttribute</see>.
        /// </summary>
        public void UnregisterAttribute()
        {
            if(_attribute == null) return;
            
            _attribute.OnChange -= AttributeOnChange;
            _attribute = null;
        }

        private void AttributeOnChange()
        {
            Max = _attribute.GetValue<TBaseType>();
        }
    }
}
