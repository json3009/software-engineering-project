using System.Linq;
using UnityEngine;
using Utilities;
using Utilities.Collections;

namespace Scripts.BodySystem
{
    /// <summary>
    /// <para>The BodyBase is a Data Component containing each body part that exists for the current body.</para>
    /// </summary>
    /// <remarks>The BodyBase component get automatically generated from the <see cref="BodyFactory">Body Factory</see> class.</remarks>
    public class BodyBase : MonoBehaviour
    {
        [SerializeField] private SerializableDictionary<BodyPartFlag, BodyPart> _attributions = new SerializableDictionary<BodyPartFlag, BodyPart>();

        [SerializeField] private BodyDefinition body;

        [SerializeField] private SerializableGUID sideID;

        /// <summary>
        /// The <see cref="BodyDefinition">Body Definition</see> used for this Body.
        /// </summary>
        public BodyDefinition Body { get => body; set => body = value; }
        
        /// <summary>
        /// The <see cref="BodySide">side</see> ID for the body.
        /// </summary>
        /// <remarks>Each side generally contains a BodyBase component.</remarks>
        public SerializableGUID SideID { get => sideID; set => sideID = value; }

        /// <summary>
        /// Get the <see cref="BodyPart">BodyPart</see> attributed to the given <see cref="BodyPartFlag.id">slot ID</see>.
        /// </summary>
        /// <param name="id">The Body Part slot ID.</param>
        /// <returns>The <see cref="BodyPart">BodyPart</see> object if found; <c>NULL</c> otherwise.</returns>
        public BodyPart GetGameObjectAttributedToSlot(string id)
        {
            return _attributions
                .Where(e => e.Key.id.Value.Equals(id))
                .Select(e => e.Value)
                .FirstOrDefault();
        }

        /// <summary>
        /// Get all attributed <see cref="BodyPartFlag">slots</see> on Body.
        /// </summary>
        /// <returns>An array of BodyParts.</returns>
        public BodyPart[] GetAllOccupiedSlots()
        {
            return _attributions.Select((e) => e.Value).ToArray();
        }

        /// <summary>
        /// Get the <see cref="BodyPart">BodyPart</see> attributed to the given <see cref="BodyPartFlag">slot</see>.
        /// </summary>
        /// <param name="slot">The Body Part slot.</param>
        /// <returns>The <see cref="BodyPart">BodyPart</see> object if found; <c>NULL</c> otherwise.</returns>
        public BodyPart GetSlot(BodyPartFlag slot)
        {
            if (_attributions.TryGetValue(slot, out BodyPart obj))
                return obj;
            
            return null;
        }

        /// <summary>
        /// Attributes the given <see cref="BodyPart">BodyPart</see> to the <see cref="BodyPartFlag">Body part slot</see>.
        /// </summary>
        /// <param name="slot">The slot to assign the BodyPart to.</param>
        /// <param name="obj">The BodyPart to assign.</param>
        public void SetSlot(BodyPartFlag slot, BodyPart obj)
        {
            if (_attributions.ContainsKey(slot))
                _attributions[slot] = obj;
            else
                _attributions.Add(slot, obj);
        }

        /// <summary>
        /// Get the side.
        /// </summary>
        /// <returns>The current Body Side.</returns>
        public BodySide GetSide()
        {
            return Body.GetSide(sideID);
        }
    }
}
