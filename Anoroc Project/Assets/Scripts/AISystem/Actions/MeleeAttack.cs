using UnityEngine;

namespace AISystem.Actions
{
    /// <summary>
    /// <para>Performs a Melee Attack Action.</para>
    /// <inheritdoc path="/summary"/>
    /// </summary>
    [CreateAssetMenu (menuName = "Game/AI/Actions/Attack [Melee]")]
    public class MeleeAttack : AIAction
    {
        private const string MELEE_COOLDOWN_KEY = "meleeCooldown";
        
        [SerializeField] private float _attackWarmup = 3;
        [SerializeField] private float _attackCooldown = 3;
        
        /// <inheritdoc/>
        public override void Act(AIStateController controller)
        {
            if(!controller.TargetObject)
                return;

            controller.TargetPosition = controller.TargetObject.transform.position;
            
            if (controller.CheckIfCountDownElapsed(_attackWarmup) && 
                controller.CheckIfCooldownDownElapsed(this, MELEE_COOLDOWN_KEY, _attackCooldown))
            {
                controller.TargetObject.DealDamage(5);
                controller.ResetCooldown(this, MELEE_COOLDOWN_KEY);
            }
        }
    }
}