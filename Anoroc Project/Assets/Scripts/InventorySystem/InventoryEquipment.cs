using System.Linq;
using CharacterSystem;
using CombatSystem.SpellSystem;
using EquipmentSystem;
using OdinSerializer;
using StatSystem;
using UnityEngine;

namespace InventorySystem
{
    /// <summary>
    /// A piece of equipment.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Inventory/New Equipment")]
    public class InventoryEquipment : InventoryObject
    {
        [SerializeField] private bool _isDroppable = true;
        [SerializeField] private bool _isStackable = false;
        [SerializeField] private int _maxStack = 1;
        
        [SerializeField] private EquipmentSet _item;
        [SerializeField] private EquipmentSlot _slot;

        [SerializeField] private SpellStatTraits _spellTraits;
        [OdinSerialize] private StatData _spellData;

        [SerializeField] private CharacterStats _characterTraits;
        [OdinSerialize] private StatData _characterData;

        public override bool IsDroppable => _isDroppable;
        public override bool IsStackable => _isStackable;
        public override int MaxStack => _maxStack;

        public override string Name => _item.Name;
        public override string Description => "";

        /// <summary>
        /// The Icon of the equipment.
        /// </summary>
        public override Sprite Icon => _item != null ? _item.Icon : null;

        /// <summary>
        /// The Equipment Set to equip / unEquip.
        /// </summary>
        public EquipmentSet Item => _item;

        /// <summary>
        /// The slot to equip to.
        /// </summary>
        public EquipmentSlot Slot => _slot;
        

        /// <summary>
        /// The spell stats data to apply upon equip.
        /// </summary>
        public StatData SpellData
        {
            get => _spellTraits != null ?  _spellData : null;
            set => _spellData = value;
        }

        /// <summary>
        /// The character stats data to apply upon equip.
        /// </summary>
        public StatData CharacterData
        {
            get => _characterTraits != null ?  _characterData : null;
            set => _characterData = value;
        }

        /// <summary>
        /// The possible applicable Spell stat traits.
        /// </summary>
        public SpellStatTraits SpellTraits
        {
            get => _spellTraits;
            set => _spellTraits = value;
        }

        /// <summary>
        /// The possible applicable character stat traits.
        /// </summary>
        public CharacterStats CharacterTraits
        {
            get => _characterTraits;
            set => _characterTraits = value;
        }

        /// <inheritdoc />
        public override void Use(InventoryManager manager, Character chr)
        {
            if (manager.Equipment.Contains(this))
            {
                // unEquip item when item was already equipped 
                UnEquip(manager, chr);
                return;
            } 
            
            var equipmentUsedBySlot = manager.Equipment
                .Where((e) => e._slot == _slot)
                .ToList();
            
            if (equipmentUsedBySlot.Any())
            {
                // unEquip existing equipped item at slot, then equip new item
                foreach (var variableEquipment in equipmentUsedBySlot)
                    variableEquipment.UnEquip(manager, chr);
            }

            Equip(manager, chr);
        }

        /// <summary>
        /// Equip Item.
        /// </summary>
        /// <param name="manager">The Inventory Manager.</param>
        /// <param name="character">The Character to equip to.</param>
        /// <returns></returns>
        private bool Equip(InventoryManager manager, Character character)
        {
            var equip = character.Equipment.Equip(_item);
            if (equip)
            {
                manager.Equipment.Add(this);
                ApplyBuffs(manager, character);
            }

            return equip;
        }

        /// <summary>
        /// UnEquip Item.
        /// </summary>
        /// <param name="manager">The Inventory Manager.</param>
        /// <param name="character">The Character to unEquip from.</param>
        /// <returns></returns>
        private bool UnEquip(InventoryManager manager, Character character)
        {
            var unEquip = character.Equipment.UnEquip(_item);
            manager.Equipment.Remove(this);
            
            character.CharacterData.RemoveDataWithSource(this);
            character.SpellData.RemoveDataWithSource(this);
            
            return unEquip;
        }

        /// <summary>
        /// Apply Stats to Character.
        /// </summary>
        /// <param name="manager">The Inventory Manager.</param>
        /// <param name="chr">The character to apply the stats to.</param>
        public void ApplyBuffs(InventoryManager manager, Character chr)
        {
            if(_characterData != null)
                chr.CharacterData.AddDataFromSource(this, _characterData);
            
            if(_spellData != null)
                chr.SpellData.AddDataFromSource(this, _spellData);
        }
    }
}