using CombatSystem.Stats.Attributes;
using StatSystem;
using StatSystem.StatAttributes;

namespace CombatSystem.SpellSystem
{
    public class SpellStatTraits : StatTraits
    {
        protected override StatType[] BaseTypes { get; } = {
            new StatType("_damage",         typeof(FloatAttribute),     "Damage",           "Determines how much Damage a spell will deal"),
            new StatType("_manaUsage",      typeof(FloatAttribute),     "Mana Usage",       "Determines how much Mana is required to cast spell"),
            new StatType("_availableMana",  typeof(FloatAttribute),     "Available Mana",   "Determines how much Mana the spell currently possesses"),
            new StatType("_minManaUsage",   typeof(FloatAttribute),     "Min Mana Usage",   "Determines the minimum Mana required to cast spell"),
            new StatType("_manaMultiplier", typeof(FloatAttribute),     "Mana Multiplier",  "Determines the Mana scaling of a spell"),
            new StatType("_level",          typeof(IntAttribute),       "Level",            "Determines the level at which to cast a spell"),
            new StatType("_sourcePos",      typeof(PositionAttribute),  "Caster Position",  "The Casters position"),
            new StatType("_attackDamageType",     typeof(DamageTypeAttribute),"Attack Damage Type", "Determines the attack damage Type"),
            new StatType("_defenceDamageType",    typeof(DamageTypeAttribute),"Defence Damage Type", "Determines the defence damage Type")
        };
    }
}