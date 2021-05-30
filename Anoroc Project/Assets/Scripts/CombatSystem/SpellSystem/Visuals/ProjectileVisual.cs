using CombatSystem.SpellSystem.Attributes;
using StatSystem.StatModifiers;
using UnityEngine;

namespace CombatSystem.SpellSystem.Visuals
{
    public class ProjectileVisual : SpellVisual
    {
        //[SerializeField] private 

        [SpellVisualInput]
        private ObjectModifier<Sprite> spriteToDisplay;

        [SpellVisualInput]
        private CurveModifier rangeToMana;

        [SpellVisualInput]
        private FloatModifier damageToSize;

        public override void StartCall()
        {
        }
    }

}