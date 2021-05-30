using UnityEngine;

namespace AISystem
{
    
    /// <summary>
    /// <para>The AIState class is used in tandem with the <see cref="AIStateController">State Machine</see>.</para>
    /// <para>The AIState dictates the current state of the <see cref="AIStateController">state machine</see>.</para>
    /// </summary>
    [CreateAssetMenu (menuName = "Game/AI/State")]
    public class AIState : ScriptableObject
    {
        [SerializeField] private AIAction[] _actions;
        [SerializeField] private AITransition[] _transitions;

        /// <summary>
        /// The <see cref="AITransition">transitions</see> array for this state.
        /// </summary>
        public AITransition[] Transitions => _transitions;
        
        /// <summary>
        /// The <see cref="AIAction">actions</see> array for this state.
        /// </summary>
        public AIAction[] Actions => _actions;

        /// <summary>
        /// <para>Performs an update cycle to this state.</para>
        /// </summary>
        /// <param name="controller">The state machine</param>
        public void UpdateState(AIStateController controller)
        {
            DoActions (controller);
            CheckTransitions (controller);
        }
        
        /// <summary>
        /// <para>Performs all <see cref="Actions">actions</see> defined within this state.</para>
        /// </summary>
        /// <param name="controller">The state machine</param>
        private void DoActions(AIStateController controller)
        {
            foreach (var action in Actions)
                action.Act (controller);
        }

        /// <summary>
        /// <para>Performs checks on all <see cref="Transitions">transitions</see> defined within this state.</para>
        /// </summary>
        /// <param name="controller">The state machine</param>
        private void CheckTransitions(AIStateController controller)
        {
            foreach (var transition in Transitions)
            {
                bool decisionSucceeded = transition.Decision.Decide (controller);

                if (decisionSucceeded) {
                    controller.TransitionToState(transition.TrueState);
                } else 
                {
                    controller.TransitionToState (transition.FalseState);
                }
            }
        }
        
        /// <summary>
        /// <para>Draws all Debug Gizmos to the Unity Editor</para>
        /// </summary>
        /// <param name="controller">The state machine</param>
        public void DrawGizmos(AIStateController controller)
        {
            foreach (var action in _actions)
                action.DrawDebug(controller);
        }
    }
}
