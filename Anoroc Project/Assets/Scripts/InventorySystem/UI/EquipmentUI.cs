using System;
using System.Collections.Generic;
using System.Linq;
using CharacterSystem;
using UnityEngine;
using Utilities;

namespace InventorySystem.UI
{
    public class EquipmentUI : MonoBehaviour, IInventoryUI
    {
        [SerializeField] private Character _character;

        [SerializeField] private EquipmentSlotUI _slotHead;
        [SerializeField] private EquipmentSlotUI _slotTorso;
        [SerializeField] private EquipmentSlotUI _slotPelvis;
        
        public InventoryManager Inventory => _character.Inventory;

        public Character Character => _character;

        private void Start()
        {
            Inventory.OnChange += ManagerOnChange;
            Inventory.OnUse += ManagerOnUse;

            _slotHead.UISystem = this;
            _slotTorso.UISystem = this;
            _slotPelvis.UISystem = this;

            UpdateUI();
        }

        private void OnDestroy()
        {
            Inventory.OnChange -= ManagerOnChange;
            Inventory.OnUse -= ManagerOnUse;
        }

        private void ManagerOnUse(InventoryObject obj, Character character)
        {
            // some item has been used
            UpdateUI();
        }

        private void ManagerOnChange()
        {
            // Some item has either been picked-up or dropped
            UpdateUI();
        }

        private void UpdateUI()
        {
            // Update UI
            List<EquipmentSlot> definedSlots = new List<EquipmentSlot>(Reflection.GetEnumValues<EquipmentSlot>());
            
            foreach (var equipment in Inventory.Equipment)
            {
                switch (equipment.Slot)
                {
                    case EquipmentSlot.Head:
                        _slotHead.SetSlot(equipment);
                        definedSlots.Remove(EquipmentSlot.Head);
                        break;
                    case EquipmentSlot.Torso:
                        _slotTorso.SetSlot(equipment);
                        definedSlots.Remove(EquipmentSlot.Torso);
                        break;
                    case EquipmentSlot.Pelvis:
                        _slotPelvis.SetSlot(equipment);
                        definedSlots.Remove(EquipmentSlot.Pelvis);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (definedSlots.Count <= 0) 
                    return;
            }
            
            foreach (var equipmentSlot in definedSlots)
            {
                switch (equipmentSlot)
                {
                    case EquipmentSlot.Head:
                        _slotHead.ResetSlot();
                        break;
                    case EquipmentSlot.Torso:
                        _slotTorso.ResetSlot();
                        break;
                    case EquipmentSlot.Pelvis:
                        _slotPelvis.ResetSlot();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}