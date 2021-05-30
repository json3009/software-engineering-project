using System.Collections.Generic;
using PathfinderSystem;
using UnityEngine;

namespace AISystem.Actions
{
    /// <summary>
    /// <para>Performs a chase action. Chases a given object until a given <see cref="StopDistance">stop distance</see>.</para>
    /// <inheritdoc path="/summary"/>
    /// </summary>
    [CreateAssetMenu (menuName = "Game/AI/Actions/Chase Object")]
    public class ChaseObject : AIAction
    {
        [SerializeField] private float _stopDistance;

        /// <summary>
        /// The stopping distance.
        /// </summary>
        public float StopDistance => _stopDistance;

        /// <inheritdoc/>
        public override void Act(AIStateController controller)
        {
            if (Vector2.Distance(controller.transform.position, controller.TargetObject.transform.position) > StopDistance)
            {
                controller.TargetPosition = controller.TargetObject.transform.position;
            }
            else
            {
                controller.TargetPosition = null;
            }
        }
    }
}