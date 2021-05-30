using CharacterSystem;
using OdinSerializer;
using UnityEngine;

namespace InventorySystem
{
    /// <summary>
    /// <para>The InventoryObject class is an abstract representation of an item that can be used by the <see cref="InventoryManager">Inventory Manager</see>.</para>
    /// </summary>
    public abstract class InventoryObject : SerializedScriptableObject
    {

        [SerializeField] private GameObject _dropPrefab;


        /// <summary>
        /// Is the Item droppable.
        /// </summary>
        public virtual bool IsDroppable => true;

        /// <summary>
        /// Is the Item Stackable.
        /// </summary>
        public virtual bool IsStackable => true;

        /// <summary>
        /// The maximum amount that this item can be stacked.
        /// </summary>
        public virtual int MaxStack => 5;

        /// <summary>
        /// The name of the Item.
        /// </summary>
        public abstract string Name { get; }
        
        /// <summary>
        /// The description of the Item.
        /// </summary>
        public abstract string Description { get; }
        
        /// <summary>
        /// The Icon of the Item.
        /// </summary>
        public abstract Sprite Icon { get; }

        /// <summary>
        /// The Prefab to use whenever this item is to be dropped.
        /// </summary>
        public GameObject DropPrefab => _dropPrefab;

        /// <summary>
        /// Use this item.
        /// </summary>
        /// <param name="manager">The Inventory Manager.</param>
        /// <param name="chr">The Character to use this Item on.</param>
        public abstract void Use(InventoryManager manager, Character chr);
    }
}