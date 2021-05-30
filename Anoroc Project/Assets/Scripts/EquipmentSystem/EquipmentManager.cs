using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.BodySystem;
using UnityEngine;
using Utilities;

namespace EquipmentSystem
{
    /// <summary>
    /// <para>The EquipmentManager class provides a system to manage all equipment pieces (<see cref="EquipmentItem">Equipment Items</see> or <see cref="EquipmentSet">Equipment Sets</see>).</para>
    /// <para>The class also provides callbacks events for changes in equipment.</para>
    /// </summary>
    [Serializable]
    public class EquipmentManager
    {

        [Serializable]
        private class EquipmentLayer
        {
            [SerializeField] public List<EquipmentItem> _objects = new List<EquipmentItem>();

            public static implicit operator List<EquipmentItem>(EquipmentLayer m)
            {
                return m._objects;
            }
        }

        #region Events

        public event Action<BodyPartFlag, SerializableGUID, EquipmentItem> OnEquip;
        public event Action<BodyPartFlag, SerializableGUID, EquipmentItem> OnUnEquip;
        public event Action<BodyPartFlag, SerializableGUID, EquipmentItem> OnEquipmentChange;

        #endregion

        #region Fields


        [SerializeField] private List<EquipmentItem> values = new List<EquipmentItem>();
        [SerializeField] private List<EquipmentSet> _sets = new List<EquipmentSet>();
        [NonSerialized] private Dictionary<SerializableGUID, EquipmentLayer> _layers;
        
        #endregion

        #region Properties

        private Dictionary<SerializableGUID, EquipmentLayer> Layers
        {
            get
            {
                if (_layers == null) PopulateDict();
                return _layers;
            }
        }

        /// <summary>
        /// Get all EquipmentItems.
        /// </summary>
        public EquipmentItem[] Values => values.ToArray();
        
        #endregion
        
        private void PopulateDict()
        {
            _layers = new Dictionary<SerializableGUID, EquipmentLayer>();
            foreach (var item in values)
            {
                GetOrCreateLayer(item.LayerID)._objects.Add(item);
            }
        }

        /// <summary>
        /// Equip new Equipment piece, uses equipment piece to determine the slot
        /// </summary>
        /// <param name="toEquip">The equipment to equip</param>
        /// <returns><c><b>TRUE</b></c>, if equipment could be equipped; <c><b>FALSE</b></c> otherwise</returns>
        public bool Equip(EquipmentItem toEquip)
        {
            if (!CanEquip(toEquip))
                return false;

            AddObject(toEquip);

            OnEquip?.Invoke(toEquip.EquipmentSlot, toEquip.LayerID, toEquip);
            OnEquipmentChange?.Invoke(toEquip.EquipmentSlot, toEquip.LayerID, toEquip);

            return true;
        }

        /// <summary>
        /// Equip new Equipment Set, uses equipment piece to determine the slot
        /// </summary>
        /// <param name="set">The equipment set to equip</param>
        /// <returns><c><b>TRUE</b></c>, if equipment could be equipped; <c><b>FALSE</b></c> otherwise</returns>
        public bool Equip(EquipmentSet set)
        {
            if (set == null) 
                return false;

            if (set.Items.values.Count > 0)
            {
                if (!CanEquip(set))
                    return false;
                
                foreach (var equipmentItem in set.Items.values)
                    Equip(equipmentItem);
                
                _sets.Add(set);
            }

            return true;
        }

        /// <summary>
        /// Checks whether an <see cref="EquipmentItem">Equipment Item</see> can be equipped or not.
        /// </summary>
        /// <param name="toEquip">The Item to equip.</param>
        /// <returns><c><b>TRUE</b></c>, if equipment can be equipped; <c><b>FALSE</b></c> otherwise</returns>
        public bool CanEquip(EquipmentItem toEquip)
        {
            if (toEquip == null || toEquip.EquipmentSlot.Equals(BodyPartFlag.None))
                return false;

            if (GetEquipment(toEquip.EquipmentSlot, toEquip.LayerID) != null)
                return false;

            return true;
        }

