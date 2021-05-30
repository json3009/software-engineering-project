using UnityEngine;

namespace AISystem.Actions
{
    /// <summary>
    /// <para>Performs a disable action. Disables the current state machine.</para>
    /// <inheritdoc path="/summary"/>
    /// </summary>
    [CreateAssetMenu (menuName = "Game/AI/Actions/Disable")]
    public class DisableAction : AIAction
    {
        /// <inheritdoc/>
        public override void Act(AIStateController controller)
        {
            controller.gameObject.SetActive(false);
        }
    }
}