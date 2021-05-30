using PathfinderSystem;
using UnityEngine;

namespace AISystem.Actions
{
    /// <summary>
    /// <para>Performs a return to base action. Returns the <see cref="AIStateController">state machine</see> object to its original location in world space.</para>
    /// <inheritdoc path="/summary"/>
    /// </summary>
    [CreateAssetMenu (menuName = "Game/AI/Actions/Return to Base")]
    public class ReturnToBase : AIAction
    {
        /// <inheritdoc/>
        public override void Act(AIStateController controller)
        {
            controller.TargetPosition = controller.OriginalPosition;
        }
    }
}