        /// <summary>
        /// Checks whether an <see cref="EquipmentSet">Equipment Set</see> can be equipped or not.
        /// </summary>
        /// <param name="toEquip">The Set to equip.</param>
        /// <returns><c><b>TRUE</b></c>, if set can be equipped; <c><b>FALSE</b></c> otherwise</returns>
        public bool CanEquip(EquipmentSet toEquip)
        {
            if (toEquip == null || _sets.Contains(toEquip))
                return false;
            
            foreach (var equipmentItem in toEquip.Items.values)
                if (!CanEquip(equipmentItem)) 
                    return false;

            return true;
        }
        
        /// <summary>
        /// DeEquip equipment given the slot
        /// </summary>
        /// <param name="toDeEquip">The Slot to deEquip</param>
        /// <param name="layer">The layer from which to unEquip from</param>
        public bool UnEquip(BodyPartFlag toDeEquip, SerializableGUID layer)
        {
            var slot = GetEquipment(toDeEquip, layer);

            if (toDeEquip == null || slot.EquipmentSlot.Equals(BodyPartFlag.None))
                return false;

            RemoveObject(slot);

            OnUnEquip?.Invoke(slot.EquipmentSlot, slot.LayerID, slot);
            OnEquipmentChange?.Invoke(slot.EquipmentSlot, slot.LayerID, slot);

            return true;
        }

        /// <summary>
        /// DeEquip equipment given the Equipment
        /// </summary>
        /// <param name="toDeEquip">The Equipment to deEquip</param>
        /// <returns>True, if equipment could be unEquipped; False, otherwise</returns>
        public bool UnEquip(EquipmentItem toDeEquip)
        {
            if (!GetOrCreateLayer(toDeEquip.LayerID)._objects.Contains(toDeEquip))
                return false;

            return UnEquip(toDeEquip.EquipmentSlot, toDeEquip.LayerID);
        }

        /// <summary>
        /// DeEquip equipment set
        /// </summary>
        /// <param name="set">The Equipment to deEquip</param>
        /// <returns>True, if equipment set could be unEquipped; False, otherwise</returns>
        public bool UnEquip(EquipmentSet set)
        {
            if (!_sets.Contains(set))
                return false;

            foreach (var item in set.Items.values)
                UnEquip(item);

            _sets.Remove(set);

            return true;
        }
        
        /// <summary>
        /// Get Equipment at a specific slot.
        /// </summary>
        /// <param name="equipmentSlot">The Slot to get the equipment by</param>
        /// <returns>The Equipment if found; Null otherwise</returns>
        public EquipmentItem GetEquipment(BodyPartFlag equipmentSlot, SerializableGUID layer)
        {
            if (!Layers.ContainsKey(layer))
                return null;

            return GetOrCreateLayer(layer)._objects
                .Where((e) => e.EquipmentSlot.Equals(equipmentSlot))
                .FirstOrDefault();
        }


        /// <summary>
        /// Gets all used slots in equipment System
        /// </summary>
        /// <returns></returns>
        public BodyPartFlag[] GetAllUsedSlots(SerializableGUID layer)
        {
            if (GetOrCreateLayer(layer)._objects.Count() == 0)
                return new BodyPartFlag[0];

            return GetOrCreateLayer(layer)._objects.Select((e) => e.EquipmentSlot).ToArray();
        }


        private EquipmentLayer GetOrCreateLayer(SerializableGUID layer)
        {
            if (Layers.TryGetValue(layer, out EquipmentLayer equipmentLayer))
                return equipmentLayer;

            EquipmentLayer newLayer = new EquipmentLayer();
            Layers.Add(layer, newLayer);
            return newLayer;
        }


        private void AddObject(EquipmentItem obj)
        {
            values.Add(obj);
            GetOrCreateLayer(obj.LayerID)._objects.Add(obj);
        }

        private void RemoveObject(EquipmentItem obj)
        {
            values.Remove(obj);
            GetOrCreateLayer(obj.LayerID)._objects.Remove(obj);
        }
    }

}