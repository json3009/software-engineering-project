using System;
using CombatSystem.SpellSystem.Attributes;
using CombatSystem.Stats.Attributes;
using StatSystem;
using StatSystem.StatAttributes;
using StatSystem.StatModifiers;
using UnityEngine;

namespace CombatSystem.SpellSystem.Behaviours
{
    public class Projectile : SpellBehaviour
    {
        private static readonly float DISTANCE_MARGIN = 0.5f;

        [SpellBehaviourStat]
        public static readonly StatType STAT_RANGE =       new StatType("_range", typeof(FloatAttribute), "Range", "Determines the range of the projectile");

        [SpellBehaviourStat]
        public static readonly StatType STAT_SPEED =       new StatType("_speed", typeof(FloatAttribute), "Speed", "Determines the speed of the projectile");

        [SpellBehaviourStat]
        public static readonly StatType STAT_TARGET_POS =  new StatType("_targetPos", typeof(PositionAttribute), "Target Position", "The Targets position");

        #region Spell Inputs

        [SpellBehaviourInput]
        private BoolModifier autoFollow;

        #endregion

        #region Fields

        private Rigidbody2D rigidBody;

        private IStatAttribute sourcePosAttr;
        private IStatAttribute targetPosAttr;
        private IStatAttribute rangeAttr;
        private IStatAttribute speedAttr;

        private float manaUsagePerUnitOfRange;

        private Vector3? previousPosition;

        private Vector3 targetPosition;

        #endregion

        #region Properties

        public Rigidbody2D RigidBody { get => rigidBody; }

        #endregion

        protected override void ValidateInputs()
        {
            if (!CombinedData.TryGetAttribute("_targetPos", out targetPosAttr))
                throw new NullReferenceException($"Target Position missing on Projectile");

            if (!CombinedData.TryGetAttribute("_range", out rangeAttr))
                throw new NullReferenceException($"Range missing on Projectile");

            if (!CombinedData.TryGetAttribute("_speed", out speedAttr))
                throw new NullReferenceException($"Speed missing on Projectile");
        }

        protected override void StartCall()
        {
            rigidBody = gameObject.AddComponent<Rigidbody2D>();
            rigidBody.gravityScale = 0;

            manaUsagePerUnitOfRange = UsageMana / rangeAttr.GetValue<float>();

            targetPosition = targetPosAttr.GetValue<Vector3>();
        }

        protected override bool UpdateCall()
        {
            if (autoFollow.Value)
                targetPosition = this.targetPosAttr.GetValue<Vector3>();

            float distance = Vector3.Distance(targetPosition, transform.position);
            if (distance <= DISTANCE_MARGIN)    
                return true;
            
            // set velocity of spell
            Vector3 directionVector = (targetPosition - transform.position).normalized;
            RigidBody.velocity = directionVector * speedAttr.GetValue<float>();

            // reduce Available Mana
            if (previousPosition.HasValue)
            {
                float distanceTravelled = Vector3.Distance(previousPosition.Value, transform.position);
                UseMana(distanceTravelled * manaUsagePerUnitOfRange);
            }

            previousPosition = transform.position;
            return false;
        }
    }
}
