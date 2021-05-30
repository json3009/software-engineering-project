using System;
using System.Collections.Generic;
using CharacterSystem;
using EquipmentSystem;
using InventorySystem.UI;
using OdinSerializer;
using UnityEngine;
using Utilities.Collections;
using Object = UnityEngine.Object;

namespace InventorySystem
{
    /// <summary>
    /// <para>The InventoryManager class provides a system to manage all <see cref="InventoryObject">Inventory Objects</see>.</para>
    /// <para>The class also provides callbacks events for changes in inventory.</para>
    /// </summary>
    [Serializable]
    public class InventoryManager
    {
        #region Fields

        [SerializeReference] private List<InventoryObject> _objects = new List<InventoryObject>();
        [SerializeReference] private List<InventoryEquipment> _equipment = new List<InventoryEquipment>();
        
        #endregion

        #region Properties

        /// <summary>
        /// Object in Inventory
        /// </summary>
        public List<InventoryObject> Objects => _objects;

        /// <summary>
        /// Equipment currently equipped.
        /// </summary>
        public List<InventoryEquipment> Equipment => _equipment;

        #endregion

        #region Events

        public event Action<InventoryObject> OnPickup;
        public event Action<InventoryObject> OnDrop;
        public event Action OnChange;
        public event Action<InventoryObject, Character> OnUse;

        #endregion

        #region Methods

        
        /// <summary>
        /// Pickup object.
        /// </summary>
        /// <param name="obj">The object to pick-up.</param>
        public void Pickup(InventoryObject obj)
        {
            if (obj == null)
                return;

            _objects.Add(obj);
            ObjectHasBeenPickedUp(obj);
            InventoryHasChanged();
        }

        /// <summary>
        /// Drop an item.
        /// </summary>
        /// <param name="obj">The object to drop.</param>
        /// <param name="position">The position at which to drop object.</param>
        /// <param name="source">The GameObject that dropped this object.</param>
        public void Drop(InventoryObject obj, Vector2 position, GameObject source = null)
        {
            if(!Delete(obj))
                return;
            
            GameObject dropPrefab = obj.DropPrefab;
            InventoryDrop drop;
            if (dropPrefab == null)
                drop = InventoryDrop.Create(obj);
            else if(dropPrefab.TryGetComponent(out drop))
                drop = Object.Instantiate(dropPrefab).GetComponent<InventoryDrop>();
            else 
                drop = Object.Instantiate(dropPrefab).AddComponent<InventoryDrop>();

            drop.DroppedBy = source;
            drop.DroppedItem = obj;
            drop.transform.position = position;
        }
        
        /// <summary>
        /// Delete an object in inventory.
        /// </summary>
        /// <param name="obj">The object to delete.</param>
        /// <returns><c><b>TRUE</b></c> if object could be deleted; <c><b>FALSE</b></c> otherwise.</returns>
        public bool Delete(InventoryObject obj)
        {
            if (obj == null || !_objects.Contains(obj))
                return false;

            _objects.Remove(obj);
            ObjectHasBeenDropped(obj);
            InventoryHasChanged();
            return true;
        }

        /// <summary>
        /// Use an object.
        /// </summary>
        /// <param name="obj">The object to use.</param>
        /// <param name="chr">The character to use object on.</param>
        public void UseItem(InventoryObject obj, Character chr)
        {
            obj.Use(this, chr);
            ObjectHasBeenUsed(obj, chr);
        }
        
        /// <summary>
        /// Apply equipped item stats.
        /// </summary>
        /// <param name="chr">The Character to apply stats to.</param>
        public void ApplyEquippedBuffs(Character chr)
        {
            foreach (InventoryEquipment inventoryEquipment in _equipment)
            {
                inventoryEquipment.ApplyBuffs(this, chr);
            }
        }

        #endregion

        #region Event Methods

        private void ObjectHasBeenPickedUp(InventoryObject obj)
        {
            OnPickup?.Invoke(obj);
        }

        private void ObjectHasBeenDropped(InventoryObject obj)
        {
            OnDrop?.Invoke(obj);
        }

        private void InventoryHasChanged()
        {
            OnChange?.Invoke();
        }

        private void ObjectHasBeenUsed(InventoryObject obj, Character chr)
        {
            OnUse?.Invoke(obj, chr);
        }

        #endregion

        #region Save / Load

        public byte[] SaveData()
        {
            return SerializationUtility.SerializeValue(_objects, DataFormat.Binary);
        }

        public void Load(byte[] data)
        {
            _objects = SerializationUtility.DeserializeValue<List<InventoryObject>>(data, DataFormat.Binary);
        }

        #endregion

        
    }
}
