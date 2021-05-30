using Scripts.BodySystem;
using UnityEngine;
using Utilities;
using Utilities.Collections;

namespace EquipmentSystem
{
    /// <summary>
    /// <para>A simple piece of equipment.</para>
    /// <inheritdoc cref="EquipmentItem" path="/summary"/>
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Equipment/New Item (Simple)")]
    public class EquipmentItemSimple : EquipmentItem
    {
        [SerializeField] private SerializableDictionary<SerializableGUID, Sprite> sprites = new SerializableDictionary<SerializableGUID, Sprite>();
        
        /// <summary>
        /// Dictionary of sprites [key: <see cref="BodySide">Side</see>. value: Sprite]
        /// </summary>
        public SerializableDictionary<SerializableGUID, Sprite> Sprites { get => sprites; set => sprites = value; }

        /// <inheritdoc />
        public override GameObject GetObjectForSide(SerializableGUID side, int sortOrder)
        {
            if (GetSprite(side) == null) return null;

            GameObject newObj = new GameObject(Name);
            newObj.transform.localPosition = Vector3.zero;
            newObj.transform.localScale = Vector3.one;
            newObj.transform.localRotation = Quaternion.identity;

            SpriteRenderer r = newObj.AddComponent<SpriteRenderer>();
            r.sprite = GetSprite(side);
            r.sortingOrder = sortOrder;

            return newObj;
        }

        /// <summary>
        /// Get Equipment Sprite by <see cref="BodySide.id">side ID</see>.
        /// </summary>
        /// <param name="side">The side ID.</param>
        /// <returns></returns>
        public Sprite GetSprite(SerializableGUID side)
        {
            if (Sprites.TryGetValue(side, out Sprite s))
                return s;

            return null;
        }

        /// <summary>
        /// Set Equipment Sprite by <see cref="BodySide.id">side ID</see>.
        /// </summary>
        /// <param name="id">The side ID.</param>
        /// <param name="sprite">The Sprite.</param>
        public void SetSprite(SerializableGUID id, Sprite sprite)
        {
            if (!sprites.ContainsKey(id))
                sprites.Add(id, sprite);

            sprites[id] = sprite;
        }

        /// <inheritdoc />
        protected override bool Validate()
        {
            return sprites.Count > 0;
        }
    }
}
