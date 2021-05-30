using UnityEngine;

namespace AISystem
{
    /// <summary>
    /// <para>The AIDecision class is used in tandem with the <see cref="AIStateController">State Machine</see>.</para>
    /// <para>When a <see cref="AIState">state</see> instance is dispatched, the <see cref="AIStateController">state machine</see>
    /// uses <see cref="AITransition">transitions</see> and AIDecisions to decide whether to remain or change <see cref="AIState">state</see>.</para>
    /// </summary>
    public abstract class AIDecision : ScriptableObject
    {
        /// <summary>
        /// Performs a decision.
        /// </summary>
        /// <param name="controller">The state machine.</param>
        public abstract bool Decide (AIStateController controller);
    }
}
