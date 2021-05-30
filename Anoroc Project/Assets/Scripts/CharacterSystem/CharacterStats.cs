using StatSystem;
using StatSystem.StatAttributes;
using UnityEngine;

namespace CharacterSystem
{
    /// <summary>
    /// CharacterStats class determines all the available Character stats. 
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Character/New Stats Asset")]
    public class CharacterStats : StatTraits
    {
        protected override StatType[] BaseTypes { get; } =
        {
            new StatType("_maxHealth", typeof(FloatAttribute), "Health", "Determines the Max health of a character"),
            new StatType("_maxMana",   typeof(FloatAttribute), "Mana",   "Determines the Max mana of a character"),
        };
    }
}