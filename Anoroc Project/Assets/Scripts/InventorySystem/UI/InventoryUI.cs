using System;
using System.Linq;
using CharacterSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace InventorySystem.UI
{
    public class InventoryUI : MonoBehaviour, IInventoryUI
    {
        [SerializeField] private Character _character;
        [SerializeField] private Transform _slotParent;
        
        private InventorySlot[] _inventorySlots;
        
        public InventoryManager Inventory => _character.Inventory;
        public Transform SlotParent => _slotParent;

        public Character Character => _character;

        private void Start()
        {
            Inventory.OnChange += ManagerOnChange;
            Inventory.OnUse += ManagerOnUse;

            _inventorySlots = _slotParent.GetComponentsInChildren<InventorySlot>();
            foreach (var inventorySlot in _inventorySlots)
                inventorySlot.UISystem = this;
            
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
            UpdateUI();
        }

        private void UpdateUI()
        {
            var inventoryObjects = Inventory.Objects
                .Where((e)=>!Inventory.Equipment.Contains(e))
                .ToArray();
            
            // Update UI
            for (var i = 0; i < _inventorySlots.Length; i++)
            {
                if(i < inventoryObjects.Length)
                {
                    _inventorySlots[i].SetSlot(inventoryObjects[i]);
                }
                else
                    _inventorySlots[i].ResetSlot();
            }
        }
    }
}