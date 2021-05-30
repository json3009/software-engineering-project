using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OdinSerializer;
using StatSystem.StatAttributes;
using UnityEngine;
using Utilities.Collections;

namespace StatSystem
{
    public abstract class StatTraits : SerializedScriptableObject
    {

        #region Fields

        [OdinSerialize]  private List<StatType> _statTypes;
        [SerializeField] private SerializableDictionary<string, int> _statTypeReferences = new SerializableDictionary<string, int>();

        private ReadOnlyCollection<StatType> _roStatTypes;

        #endregion

        #region Properties

        protected abstract StatType[] BaseTypes { get; }
        
        public ReadOnlyCollection<StatType> StatTypes { get => _roStatTypes ??= StatTypesInternal.AsReadOnly(); }

        private protected List<StatType> StatTypesInternal
        {
            get => _statTypes ??= new List<StatType>(BaseTypes);
            set => _statTypes = value;
        }

        #endregion

        #region Methods

        public StatType GetStatTypeByID(string id)
        {
            return StatTypesInternal
                .Where((e) => e.ID.Equals(id))
                .FirstOrDefault();
        }


        private void RegisterTrait(params StatType[] traits)
        {
            foreach (var trait in traits)
            {
                if (BaseTypes.Contains(trait))
                    continue;

                if (!_statTypeReferences.ContainsKey(trait.ID))
                {
                    _statTypeReferences.Add(trait.ID, 1);
                    StatTypesInternal.Add(trait);
                }
                else
                {
                    _statTypeReferences[trait.ID] += 1;
                }
            }
        }

        private void UnregisterTrait(params StatType[] traits)
        {
            foreach (var trait in traits)
            {
                if (BaseTypes.Contains(trait))
                    continue;

                if (!_statTypeReferences.ContainsKey(trait.ID))
                    continue;

                if (_statTypeReferences[trait.ID] < 2)
                {
                    _statTypeReferences.Remove(trait.ID);
                    StatTypesInternal.Remove(trait);
                }

                else
                {
                    _statTypeReferences[trait.ID] -= 1;
                }
            }
        }

        public void UpdateTraits(StatType[] traits)
        {
            List<StatType> current_types = new List<StatType>(BaseTypes.Concat(traits).Distinct());

            for (int i = 0; i < current_types.Count; i++)
            {
                int indexIfFound;
                if ((indexIfFound = StatTypesInternal.IndexOf(current_types[i])) > 0)
                    current_types[i] = StatTypesInternal[indexIfFound];
            }

            StatTypesInternal.Clear();
            StatTypesInternal.AddRange(current_types);
        }

        #endregion
    }
}
