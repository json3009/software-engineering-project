using System;

namespace CombatSystem.SpellSystem.Attributes
{

    [AttributeUsage(AttributeTargets.Field)]
    public class SpellBehaviourInputAttribute : Attribute
    {
        public SpellBehaviourInputAttribute(){}
    }
}