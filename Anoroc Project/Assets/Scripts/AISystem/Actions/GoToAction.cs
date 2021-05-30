using System.Linq;
using PathfinderSystem;
using UnityEngine;

namespace AISystem.Actions
{
    /// <summary>
    /// <para>Performs a Go To Action. Moves state machine object to a given object using the <see cref="AIStateController.PathFindSystem">pathfinding system</see>.</para>
    /// <inheritdoc path="/summary"/>
    /// </summary>
    /// <remarks>Uses <see cref="PATH_ERROR">PATH_ERROR</see> margin to determine stopping distance.</remarks>
    [CreateAssetMenu (menuName = "Game/AI/Actions/Move to")]
    public class GoToAction : AIAction
    {
        /// <summary>
        /// The margin at which to stop moving.
        /// </summary>
        private const float PATH_ERROR = 0.5f;
        
        [SerializeField] private float _movementSpeed = 2;

        /// <summary>
        /// The movement speed.
        /// </summary>
        public float MovementSpeed => _movementSpeed;

        /// <inheritdoc/>
        public override void Act(AIStateController controller)
        {
            if (!controller.TargetPosition.HasValue)
            {
                controller.Rigidbody.velocity= Vector2.zero;
                return;
            }

            if (!controller.PreviousTargetPosition.HasValue ||
                controller.TargetPosition.Value != controller.PreviousTargetPosition.Value ||
                controller.Path == null)
            {
                
                controller.Path = 
                    controller.PathFindSystem.FindPath(controller.transform.position, controller.TargetPosition.Value);
                
                controller.PreviousTargetPosition = controller.TargetPosition;

                if(controller.Path.Count > 0)
                    controller.Path?.Pop();
            }

            if (controller.Path == null || controller.Path.Count <= 0)
            {
                controller.TargetPosition = null;
                controller.PreviousTargetPosition = null;
                return;
            }

            if (!controller.NextNode.HasValue ||  
                Vector2.Distance(controller.NextNode.Value, controller.transform.position) < PATH_ERROR)
            {
                controller.NextNode = controller.Path.Pop();
            }
            
            var newDirection = (controller.NextNode.Value - (Vector2) controller.transform.position).normalized;
            controller.Rigidbody.velocity = newDirection * MovementSpeed;
            
        }
        
        /// <inheritdoc/>
        /// <summary>
        /// <para>Draws the calculated path from the start node to the end Node using the <see cref="AIStateController.Path">Path</see> array.</para>
        /// </summary>
        public override void DrawDebug(AIStateController controller)
        {
            if(controller.Path == null || controller.Path.Count < 2) return;
            
            var path = controller.Path.ToArray();

            Debug.DrawLine(controller.transform.position, path[0], Color.yellow);
            
            for (int i = 0; i < path.Length-1; i++)
            {
                Debug.DrawLine(path[i], path[i+1], Color.yellow);   
            }
        }
    }
}