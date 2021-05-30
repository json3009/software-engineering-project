using System;
using OdinSerializer;
using StatSystem;
using UnityEngine;

namespace CombatSystem.SpellSystem
{
    [Serializable]
    public class SpellArchetypeLevelData : IComparable<SpellArchetypeLevelData>
    {
        [SerializeField] private int _level = 0;
        [OdinSerialize] private StatData _modifiers = new StatData();
        [SerializeField] private string _title = "";
        [SerializeField] private string _description = "";

        public int Level { get => _level; set => _level = value; }
        public StatData Modifiers { get => _modifiers; set => _modifiers = value; }
        public string Title { get => _title; set => _title = value; }
        public string Description { get => _description; set => _description = value; }

        public int CompareTo(SpellArchetypeLevelData other)
        {
            if (this.Level > other.Level)
                return 1;
            else if (this.Level < other.Level)
                return -1;

            return 0;
        }

        public SpellArchetypeLevelData Combine(SpellArchetypeLevelData other)
        {
            if(this.Level > other.Level)
            {
                return Combine(other, this);
            }
            else
            {
                return Combine(this, other);
            }
        }

        private static SpellArchetypeLevelData Combine(SpellArchetypeLevelData lowerLevel, SpellArchetypeLevelData higherLevel)
        {
            SpellArchetypeLevelData newLevelData = new SpellArchetypeLevelData
            {
                _title = higherLevel._title,
                _description = higherLevel._description,
                _level = higherLevel._level
            };

            newLevelData._modifiers = lowerLevel._modifiers.Combine(higherLevel.Modifiers);
            return newLevelData;
        }

        public override string ToString()
        {
            return $"{_level} - {_title}";
        }
    }
}
