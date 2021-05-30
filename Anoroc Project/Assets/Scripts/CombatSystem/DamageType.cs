using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;
using Utilities.Collections;

namespace Scripts.CombatSystem
{
    [Serializable]
    public class DamageType
    {
        [SerializeField] private SerializableGUID _id;
        [SerializeField] private string _name;
        [SerializeField] private Sprite _icon;
        [SerializeField] private List<Sprite> _textures = new List<Sprite>();
        [SerializeField] private Color _color;

        public SerializableGUID ID { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public Sprite Icon { get => _icon; set => _icon = value; }
        public List<Sprite> Textures { get => _textures; set => _textures = value; }
        public Color Color { get => _color; set => _color = value; }
    }
}