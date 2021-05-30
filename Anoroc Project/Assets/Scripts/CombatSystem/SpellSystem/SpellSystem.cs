using System.Collections.Generic;
using Scripts.CombatSystem;
using StatSystem;
using UnityEngine;

/*#if UNITY_EDITOR
using UnityEditor;
#endif*/


namespace CombatSystem.SpellSystem
{
    [CreateAssetMenu(menuName = "Game/Combat/Spell System")]
    public class SpellSystem : ScriptableObject
    {
        [SerializeField] private SpellStatTraits _traits;
        [SerializeField] private DamageDefinition _damageDefinition;

        [SerializeField] private List<SpellArchetype> _registeredArchetypes = new List<SpellArchetype>();

        public SpellStatTraits Traits { get => _traits; set => _traits = value; }
        public DamageDefinition DamageDefinition { get => _damageDefinition; set { _damageDefinition = value; } }
        public List<SpellArchetype> RegisteredArchetypes { get => _registeredArchetypes; }
    }
}
