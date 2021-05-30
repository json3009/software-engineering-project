using UnityEngine;

namespace AISystem.Actions
{    
    /// <summary>
    /// <para>Performs a ranged Attack.</para>
    /// <inheritdoc path="/summary"/>
    /// </summary>
    [CreateAssetMenu (menuName = "Game/AI/Actions/Attack [Ranged]")]
    public class RangedAttack : AIAction
    {
        private const string RANGED_COOLDOWN_KEY = "rangedCooldown";
        
        [SerializeField] private float _attackWarmup = 3;
        [SerializeField] private float _attackCooldown = 3;
        
        /// <inheritdoc/>
        public override void Act(AIStateController controller)
        {
            if(!controller.TargetObject)
                return;

            controller.Rigidbody.velocity = Vector2.zero;
            controller.TargetPosition = controller.TargetObject.transform.position;
            
            if (controller.CheckIfCountDownElapsed(_attackWarmup) && 
                controller.CheckIfCooldownDownElapsed(this, RANGED_COOLDOWN_KEY, _attackCooldown))
            {
                controller.TargetObject.DealDamage(5);
                controller.ResetCooldown(this, RANGED_COOLDOWN_KEY);
            }
        }
    }
}