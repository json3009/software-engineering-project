using System;
using System.Collections.Generic;
using System.Linq;
using StatSystem;
using StatSystem.StatAttributes;
using UnityEngine;

namespace CombatSystem.SpellSystem
{
    public abstract class SpellBehaviour : MonoBehaviour
    {
        [SerializeField] private int _level;
        [SerializeField] private SpellArchetype _mainType;
        [SerializeField] private StatData _combinedData;
        [SerializeField] private StatData _dataInput;

        [SerializeField] private GameObject _prefabVisual;

        private HashSet<SpellVisual> _toAwaitFinish;

        private FloatAttribute _manaUsageAttr;
        private FloatAttribute _minManaUsageAttr;
        private FloatAttribute _availableManaAttr;
        private IStatAttribute _sourcePosAttr;


        private bool _hasBeenCasted = false;
        private bool _hasFinished = false;

        private float _usedMana = 0;

        private float _maxAvailableMana;
        private float _usageMana;
        private float _maxSpellMana;
        private float _minSpellMana;
        private float _normalizedMana;

        public event Action OnHasCasted;
        public event Action OnUpdate;
        public event Action OnImpact;
        public event Action OnFinish;

        public SpellArchetype MainType { get => _mainType; set => _mainType = value; }
        public StatData CombinedData { get => _combinedData; set => _combinedData = value; }
        public int Level { get => _level; set => _level = value; }
        public GameObject PrefabVisual { get => _prefabVisual; set => _prefabVisual = value; }

        public bool HasBeenCasted { get => _hasBeenCasted; protected set => _hasBeenCasted = value; }
        public bool HasFinished { get => _hasFinished; protected set => _hasFinished = value;
        }

        public float MaxAvailableMana { get => _maxAvailableMana; }
        public float UsedMana { get => _usedMana; }
        public float UsageMana { get => _usageMana; }
        public float MaxSpellMana { get => _maxSpellMana; }
        public float MinSpellMana { get => _minSpellMana; }

        public StatData DataInput
        {
            get => _dataInput;
            set => _dataInput = value;
        }

        public float NormalizedMana => _normalizedMana;

        protected abstract void ValidateInputs();
        protected virtual void StartCall() { }
        protected virtual bool UpdateCall() { return true; }
        protected virtual void ImpactCall() { }

        private void Start()
        {
            try
            {
                if (MainType == null)
                    throw new NullReferenceException($"MainType variable is undefined!");

                if (!CombinedData.TryGetAttribute("_manaUsage", out _manaUsageAttr))
                    throw new NullReferenceException($"Mana Usage missing on Archetype");

                if (!CombinedData.TryGetAttribute("_minManaUsage", out _minManaUsageAttr))
                    throw new NullReferenceException($"Min Mana Usage missing on Archetype");
                

                if (!DataInput.TryGetAttribute("_availableMana", out _availableManaAttr))
                    throw new NullReferenceException($"Available Mana missing on Archetype");

                CombinedData.TryGetAttribute("_sourcePos", out _sourcePosAttr);

                ValidateInputs();
            }
            catch (Exception)
            {
                Destroy(gameObject);
                throw;
            }

            if (_availableManaAttr.Value < _minManaUsageAttr.Value)
            {
                Destroy(gameObject);
                return;
            }
                
            
            // calculate mana
            CalculateMana();
            
            StartCall();
            
            if (_sourcePosAttr != null)
                transform.position = _sourcePosAttr.GetValue<Vector3>();
            
            _hasBeenCasted = true;
            OnHasCasted?.Invoke();
        }

        private void Update()
        {
            if (!_hasBeenCasted)
                return;

            if (_hasFinished)
                return;

            if (UsedMana < MaxAvailableMana && !UpdateCall())
                OnUpdate?.Invoke();
            else
                Impact();
        }

        private void Impact()
        {
            _hasFinished = true;
            OnImpact?.Invoke();

            /*availableManaAttr
                .AddModifier(new FloatModifier(
                    -Math.Min(UsedMana, MaxAvailableMana),
                    FloatModifier.FloatModifierType.Flat)
                );*/

            ImpactCall();
            OnFinish?.Invoke();
            RequestDestroy();
        }

        protected void UseMana(float amount)
        {
            float absAmount = Math.Abs(amount);
            _usedMana += absAmount;
            _availableManaAttr.BaseValue -= absAmount;
            _normalizedMana = NormalizeValue(_availableManaAttr.Value, _minSpellMana, _usageMana);
        }


        private void CalculateMana()
        {
            _minSpellMana = _minManaUsageAttr.GetValue<float>();
            _maxSpellMana = _manaUsageAttr.GetValue<float>();

            _usageMana = Math.Max(
                _minSpellMana,
                _maxSpellMana
            );

            _maxAvailableMana = Math.Min(_availableManaAttr.GetValue<float>(), _usageMana);

            _normalizedMana = NormalizeValue(_availableManaAttr.Value, _minSpellMana, _usageMana);
        }

        private void RequestDestroy()
        {
            if (_toAwaitFinish != null && _toAwaitFinish.Where((e) => !e.HasFinished).Any())
                return;

            if (!_hasFinished)
                return;

            Destroy(gameObject);
        }

        public void RequestDelayedDestroy(SpellVisual visual)
        {
            if (_toAwaitFinish == null)
                _toAwaitFinish = new HashSet<SpellVisual>();

            if(_toAwaitFinish.Add(visual))
                visual.OnVisualsFinished += RequestDestroy;
        }

        private void OnDestroy()
        {
            if(_toAwaitFinish != null)
                foreach (var visual in _toAwaitFinish)
                    visual.OnVisualsFinished -= RequestDestroy;
        }

        private float NormalizeValue(float val, float min, float max)
        {
            if (val > max) return 1;
            if (val < min) return 0;
            return (val - min) / (max - min);
        }
    }
}
