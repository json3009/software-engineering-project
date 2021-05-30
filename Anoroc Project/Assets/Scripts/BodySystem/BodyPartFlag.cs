using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Scripts.BodySystem
{
    [Serializable]
    public class BodyPartFlag : IComparable, IEquatable<BodyPartFlag>
    {
        public string name;
        public SerializableGUID id;

        [SerializeReference] public BodyPartFlag parent;
        [SerializeReference] private List<BodyPartFlag> children = null;


        public static BodyPartFlag None { get => new BodyPartFlag() { name = "None", id = Guid.Empty }; }
        public List<BodyPartFlag> Children { get => children; }

        public bool AddChild(BodyPartFlag bodyPart)
        {
            if (children == null)
                children = new List<BodyPartFlag>();

            if (bodyPart.id.Value.IsEmpty())
                return false;

            if (children.Exists((e) => e.id.Equals(bodyPart.id)))
                return false;

            children.Add(bodyPart);
            return true;
        }

        public bool RemoveChild(BodyPartFlag bodyPart)
        {
            if (children == null)
                return false;

            return children.Remove(bodyPart);
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;
            if (!(obj is BodyPartFlag))
                throw new ArgumentException("Must be BodyPartFlag");

            return id.CompareTo((BodyPartFlag)obj);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as BodyPartFlag);
        }

        public bool Equals(BodyPartFlag other)
        {
            return other != null &&
                   id.Equals(other.id);
        }

        public override int GetHashCode()
        {
            return 1877310944 + id.GetHashCode();
        }

        public static bool operator ==(BodyPartFlag left, BodyPartFlag right)
        {
            return EqualityComparer<BodyPartFlag>.Default.Equals(left, right);
        }

        public static bool operator !=(BodyPartFlag left, BodyPartFlag right)
        {
            return !(left == right);
        }
    }
}
