using System;
using Scripts.BodySystem;
using UnityEngine;
using Utilities;

namespace EquipmentSystem
{
    /// <summary>
    /// <para>The Equipment Item class is a abstract representation of a piece of equipment that can be used by the <see cref="EquipmentManager">Equipment Manager</see>.</para>
    /// </summary>
    public abstract class EquipmentItem : ScriptableObject
    {
        #region Fields
        
        [SerializeField] private string _name;
        [SerializeField] private BodyDefinition _definition;

        [SerializeField] private SerializableGUID _layer;
        [SerializeField] private SerializableGUID _equipmentSlot = Guid.Empty;

        [SerializeField] private Sprite _icon;

        #endregion

        #region Properties

        /// <summary>
        /// The name of the equipment.
        /// </summary>
        /// <remarks>Will use the asset name if left unset.</remarks>
        public string Name { get => _name.IsEmpty() ? name : _name; set => _name = value; }
        
        /// <summary>
        /// The <see cref="BodyLayer">Layer</see> on which the equipment rests.
        /// </summary>
        ///<remarks>
        /// Uses the <see cref="LayerID">LayerID</see> to GET &amp; SET the <see cref="BodyLayer">BodyLayer</see> object.
        /// </remarks>
        public BodyLayer Layer { get => Definition.GetLayer(_layer); set => _layer = value?.id ?? Guid.Empty; }
        
        /// <summary>
        /// The <see cref="BodyLayer.id">Layer ID</see> on which the equipment rests.
        /// </summary>
        public SerializableGUID LayerID { get => _layer; set => _layer = value; }
        
        /// <summary>
        /// The <see cref="BodyPartFlag">slot</see> on which to equip. 
        /// </summary>
        ///<remarks>
        /// Uses the <see cref="EquipmentSlotID">EquipmentSlotID</see> to GET &amp; SET the <see cref="BodyPartFlag">BodyPartFlag</see> object.
        /// </remarks>
        public BodyPartFlag EquipmentSlot { get => Definition.GetPartByID(_equipmentSlot) ?? BodyPartFlag.None; set => _equipmentSlot = value.id; }
        
        /// <summary>
        /// The <see cref="BodyPartFlag.id">slot ID</see> on which to equip. 
        /// </summary>
        public SerializableGUID EquipmentSlotID { get => _equipmentSlot; set => _equipmentSlot = value; }

        /// <summary>
        /// The Icon of the equipment.
        /// </summary>
        public Sprite Icon { get => _icon; set => _icon = value; }

        /// <summary>
        /// The <see cref="BodyDefinition"/> to use.
        /// </summary>
        public BodyDefinition Definition { get => _definition; set => _definition = value; }

        #endregion

        #region Methods

        /// <summary>
        /// Tests whether the Equipment Item is valid
        /// </summary>
        /// <returns><c><b>TRUE</b></c>, if valid; <c><b>FALSE</b></c> otherwise</returns>
        public bool IsValid()
        {
            if (_definition == null)
                return false;

            if (Layer == null)
                return false;

            return Validate();
        }

        #endregion

        #region Abstract / Virtual Methods

        /// <summary>
        /// Get the GameObject to render depending on the given Side.
        /// </summary>
        /// <param name="side">The <see cref="BodySide.id">side ID</see> to render</param>
        /// <param name="sortOrder">The sort order at which to draw the object</param>
        /// <returns>The created GameObject</returns>
        public abstract GameObject GetObjectForSide(SerializableGUID side, int sortOrder);

        /// <summary>
        /// Validate subClass 
        /// </summary>
        /// <returns><c><b>TRUE</b></c>, if valid; <c><b>FALSE</b></c> otherwise</returns>
        protected abstract bool Validate();

        #endregion
    }
}
