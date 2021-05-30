using System.Collections.Generic;
using Scripts.BodySystem;
using UnityEngine;

namespace EquipmentSystem
{

    /// <summary>
    /// The EquipmentSet class represents a collection of one or more <see cref="EquipmentItem">Equipment Items</see> on a single <see cref="BodyDefinition">Body Definition</see>
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Equipment/New Set of Equipment")]
    public class EquipmentSet : ScriptableObject
    {
        [SerializeField] private string _name;
        [SerializeField] private Sprite _icon;
        [SerializeField] private Texture2D _generatedIcon;
        [SerializeField] private EquipmentManager _items = new EquipmentManager();

        /// <summary>
        /// The name of the equipment Set.
        /// </summary>
        public string Name
        {
            get => _name;
            set => _name = value;
        }

        /// <summary>
        /// The Icon of the equipment Set.
        /// </summary>
        public Sprite Icon
        {
            get => _icon;
            set => _icon = value;
        }
        
        /// <summary>
        /// The Equipment Manager (handles all <see cref="EquipmentItem">Equipment Items</see>)
        /// </summary>
        public EquipmentManager Items => _items;

        
        /// <summary>
        /// Gets the generated Texture
        /// </summary>
        /// <remarks>
        /// Will be <c>NULL</c> if icon was set manually.
        /// </remarks>
        public Texture2D GeneratedIcon
        {
            get => _generatedIcon;
            set => _generatedIcon = value;
        }
    }
}
