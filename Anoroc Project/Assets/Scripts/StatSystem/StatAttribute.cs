using System;
using System.Collections.Generic;
using OdinSerializer;
using UnityEngine;

namespace StatSystem
{
    [Serializable]
    public abstract class StatAttribute<TBaseType, TModifier> : 
        IStatAttribute<TBaseType, TModifier>, 
        ISerializationCallbackReceiver 
            where TModifier : IStatModifier<TBaseType>
    
    {
        #region Fields
        
        [NonSerialized] protected bool _hasChanged = true;
        [NonSerialized] protected TBaseType _value = default;

        [OdinSerialize] protected TBaseType _baseValue = default;
        [OdinSerialize] protected List<TModifier> _modifiers = new List<TModifier>();
        
        #endregion

        #region Properties

        public TModifier[] Modifiers => _modifiers.ToArray();

        public TBaseType Value
        {
            get
            {
                if (_hasChanged) RecalculateValue();
                return _value;
            }
            
            
        }

        public TBaseType BaseValue
        {
            get => _baseValue;
            set { _baseValue = value; _hasChanged = true; }
        }

        public Type ValueType => typeof(TModifier);

        object IStatAttribute.BaseValue { get => BaseValue; set => BaseValue = (TBaseType)value; }

        IStatModifier<TBaseType>[] IStatAttribute<TBaseType>.Modifiers => Modifiers as IStatModifier<TBaseType>[];
        IStatModifier[] IStatAttribute.Modifiers => Modifiers as IStatModifier[];

        object IStatAttribute.Value => Value;


        #endregion

        #region Events

        public event Action OnChange;

        #endregion

        #region Constructors

        protected StatAttribute() : this(default) { }

        protected StatAttribute(TBaseType baseValue, params TModifier[] modifiers)
        {
            _baseValue = baseValue;
            AddModifier(modifiers);
        }


        #endregion

        #region Public Methods

        public void Merge(IStatAttribute<TBaseType, TModifier> attr)
        {
            _modifiers.AddRange(attr.Modifiers);
            _modifiers.Sort();
            _hasChanged = true;
        }

        public void AddModifier(params TModifier[] modifiers)
        {
            foreach (var item in modifiers)
                if (item != null)
                    _modifiers.Add(item);

            _modifiers.Sort();
            _hasChanged = true;
            ValuesHaveChanged();
        }

        public bool RemoveModifier(TModifier modifier)
        {
            if (_modifiers.Remove(modifier))
            {
                _hasChanged = true;
                ValuesHaveChanged();
                return true;
            }


            return false;
        }

        public bool RemoveModifier(int index)
        {
            _modifiers.RemoveAt(index);
            _hasChanged = true;
            ValuesHaveChanged();
            return true;
        }

        public bool RemoveModifiers(UnityEngine.Object source)
        {
            if (source == null) return false;

            var result = _modifiers
                .RemoveAll((e) => e.Source != null && e.Source.Equals(source)) > 0;

            if (!result) return false;
            
            _hasChanged = true;
            ValuesHaveChanged();
            return true;
        }

        public void RequestRecalculation()
        {
            _modifiers.Sort();
            _hasChanged = true;
            ValuesHaveChanged();
        }

        #endregion

        #region Private Methods

        protected void RecalculateValue()
        {
            CalculateValue();

            _hasChanged = false;
        }
        
        protected abstract void CalculateValue();


        public void AddModifier(params IStatModifier[] modifiers)
        {
            foreach (var item in modifiers)
            {
                if (item is TModifier modifier)
                    AddModifier(modifier);
            }
        }

        public void Merge(IStatAttribute attr)
        {
            if (attr is IStatAttribute<TBaseType, TModifier> attribute)
                Merge(attribute);
        }

        public bool RemoveModifier(IStatModifier modifier)
        {
            return modifier is IStatModifier<TBaseType> statModifier && RemoveModifier(statModifier);
        }
        public IStatAttribute<TBaseType, TModifier> Combine(IStatAttribute<TBaseType, TModifier> obj)
        {
            var copy = (IStatAttribute<TBaseType, TModifier>)SerializationUtility.CreateCopy(this);
            copy.Merge(obj);
            return copy;
        }

        public IStatAttribute Combine(IStatAttribute obj)
        {
            return !(obj is IStatAttribute<TBaseType, TModifier> attribute) ? 
                this : 
                Combine(attribute);
        }
        public TBaseType GetValue<TTargetType>()
        {
            if (typeof(TTargetType).IsAssignableFrom(typeof(TBaseType)))
                return Value;
            
            Debug.LogError("Trying to GetValue<" + typeof(TTargetType).FullName + "> for Output Type: " + (typeof(TBaseType)).FullName);
            return default;
        }

        TTargetType IStatAttribute.GetValue<TTargetType>()
        {
            return (TTargetType)(object)GetValue<TTargetType>();
        }

        public bool TryGetValue<TTargetType>(out TTargetType value)
        {
            value = ((IStatAttribute)this).GetValue<TTargetType>();
            return value != null;
        }

        void IStatAttribute<TBaseType>.AddModifier(params IStatModifier<TBaseType>[] modifiers)
        {
            AddModifier(modifiers);
        }

        void IStatAttribute<TBaseType>.Merge(IStatAttribute<TBaseType, IStatModifier<TBaseType>> attr)
        {
            Merge(attr);
        }

        bool IStatAttribute<TBaseType>.RemoveModifier(IStatModifier<TBaseType> modifier)
        {
            return RemoveModifier(modifier);
        }

        IStatAttribute<TBaseType, IStatModifier<TBaseType>> IStatAttribute<TBaseType>.Combine(IStatAttribute<TBaseType, IStatModifier<TBaseType>> obj)
        {
            return (IStatAttribute<TBaseType, IStatModifier<TBaseType>>)Combine(obj);
        }

        private void ValuesHaveChanged()
        {
            OnChange?.Invoke();
        }
        
        public void OnBeforeSerialize()
        {
            _hasChanged = true;
        }

        public void OnAfterDeserialize()
        {
            _hasChanged = true;
        }
        
        #endregion
    }
}
