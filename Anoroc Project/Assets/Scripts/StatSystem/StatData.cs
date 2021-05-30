using System;
using System.Collections.Generic;
using System.Linq;
using InventorySystem;
using OdinSerializer;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace StatSystem
{

    [Serializable]
    public class StatData
    {
        [Serializable]
        public class DataWrapper : ICloneable
        {
            [OdinSerialize, NonSerialized] private StatType _type;
            [OdinSerialize, NonSerialized] private IStatAttribute _attribute;

            public StatType Type { get => _type; }
            public IStatAttribute Attribute { get => _attribute; }

            public DataWrapper(StatType type, IStatAttribute attribute)
            {
                _type = type;
                _attribute = attribute;
            }

            public object Clone()
            {
                return SerializationUtility.CreateCopy(this);
            }
        }

        [OdinSerialize, NonSerialized] private List<DataWrapper> _modifiers = new List<DataWrapper>();

        public List<DataWrapper> Modifiers { get => _modifiers; }

        public IStatAttribute GetAttribute(string id)
        {
            return _modifiers
                .Where((e) => e.Type.ID.Equals(id))
                .Select((e) => e.Attribute)
                .FirstOrDefault();
        }
        
        public T GetAttribute<T>(string id) where T : IStatAttribute
        {
            var attr = GetAttribute(id);
            if (attr.GetType() != typeof(T))
                throw new InvalidCastException();

            return (T) attr;
        }


        public bool TryGetAttribute(string id, out IStatAttribute attribute)
        {
            attribute = GetAttribute(id);
            return attribute != null;
        }

        public bool TryGetAttribute<T>(string id, out T attribute) where T : IStatAttribute
        {
            attribute = GetAttribute<T>(id);
            return attribute != null;
        }

        public StatData Combine(StatData data)
        {
            StatData newData = new StatData();

            Dictionary<string, DataWrapper> attributes = new Dictionary<string, DataWrapper>();

            foreach (var item in _modifiers)
                attributes.Add(item.Type.ID, (DataWrapper) item.Clone());
            
            foreach (var item in data._modifiers)
            {
                if (attributes.TryGetValue(item.Type.ID, out DataWrapper wrapper))
                {
                    wrapper.Attribute.Merge(item.Attribute);
                }
                else
                {
                    attributes.Add(item.Type.ID, (DataWrapper) item.Clone());
                }
            }

            newData._modifiers.AddRange(attributes.Values.ToList());
            return newData;
        }

        public void AddNewAttribute(StatType type, out IStatAttribute statAttribute)
        {
            statAttribute = (IStatAttribute)Activator.CreateInstance(type.Type);
            Modifiers.Add(new DataWrapper(type, statAttribute));
        }

        public void AddNewAttribute<T>(StatType type, out T statAttribute) where T : IStatAttribute
        {
            AddNewAttribute(type, out IStatAttribute stat);
            if (stat.GetType() == typeof(T))
                statAttribute = (T) stat;
            else
                throw new InvalidCastException();
        }

        public IStatAttribute GetOrAddAttribute(StatType attr)
        {
            if (TryGetAttribute(attr.ID, out IStatAttribute foundAttr))
                return foundAttr;
            
            AddNewAttribute(attr, out IStatAttribute statAttribute);
            return statAttribute;
        }

        public void AddDataFromSource(UnityEngine.Object source, StatData data)
        {
            Dictionary<string, DataWrapper> attributes = new Dictionary<string, DataWrapper>();

            foreach (var item in _modifiers)
                attributes.Add(item.Type.ID, item);
            
            foreach (var item in data._modifiers)
            {
                if (attributes.TryGetValue(item.Type.ID, out DataWrapper wrapper))
                {
                    foreach (var externalModifier in item.Attribute.Modifiers)
                    {
                        var clone = (IStatModifier)externalModifier.Clone();
                        clone.Source = source;
                        wrapper.Attribute.AddModifier(clone);
                    }
                }
                else
                {
                    var clone = (DataWrapper) item.Clone();
                    attributes.Add(item.Type.ID, (DataWrapper) clone);
                    
                    foreach (var externalModifier in clone.Attribute.Modifiers)
                    {
                        externalModifier.Source = source;
                    }
                }
            }
            
            _modifiers.Clear();
            _modifiers.AddRange(attributes.Values);
        }

        public void RemoveDataWithSource(UnityEngine.Object source)
        {
            foreach (var item in _modifiers)
            {
                item.Attribute.RemoveModifiers(source);
            }
        }


    }
}
