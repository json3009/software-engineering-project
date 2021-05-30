using System.Collections;
using System.Collections.Generic;
using CharacterSystem;
using PathfinderSystem;
using UnityEngine;

namespace AISystem
{
    /// <summary>
    /// The AIStateController is a finite state machine. 
    /// </summary>
    public class AIStateController : MonoBehaviour
    {
        [SerializeField] private bool _aiActive;

        [SerializeField] private AIState _currentState;
        [SerializeField] private Character _targetObject;

        [SerializeField] private PathFinder _pathFindSystem;
        
        private Rigidbody2D _rigidbody;
        private float _stateTimeElapsed;

        private Vector2? _previousTargetPosition;
        private Vector2? _targetPosition;
        private Vector2 _originalPosition;

        private Vector2? _nextNode;

        private Stack<Vector2> _path;

        private Dictionary<string, float> _cooldown = new Dictionary<string, float>();


        /// <summary>
        /// The target object, such as the player character.
        /// </summary>
        public Character TargetObject => _targetObject;

        /// <summary>
        /// The Pathfinding system to use.
        /// </summary>
        public PathFinder PathFindSystem => _pathFindSystem;

        public Rigidbody2D Rigidbody => _rigidbody;

        public Vector2 OriginalPosition => _originalPosition;

        /// <summary>
        /// Used for pathfinding.
        /// Determines the target position.
        /// </summary>
        internal Vector2? TargetPosition
        {
            get => _targetPosition;
            set => _targetPosition = value;
        }
        
        internal Vector2? PreviousTargetPosition
        {
            get => _previousTargetPosition;
            set => _previousTargetPosition = value;
        }

        /// <summary>
        /// Used for pathfinding.
        /// Determines the next node.
        /// </summary>
        internal Vector2? NextNode
        {
            get => _nextNode;
            set => _nextNode = value;
        }

        /// <summary>
        /// Used for pathfinding.
        /// Determines the current path to use.
        /// </summary>
        public Stack<Vector2> Path
        {
            get => _path;
            set => _path = value;
        }

        
        void Awake ()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _originalPosition = transform.position;
        }

        void Update()
        {
            if (!_aiActive)
                return;
            
            _currentState.UpdateState (this);
        }

        /// <summary>
        /// Transition to a state.
        /// </summary>
        /// <param name="nextState">The next state to transition to.</param>
        public void TransitionToState(AIState nextState)
        {
            if (!nextState) return;
            
            _currentState = nextState;
            OnExitState ();
        }

        public bool CheckIfCountDownElapsed(float duration)
        {
            _stateTimeElapsed += Time.deltaTime;
            return (_stateTimeElapsed >= duration);
        }
        
        public bool CheckIfCooldownDownElapsed(object source, string id, float duration)
        {
            var key = $"{source}.{id}";
            if(!_cooldown.ContainsKey(key))
                ResetCooldown(source, id);

            var value = Time.realtimeSinceStartup - _cooldown[key];
            return value >= duration;
        }

        public void ResetCooldown(object source, string id)
        {
            var timeSinceStartup = Time.realtimeSinceStartup;
            var key = $"{source}.{id}";
            if (_cooldown.ContainsKey(key))
                _cooldown[key] = timeSinceStartup;
            else 
                _cooldown.Add(key, timeSinceStartup);
        }

        private void OnExitState()
        {
            _stateTimeElapsed = 0;
            _cooldown.Clear();
        }
        
        private void OnDrawGizmosSelected()
        {
            _currentState.DrawGizmos(this);
        }
        
    }
}
