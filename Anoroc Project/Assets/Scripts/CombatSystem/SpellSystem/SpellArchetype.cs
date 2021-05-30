using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OdinSerializer;
using StatSystem;
using StatSystem.StatAttributes;
using UnityEngine;
using Utilities;

namespace CombatSystem.SpellSystem
{
    [CreateAssetMenu(menuName = "Game/Combat/New Spell Archetype")]
    public class SpellArchetype : SerializedScriptableObject
    {

        #region Fields

        [OdinSerialize] private SpellArchetypeLevels _level = new SpellArchetypeLevels();

        [SerializeField] private string _name;
        [SerializeField] private string _desc;
        [SerializeField] private Type _behaviour;

        [SerializeField] private GameObject _prefabVisual;
        [SerializeField] private SpellSystem _system;

        [OdinSerialize] private Dictionary<string, IStatModifier> _behaviourModifiers;
        [OdinSerialize] private Dictionary<string, IStatModifier> _visualModifiers;

        #endregion

        #region Properties

        public string Name { get => _name; set => _name = value; }
        public string Description { get => _desc; set => _desc = value; }

        public SpellSystem System { get => _system; set => _system = value; }

        public SpellArchetypeLevels Level { get => _level; set => _level = value; }
        public Type Behaviour { get => _behaviour; set => _behaviour = value; }
        public GameObject PrefabVisual { get => _prefabVisual; set => _prefabVisual = value; }
        public Dictionary<string, IStatModifier> BehaviourModifiers => _behaviourModifiers ??= new Dictionary<string, IStatModifier>();
        public Dictionary<string, IStatModifier> VisualModifiers { get => _visualModifiers ??= new Dictionary<string, IStatModifier>(); }

        #endregion

        #region Abstract / Virtual Methods

        /*public List<SpellBehaviour> CastNextSpells(List<SpellArchetype> spells, List<StatData> spellData)
        {
            if (!spells.Any() || !spells.Count().Equals(spellData.Count()))
                return new List<SpellBehaviour>();

            return new List<SpellBehaviour>() { spells[0].Cast(spellData[0]) };
        }*/

        #endregion

        #region Public Methods

        public SpellBehaviour Cast(StatData data, int levelToCast = 0)
        {
            if (Behaviour == null)
                throw new NullReferenceException("Behaviour is not defined");

            SpellBehaviour behaviour = (SpellBehaviour) new GameObject($"Archetype_{Behaviour.Name}")
                .AddComponent(Behaviour);
            
            behaviour.transform.position = Vector3.zero;

            if (data.TryGetAttribute("_level", out IStatAttribute attribute))
                levelToCast = attribute.GetValue<int>();

            if (_behaviourModifiers != null)
            {
                foreach (var field in _behaviourModifiers)
                {
                    FieldInfo fieldInfo;
                    if ((fieldInfo = Behaviour.GetAllField(field.Key)) != null)
                        fieldInfo.SetValue(behaviour, field.Value);
                }
            }

            var levelData = _level.GetLevelData(levelToCast);
            var spellArchetypeData = data.Combine(levelData.Modifiers);

            behaviour.CombinedData = spellArchetypeData;
            behaviour.DataInput = data;
            behaviour.Level = levelData.Level;
            behaviour.MainType = this;

            if (PrefabVisual && PrefabVisual.TryGetComponent<SpellVisual>(out _))
            {
                GameObject instance = Instantiate(PrefabVisual, behaviour.transform);
                if (instance.TryGetComponent<SpellVisual>(out SpellVisual spellVisual))
                {
                    spellVisual.Behaviour = behaviour;
                    spellVisual.Data = spellArchetypeData;
                    if (_visualModifiers != null)
                    {
                        foreach (var field in _visualModifiers)
                        {
                            FieldInfo fieldInfo;
                            if ((fieldInfo = spellVisual.GetType().GetAllField(field.Key)) != null)
                                fieldInfo.SetValue(spellVisual, field.Value);
                        }
                    }
                }
            }

            return behaviour;
        }

        public float GetMaxManaCost(int level, StatData data)
        {
            var levelData = _level.GetLevelData(level);
            var spellArchetypeData = data.Combine(levelData.Modifiers);

            if (spellArchetypeData.TryGetAttribute("_manaUsage", out FloatAttribute attr))
                return attr.Value;
            else
                throw new ArgumentNullException(nameof(spellArchetypeData), "No Mana usage was defined!");
        }

        #endregion

    }
}
