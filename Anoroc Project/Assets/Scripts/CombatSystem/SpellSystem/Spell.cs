using System.Linq;
using CharacterSystem;
using StatSystem;
using StatSystem.StatModifiers;
using UnityEngine;

namespace CombatSystem.SpellSystem
{
    [CreateAssetMenu(menuName = "Game/Combat/Spell")]
    public class Spell : ScriptableObject
    {
        [SerializeField] private string _description;
        [SerializeField] private SpellSystem _system;
        [SerializeField] private SpellArchetype[] _spells;

        public SpellArchetype[] Spells => _spells;
        public string Description => _description;

        public SpellSystem System => _system;
        public void Cast(Character character, StatData data, Vector3? sourcePos = null)
        {
            //if(data.TryGetAttribute(c, out IStatAttribute sourceAttr))
                //data.Modifiers.AddNewAttribute(system.Traits.GetStatTypeByID("_sourcePos"), out IStatAttribute sourceAttr);
                
            IStatAttribute sourceAttr = data.GetOrAddAttribute(System.Traits.GetStatTypeByID("_sourcePos"));
            data.AddDataFromSource(character, character.SpellData);

            sourceAttr.AddModifier(new PositionModifier(sourcePos ?? character.transform.position));
            Cast(character, 0, data);
        }

        private void Cast(Character character, int index, StatData data)
        {
            if(index < 0 || index >= _spells.Length)
                return;
            
            SpellBehaviour behaviour = _spells[index].Cast(data, character.GetLevelForSpellArchetype(_spells[index]));
            behaviour.OnHasCasted += () => character.UsedSpellArchetype(_spells[index]);
            behaviour.OnImpact += () =>
            {
                data.GetAttribute("_sourcePos")
                    .AddModifier(new PositionModifier(behaviour.transform.position));

                Cast(character, index + 1, data);
            };
        }

        public float GetMaxManaCost(Character character, StatData data)
        {
            // todo: get level from character
            return _spells.Sum(spellArchetype => spellArchetype.GetMaxManaCost(2, data));
        }
    }
}
