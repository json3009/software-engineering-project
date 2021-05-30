using System;

namespace StatSystem
{
    public interface IStatAttribute<TBaseType, TModifier> : IStatAttribute<TBaseType> where TModifier : IStatModifier<TBaseType>
    {
        new TBaseType BaseValue { get; set; }
        new TModifier[] Modifiers { get; }
        new TBaseType Value { get; }
        new Type ValueType { get; }

        void AddModifier(params TModifier[] modifiers);
        void Merge(IStatAttribute<TBaseType, TModifier> attr);
        bool RemoveModifier(TModifier modifier);

        new TBaseType GetValue<TTargetType>();
        IStatAttribute<TBaseType, TModifier> Combine(IStatAttribute<TBaseType, TModifier> obj);
        new void RequestRecalculation();
    }


    public interface IStatAttribute<TBaseType> : IStatAttribute
    {
        new TBaseType BaseValue { get; set; }
        new IStatModifier<TBaseType>[] Modifiers { get; }
        new TBaseType Value { get; }

        new Type ValueType { get; }

        void AddModifier(params IStatModifier<TBaseType>[] modifiers);
        void Merge(IStatAttribute<TBaseType, IStatModifier<TBaseType>> attr);
        bool RemoveModifier(IStatModifier<TBaseType> modifier);

        new TBaseType GetValue<TTargetType>();
        IStatAttribute<TBaseType, IStatModifier<TBaseType>> Combine(IStatAttribute<TBaseType, IStatModifier<TBaseType>> obj);
        new void RequestRecalculation();
    }
    
    
    public interface IStatAttribute
    {
        event Action OnChange;
        
        object BaseValue { get; set; }
        IStatModifier[] Modifiers { get; }
        object Value { get; }
        Type ValueType { get; }

        void AddModifier(params IStatModifier[] modifiers);
        void Merge(IStatAttribute attr);
        bool RemoveModifier(IStatModifier modifier);
        bool RemoveModifier(int index);
        bool RemoveModifiers(UnityEngine.Object source);

        TTargetType GetValue<TTargetType>();
        bool TryGetValue<TTargetType>(out TTargetType value);

        IStatAttribute Combine(IStatAttribute obj);

        void RequestRecalculation();
    }
}