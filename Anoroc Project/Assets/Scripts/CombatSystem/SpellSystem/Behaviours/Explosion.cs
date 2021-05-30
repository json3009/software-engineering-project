using System;
using System.Linq;
using CharacterSystem;
using CombatSystem.SpellSystem.Attributes;
using CombatSystem.Stats.Modifiers;
using Scripts.CombatSystem;
using StatSystem;
using StatSystem.StatAttributes;
using UnityEngine;
using Utilities;

namespace CombatSystem.SpellSystem.Behaviours
{
    public class Explosion : SpellBehaviour
    {
        [SpellBehaviourStat]
        public static readonly StatType STAT_RANGE = new StatType("_radius", typeof(FloatAttribute), "Radius", "Determines the radius of an Explosion");

        [SpellBehaviourStat]
        public static readonly StatType STAT_SPEED = new StatType("_explosionPower", typeof(FloatAttribute), "Explosion Power", "Determines the power of the explosion");


        Rigidbody2D rb;

        private FloatAttribute radiusAttr;
        private FloatAttribute damageAttr;
        private FloatAttribute explosionPowerAttr;
        private IStatAttribute attackDamageAttr;

        protected override void ValidateInputs()
        {
            if (!CombinedData.TryGetAttribute("_radius", out radiusAttr))
                throw new NullReferenceException($"Radius missing on explosion");

            if (!CombinedData.TryGetAttribute("_damage", out damageAttr))
                throw new NullReferenceException($"Damage missing on explosion");
            
            if (!CombinedData.TryGetAttribute("_attackDamageType", out attackDamageAttr))
                throw new NullReferenceException($"Attack Damage missing on explosion");

            if (!CombinedData.TryGetAttribute("_explosionPower", out explosionPowerAttr))
                throw new NullReferenceException($"Explosion Power missing on explosion");
        }

        protected override void StartCall()
        {
            Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
        }

        protected override void ImpactCall()
        {
            Vector3 explosionPos = transform.position;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(explosionPos, radiusAttr.GetValue<float>());

            var power = explosionPowerAttr.Value * NormalizedMana;
            var damage = damageAttr.Value * NormalizedMana;
            var radius = radiusAttr.Value * NormalizedMana;
            
            foreach (Collider2D hit in colliders)
            {
                var rbHit = hit.GetComponent<Rigidbody2D>();
                var character = hit.GetComponentInChildren<Character>(false);

                if (rbHit != null)
                    rbHit.AddExplosionForce(power, explosionPos, radius);
                
                if (character != null)
                    character.DealDamage(damage, (DamageTypeModifier)attackDamageAttr.Modifiers.LastOrDefault());
            }
        }
    }
}
