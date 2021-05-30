using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;
using Utilities.Collections;

namespace Scripts.CombatSystem
{
    [CreateAssetMenu(menuName = "Game/Combat/Damage Definition")]
    public partial class DamageDefinition : ScriptableObject, ISerializationCallbackReceiver
    {
        private static readonly float DEFAULT_MODIFIER = 1; 


        [SerializeField] private List<DamageType> _types = new List<DamageType>();
        [SerializeField] private SerializableDictionary<InternalDamageMatrixKey, float> _damageMatrix = new SerializableDictionary<InternalDamageMatrixKey, float>();

        public List<DamageType> Types { get => _types; set => _types = value; }

        public float GetDamageTypeValue(SerializableGUID attacker, SerializableGUID defender)
        {
            return (_damageMatrix.TryGetValue((attacker, defender), out float t)) ? t : DEFAULT_MODIFIER;
        }

        public DamageType GetType(SerializableGUID id)
        {
            return _types.Where((e) => e.ID.Equals(id)).FirstOrDefault();
        }

        public bool TryGetType(SerializableGUID id, out DamageType damageType)
        {
            damageType = _types.Where((e) => e.ID.Equals(id)).FirstOrDefault();
            return damageType != null;
        }

        public void SetDamageTypeValue(SerializableGUID attacker, SerializableGUID defender, float value)
        {
            if (attacker.Value.IsEmpty() || defender.Value.IsEmpty())
                return;

            if (_damageMatrix.ContainsKey((attacker, defender)))
                _damageMatrix[(attacker, defender)] = value;
            else
                _damageMatrix.Add((attacker, defender), value);
        }


        #region Trash Handlers

        public void OnAfterDeserialize() { }

        public void OnBeforeSerialize()
        {
            List<InternalDamageMatrixKey> toDelete = new List<InternalDamageMatrixKey>(); 
            foreach (var item in _damageMatrix)
            {
                if (!TryGetType(item.Key.Attacker, out _) || 
                    !TryGetType(item.Key.Defender, out _) || 
                    item.Value == DEFAULT_MODIFIER)

                    toDelete.Add(item.Key);
            }

            foreach (var item in toDelete)
            {
                _damageMatrix.Remove(item);
            }
        }

        #endregion

    }
}
