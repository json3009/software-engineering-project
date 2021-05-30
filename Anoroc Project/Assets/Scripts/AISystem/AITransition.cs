using System;
using UnityEngine;

namespace AISystem
{
    /// <summary>
    /// <para>The AITransition class is used in tandem with the <see cref="AIStateController">State Machine</see>.</para>
    /// <para>When a <see cref="AIState">state</see> instance is dispatched, the <see cref="AIStateController">state machine</see>
    /// uses AITransition to decide whether to remain or change <see cref="AIState">state</see>.</para>
    /// </summary>
    [Serializable]
    public class AITransition
    {
        [SerializeField] private string _name;
        [SerializeField] private AIDecision _decision;
        [SerializeField] private AIState _trueState;
        [SerializeField] private AIState _falseState;
        
        /// <summary>
        /// The decision to perform.
        /// </summary>
        public AIDecision Decision => _decision;
        
        /// <summary>
        /// The state to transition to if the <see cref="Decision">decision</see> returns <b>TRUE</b>.
        /// </summary>
        /// <remarks>If NULL; Will remain in current State.</remarks>
        public AIState TrueState => _trueState;

        /// <summary>
        /// The state to transition to if the <see cref="Decision">decision</see> returns <b>FALSE</b>.
        /// </summary>
        /// <remarks>If NULL; Will remain in current State.</remarks>
        public AIState FalseState => _falseState;
    }
}
