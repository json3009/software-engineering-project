using System;
using System.Collections.Generic;
using OdinSerializer;
using UnityEngine.UIElements;

namespace StatSystem
{

    [Serializable]
    public abstract class StatModifier<TBaseType> : IStatModifier<TBaseType>, IComparable<StatModifier<TBaseType>>, ICloneable
    {
        [OdinSerialize] protected TBaseType _value;
        [OdinSerialize] protected int _order;
        [OdinSerialize] protected UnityEngine.Object _source;

        public TBaseType Value => _value;


        /// <inheritdoc cref="IStatModifier{TBaseType}.Order" />
        public int Order => _order;
        public UnityEngine.Object Source
        {
            get => _source;
            set => _source = value;
        }

        object IStatModifier.Value => Value;

        protected StatModifier()
        {
            _value = default;
            _source = null;
            _order = 0;
        }

        protected StatModifier(TBaseType value, int order = 0, UnityEngine.Object source = null)
        {
            _value = value;
            _source = source;
            _order = order;
        }

        public int CompareTo(StatModifier<TBaseType> other)
        {
            if (Order < other.Order)
                return -1;
            else if (Order > other.Order)
                return 1;

            return 0;
        }

        public abstract VisualElement CreateGUI(Action onChange);


        public override string ToString()
        {
            return $"StatModifier - [{Value}]";
        }

        public object Clone()
        {
            return SerializationUtility.CreateCopy(this);
            /*var copy = Activator.CreateInstance<StatModifier<TBaseType>>();
            copy._order = _order;
            copy._source = _source;
            copy._value = _value;
            AddToClone(copy);
            return copy;*/
        }

        protected abstract void AddToClone(StatModifier<TBaseType> obj);

        public override bool Equals(object obj)
        {
            return obj is StatModifier<TBaseType> modifier &&
                   EqualityComparer<TBaseType>.Default.Equals(_value, modifier._value) &&
                   _order == modifier._order &&
                   EqualityComparer<object>.Default.Equals(_source, modifier._source);
        }

        public override int GetHashCode()
        {
            int hashCode = 684788728;
            hashCode = hashCode * -1521134295 + EqualityComparer<TBaseType>.Default.GetHashCode(_value);
            hashCode = hashCode * -1521134295 + _order.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<object>.Default.GetHashCode(_source);
            return hashCode;
        }

        public int CompareTo(IStatModifier other)
        {
            throw new NotImplementedException();
        }
    }
}