using UnityEngine;

namespace AISystem
{
    /// <summary>
    /// <para>The AIAction class is used in tandem with the <see cref="AIStateController">State Machine</see>.</para>
    /// <para>When a <see cref="AIState">state</see> instance is dispatched, the <see cref="AIStateController">state machine</see> responds by performing AIActions.</para>
    /// </summary>
    public abstract class AIAction : ScriptableObject
    {
        /// <summary>
        /// Performs an Action, such as changing a variable, performing I/O, invoking a function, ...
        /// </summary>
        /// <param name="controller">The state machine.</param>
        public abstract void Act (AIStateController controller);

        /// <summary>
        /// Only compiles in the UNITY EDITOR!
        /// Draws debug information in the Editor.
        /// </summary>
        /// <param name="controller">The state machine.</param>
        public virtual void DrawDebug(AIStateController controller)
        {
        }
    }
}
