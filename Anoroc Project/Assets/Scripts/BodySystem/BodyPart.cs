using System;
using UnityEngine;
using Utilities;

namespace Scripts.BodySystem
{
    /// <summary>
    /// The BodyPart class is a data component class that holds data related to the body.
    /// </summary>
    /// <remarks>Generally, each body part slot will have a BodyPart component.</remarks>
    public class BodyPart : MonoBehaviour
    {
        [SerializeField] private SerializableGUID flagID;
        [SerializeField] private BodyBase bodyBase;

        [NonSerialized] private BodyPartFlag flag = null;

        /// <summary>
        /// The ID of the Body Part (defined in <see cref="BodyDefinition">Body definition</see>)
        /// </summary>
        public SerializableGUID FlagID { get => flagID; set => flagID = value; }
        
        /// <summary>
        /// The BodyBase Component reference.
        /// </summary>
        public BodyBase BodyBase { get => bodyBase; set => bodyBase = value; }
        
        /// <summary>
        /// The Body Definition used.
        /// </summary>
        public BodyDefinition Definition { get => bodyBase.Body; }
        
        /// <summary>
        /// The BodyPart Flag, calculated from both the Definition and the FlagID.
        /// </summary>
        public BodyPartFlag Flag { get { if (flag == null) flag = Definition.GetPartByID(flagID); return flag; } }

    }
}
