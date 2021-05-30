using UnityEngine;

namespace AISystem.Decisions
{
    
    /// <summary>
    /// <para>Checks whether the <see cref="AIStateController">state machine</see> object is in range of the target position.</para>
    /// <inheritdoc path="/summary"/>
    /// </summary>
    [CreateAssetMenu (menuName = "Game/AI/Decisions/Arrived at target")]
    public class ArrivedAtTarget : AIDecision
    {
        [SerializeField] private float _distance = 1;

        /// <summary>
        /// The stopping distance.
        /// </summary>
        public float Distance => _distance;

        /// <inheritdoc/>
        /// <summary>
        /// Tests if target position is in range. 
        /// </summary>
        /// <remarks>Uses <see cref="Distance"></see> to determine whether the the object has arrived.</remarks>
        /// <returns><b>True</b>, if arrived at target position; <b>False</b> otherwise!</returns>
        public override bool Decide(AIStateController controller)
        {
            if (!controller.TargetPosition.HasValue)
                return true;
                
            if (Vector2.Distance(controller.TargetPosition.Value, controller.transform.position) <= Distance)
                return true;

            return false;
        }
    }
}