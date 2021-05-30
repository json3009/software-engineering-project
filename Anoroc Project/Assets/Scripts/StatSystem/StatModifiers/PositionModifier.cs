using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace StatSystem.StatModifiers
{
    public class PositionModifier : StatModifier<Vector3>
    {

        public enum PositionModifierType
        {
            Constant,
            Object
        }

        [SerializeField] private PositionModifierType _type;
        [SerializeField] private GameObject _target;

        public PositionModifierType Type => _type;
        public new Vector3 Value
        {
            get
            {
                return _type switch
                {
                    PositionModifierType.Constant => base.Value,
                    PositionModifierType.Object => _target.transform.position,
                    _ => base.Value,
                };
            }
        }


        public PositionModifier() : this(Vector3.zero) { }

        public PositionModifier(Vector3 value, int order = 0, UnityEngine.Object source = null) : base(value, order, source)
        {
            _type = PositionModifierType.Constant;
        }

        public PositionModifier(GameObject value, UnityEngine.Object source = null) : base(value.transform.position, 0, source)
        {
            _type = PositionModifierType.Object;
            _target = value;
        }

        public override VisualElement CreateGUI(Action onChange)
        {
            throw new NotImplementedException();
        }

        protected override void AddToClone(StatModifier<Vector3> obj)
        {
            if(!(obj is PositionModifier copy)) return;

            copy._target = _target;
            copy._type = _type;
        }
    }
}
