using System;

namespace Utilities
{

    [Serializable]
    public struct SerializableGUID : IComparable, IComparable<SerializableGUID>, IEquatable<SerializableGUID>
    {
        public string Value;

        private SerializableGUID(string value)
        {
            Value = value;
        }

        public static implicit operator SerializableGUID(Guid guid)
        {
            return new SerializableGUID(guid.ToString());
        }

        public static implicit operator Guid(SerializableGUID serializableGuid)
        {
            return new Guid(serializableGuid.Value);
        }

        public int CompareTo(object value)
        {
            if (value == null)
                return 1;
            if (!(value is SerializableGUID))
                throw new ArgumentException("Must be SerializableGuid");
            SerializableGUID guid = (SerializableGUID)value;
            return guid.Value == Value ? 0 : 1;
        }

        public int CompareTo(SerializableGUID other)
        {
            return other.Value == Value ? 0 : 1;
        }

        public bool Equals(SerializableGUID other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return (Value != null ? Value.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return (Value != null ? new Guid(Value).ToString() : string.Empty);
        }
    }
}
