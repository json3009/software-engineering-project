using System;
using UnityEngine.UIElements;

namespace StatSystem
{
    public interface IStatModifier<out TBaseType> : IStatModifier
    {
        /// <summary>
        /// The order 
        /// </summary>
        new int Order { get; }
        
        /// <summary>
        /// Source
        /// </summary>
        new UnityEngine.Object Source { get; set; }
        new TBaseType Value { get; }

        new VisualElement CreateGUI(Action onChange);

    }

    public interface IStatModifier : ICloneable
    {
        int Order { get; }
        UnityEngine.Object Source { get; set; }
        object Value { get; }

        VisualElement CreateGUI(Action onChange);
    }
}