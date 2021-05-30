using System;
using StatSystem;
using UnityEngine;

namespace CombatSystem.SpellSystem
{
    public abstract class SpellVisual : MonoBehaviour
    {
        public event Action OnVisualsFinished;

        [SerializeField] private SpellBehaviour _behaviour;

        private StatData data;
        private bool _hasFinished = false;


        public SpellBehaviour Behaviour { get => _behaviour; set => _behaviour = value; }
        public StatData Data { get => data; set => data = value; }
        public bool HasFinished { get => _hasFinished; set => _hasFinished = value; }

        public abstract void StartCall();
        public virtual void UpdateCall() { }

        public void Start()
        {
            StartCall();
        }

        private void LateUpdate()
        {
            UpdateCall();
        }

        protected void VisualsHaveFinished()
        {
            _hasFinished = true;
            OnVisualsFinished?.Invoke();
        }
    }
}
