using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Scripts.CombatSystem
{
    public partial class DamageDefinition
    {
        [Serializable]
        internal struct InternalDamageMatrixKey : IEquatable<InternalDamageMatrixKey>
        {
            [SerializeField] private SerializableGUID _attacker;
            [SerializeField] private SerializableGUID _defender;

            public SerializableGUID Attacker { get => _attacker; }
            public SerializableGUID Defender { get => _defender; }

            public InternalDamageMatrixKey(DamageType attacker, DamageType defender)
                : this(attacker.ID, defender.ID) { }

            public InternalDamageMatrixKey(SerializableGUID attacker, SerializableGUID defender)
            {
                _attacker = attacker;
                _defender = defender;
            }

            public void Deconstruct(out SerializableGUID attacker, out SerializableGUID defender)
            {
                attacker = _attacker;
                defender = _defender;
            }

            public override bool Equals(object obj)
            {
                return obj is InternalDamageMatrixKey key && Equals(key);
            }

            public bool Equals(InternalDamageMatrixKey other)
            {
                return _attacker.Equals(other._attacker) &&
                       _defender.Equals(other._defender);
            }

            public override int GetHashCode()
            {
                int hashCode = 1843022434;
                hashCode = hashCode * -1521134295 + _attacker.GetHashCode();
                hashCode = hashCode * -1521134295 + _defender.GetHashCode();
                return hashCode;
            }

            public static implicit operator InternalDamageMatrixKey((SerializableGUID attacker, SerializableGUID defender) value)
            {
                return new InternalDamageMatrixKey(value.attacker, value.defender);
            }

            public static bool operator ==(InternalDamageMatrixKey left, InternalDamageMatrixKey right)
            {
                return EqualityComparer<InternalDamageMatrixKey>.Default.Equals(left, right);
            }

            public static bool operator !=(InternalDamageMatrixKey left, InternalDamageMatrixKey right)
            {
                return !(left == right);
            }

        }
    }
}
