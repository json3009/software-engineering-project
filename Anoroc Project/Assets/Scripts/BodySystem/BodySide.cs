using System;
using UnityEngine;
using Utilities;

namespace Scripts.BodySystem
{
    /// <summary>
    /// A side of the Body
    /// </summary>
    [Serializable]
    public class BodySide
    {
        [SerializeField] public string name;
        [SerializeField] public SerializableGUID id;
    }
}
