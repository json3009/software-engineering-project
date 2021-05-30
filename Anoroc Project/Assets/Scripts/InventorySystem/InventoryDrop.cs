using CharacterSystem;
using EventSystem;
using UnityEngine;

namespace InventorySystem
{
    
    /// <summary>
    /// <para>The InventoryDrop class is a wrapper component that handles dropped items in world space.</para>
    /// <para>It also handles pickup as well as auto rescaling of sprites to conform to a specific defined size.</para>
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    [ExecuteInEditMode]
    public class InventoryDrop : MonoBehaviour
    {
        private BoxCollider2D _collider;
        private GameObject _droppedBy;
        
        [SerializeField] private int _size = 1;

        [SerializeField] private InventoryObject _droppedItem;
        [SerializeField] private SpriteRenderer _renderer;


        /// <summary>
        /// The dropped Item
        /// </summary>
        public InventoryObject DroppedItem
        {
            get => _droppedItem;
            set => SetUpDrop(value);
        }

        /// <summary>
        /// The GameObject that dropped this item.
        /// </summary>
        public GameObject DroppedBy
        {
            get => _droppedBy;
            set => _droppedBy = value;
        }

        private void Start()
        {
            _collider = GetComponent<BoxCollider2D>();
            _collider.isTrigger = true;

            var component = GetComponent<Rigidbody2D>();
            component.isKinematic = true;
            
            if (_droppedItem != null)
                SetUpDrop(_droppedItem);
        }

        private void SetUpDrop(InventoryObject obj)
        {
            if (obj == null)
                return;

            if (!_collider)
                _collider = GetComponent<BoxCollider2D>();

            _droppedItem = obj;

            if (_renderer == null)
            {
                _renderer = new GameObject("GFX").AddComponent<SpriteRenderer>();
                _renderer.transform.SetParent(transform, false);
            }
            
            _renderer.sprite = Sprite.Create(
                _droppedItem.Icon.texture, 
                _droppedItem.Icon.rect,
                new Vector2(0.5f, 0.5f), 
                _droppedItem.Icon.pixelsPerUnit
            );
            
            _renderer.spriteSortPoint = SpriteSortPoint.Pivot;
            

            var scale = (float) _size / (float) _droppedItem.Icon.bounds.size.x;
            
            var localScale = Vector3.one * scale;
            _renderer.transform.localScale = localScale;

            _collider.size = Vector2.one * ((float)_size);
        }


        /// <summary>
        /// Create a new Inventory drop.
        /// </summary>
        /// <param name="obj">The item to drop.</param>
        /// <returns>The reference to the component.</returns>
        public static InventoryDrop Create(InventoryObject obj)
        {
            var gameObject = new GameObject($"Drop [{obj.Name}]");
            var drop = gameObject.AddComponent<InventoryDrop>();
            drop.SetUpDrop(obj);
            
            return drop;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!TryGetComponentInChildren(other.gameObject, out Character character)) return;
            
            if(DroppedBy != null && DroppedBy == character.gameObject)
                return;
            
            character.Inventory.Pickup(_droppedItem);
            Destroy(gameObject);

            GlobalEventSystem.Instance.CharacterHasPickedItemUp(_droppedItem, character);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!TryGetComponentInChildren(other.gameObject, out Character character)) return;
            
            if(DroppedBy != null && DroppedBy == character.gameObject)
                DroppedBy = null;
        }

        private static bool TryGetComponentInChildren<T>(GameObject obj, out T component) where T : MonoBehaviour
        {
            component = obj.GetComponentInChildren<T>();

            return component != null;
        }
    }
}