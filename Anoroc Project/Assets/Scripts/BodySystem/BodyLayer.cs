using System;
using UnityEngine;
using Utilities;

namespace Scripts.BodySystem
{
    /// <summary>
    /// The layer of the body
    /// </summary>
    [Serializable]
    public class BodyLayer
    {
        [SerializeField] public string name;
        [SerializeField] public SerializableGUID id;
    }
}
