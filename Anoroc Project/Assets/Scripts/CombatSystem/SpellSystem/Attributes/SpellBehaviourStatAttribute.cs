using System;

namespace CombatSystem.SpellSystem.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class SpellBehaviourStatAttribute : Attribute
    {
        public SpellBehaviourStatAttribute()
        {
        }
    }
}