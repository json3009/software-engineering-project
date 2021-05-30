using OdinSerializer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace StatSystem
{
    [Serializable]
    public class StatType : IEquatable<StatType>
    {
        [SerializeField] private readonly string _id;
        [SerializeField] private string _name;
        [SerializeField] private string _desc;
        [OdinSerialize] private Type _type;
        [SerializeField] private bool _hidden = false;

        public string ID => _id;

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public string Description
        {
            get => _desc;
            set => _desc = value;
        }

        public bool Hidden
        {
            get => _hidden;
            set => _hidden = value;
        }

        public Type Type
        {
            get => _type;
            set => _type = value;
        }

        public StatType(string id, Type type, string name, string description)
        {
            _id = id;
            _type = type;
            _name = name;
            _desc = description;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as StatType);
        }

        public bool Equals(StatType other)
        {
            return other != null &&
                   _id == other._id;
        }

        public override int GetHashCode()
        {
            return 1969571243 + EqualityComparer<string>.Default.GetHashCode(_id);
        }

        public static bool operator ==(StatType left, StatType right)
        {
            return EqualityComparer<StatType>.Default.Equals(left, right);
        }

        public static bool operator !=(StatType left, StatType right)
        {
            return !(left == right);
        }
    }
}