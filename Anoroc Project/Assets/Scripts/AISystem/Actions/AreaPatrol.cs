using UnityEngine;

namespace AISystem.Actions
{
    
    /// <summary>
    /// <para>Performs a Patrol action in a radius.</para>
    /// <inheritdoc path="/summary"/>
    /// </summary>
    [CreateAssetMenu (menuName = "Game/AI/Actions/Patrol Area")]
    public class AreaPatrol : AIAction
    {
        [SerializeField] private float _radius = 5;
        [SerializeField] private float _patrolChance = 0.1f;
        
        /// <inheritdoc/>
        public override void Act(AIStateController controller)
        {
            if(controller.TargetPosition.HasValue)
                return;

            if (_patrolChance - Random.Range(0f, 1f) <= 0) return;
            
            var randomPos = (Vector2) Random.insideUnitCircle * _radius;
            var target = controller.OriginalPosition + randomPos;

            if (controller.PathFindSystem.IsPositionWalkable(target))
                controller.TargetPosition = target;
        }
        
        /// <inheritdoc/>
        /// <summary>
        /// <para>Draws a yellow box on the target position.</para>
        /// </summary>
        /// <param name="controller"></param>
        public override void DrawDebug(AIStateController controller)
        {
            if (!controller.TargetPosition.HasValue) return;

            var color = Gizmos.color;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(controller.OriginalPosition, _radius);
            Gizmos.DrawCube(controller.TargetPosition.Value, new Vector3(1, 1, 0));
            Gizmos.color = color;
        }
    }
}