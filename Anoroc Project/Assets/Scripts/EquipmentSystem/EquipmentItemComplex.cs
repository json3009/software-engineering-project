using UnityEngine;
using Utilities;
using Utilities.Collections;

namespace EquipmentSystem
{
    [CreateAssetMenu(menuName = "Game/Equipment/New Item (Complex)")]
    public class EquipmentItemComplex : EquipmentItem
    {
        [SerializeField] private SerializableDictionary<SerializableGUID, GameObject> objectsBySide = new SerializableDictionary<SerializableGUID, GameObject>();

        [SerializeField] private GameObject theObject;

        public GameObject TheObject { get => theObject; set => theObject = value; }

        public override GameObject GetObjectForSide(SerializableGUID side, int sortOrder)
        {
            //throw new NotImplementedException();
            return GetObject(side);
        }

        public GameObject GetObject(SerializableGUID side)
        {
            if (objectsBySide.TryGetValue(side, out GameObject o))
                return o;

            return null;
        }

        public void SetObject(SerializableGUID id, GameObject o)
        {
            if (!objectsBySide.ContainsKey(id))
                objectsBySide.Add(id, o);

            objectsBySide[id] = o;
        }

        protected override bool Validate()
        {
            return theObject != null;
        }
    }
}
