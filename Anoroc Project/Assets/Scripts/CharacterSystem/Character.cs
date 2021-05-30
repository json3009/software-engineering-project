using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.BodySystem;
using CombatSystem.SpellSystem;
using CombatSystem.Stats.Modifiers;
using EquipmentSystem;
using EventSystem;
using InventorySystem;
using OdinSerializer;
using Scripts.CombatSystem;
using StatSystem;
using UnityEditor;
using UnityEngine;
using Utilities;
using Utilities.Collections;

namespace CharacterSystem
{
    /// <summary>
    /// The Character system handles everything from inventory / equipment to health.
    /// </summary>
    [ExecuteInEditMode, SelectionBase]
    public class Character : SerializedMonoBehaviour
    {
        /// <summary>
        /// The level thresholds, to level up spells. 
        /// </summary>
        private static readonly int[] LEVEL_THRESHOLD =
        {
            0,      // level 0
            2,      // level 1
            3,     // level 2
            4,     // level 3
            5,     // level 4
            7,     // level 5
            8,    // level 6 (GOD level)
        };
        
        /*private static readonly int[] LEVEL_THRESHOLD =
        {
            0,      // level 0
            5,      // level 1
            10,     // level 2
            25,     // level 3
            50,     // level 4
            70,     // level 5
            150,    // level 6 (GOD level)
        };*/
        
        
        [Serializable]
        internal class ObjectsPerSide
        {
            [SerializeField] private SerializableDictionary<string, GameObject> _objs = new SerializableDictionary<string, GameObject>();

            public SerializableDictionary<string, GameObject> Objs { get => _objs; }
        }
        #region Fields

        [SerializeField] private CharacterStats _characterStats;
        [SerializeField] private SpellStatTraits  _spellStats;

        [OdinSerialize] private StatData _spellData;
        [OdinSerialize] private StatData _characterData;
        
        [SerializeField] private List<BodyBase> _bases;
        [SerializeField] private BodyDefinition _definition;

        [SerializeField] private EquipmentManager _equipment = new EquipmentManager();
        [SerializeField] private InventoryManager _inventory = new InventoryManager();

        [SerializeField] private SerializableDictionary<SerializableGUID, ObjectsPerSide> _sides = new SerializableDictionary<SerializableGUID, ObjectsPerSide>();

        [SerializeField] private SerializableGUID currentSide;

        [SerializeField] private CharacterStat<float> _health = new CharacterStat<float>(0, 120, 120);
        [SerializeField] private CharacterStat<float> _mana =   new CharacterStat<float>(0, 100, 100);

        [SerializeField] private bool _canPickupItems = false;
        
        
        private IStatAttribute _defenceDamageTypeAttr;

        private DamageTypeModifier _defenceDamageType;

        // item1: currentLevel
        // item2: currentXP
        private readonly Dictionary<SpellArchetype, (int, int)> _levels = new Dictionary<SpellArchetype, (int, int)>();

        //[SerializeField] private DamageDefinition _damageDefinition;
        //[SerializeField] private List<SerializableGUID> _resistances;

        #endregion

        #region Properties

        /// <summary>
        /// The Equipment System (handles worn equipment).
        /// </summary>
        public EquipmentManager Equipment => _equipment;
        
        /// <summary>
        /// The Inventory System (handles items and equipment not worn).
        /// </summary>
        public InventoryManager Inventory => _inventory;
        
        /// <summary>
        /// Bases contains all BodyPartBases.
        /// </summary>
        public List<BodyBase> Bases => _bases;
        
        /// <summary>
        /// The current render side.
        /// </summary>
        public SerializableGUID CurrentSide { get => currentSide; set => currentSide = value; }
        
        /// <summary>
        /// The BodyDefinition used for the Body.
        /// </summary>
        public BodyDefinition Definition { get => _definition; set => _definition = value; }

        /// <summary>
        /// The Mana Stat for the character.
        /// </summary>
        public CharacterStat<float> Mana => _mana;
        
        /// <summary>
        /// The Health Stat for the character. 
        /// </summary>
        public CharacterStat<float> Health => _health;

        public bool CanPickupItems => _canPickupItems;

        /// <summary>
        /// The available character stats that can be overwritten.
        /// </summary>
        public CharacterStats CharacterStats
        {
            get => _characterStats;
            set => _characterStats = value;
        }

        /// <summary>
        /// The available spell stats that can be overwritten.
        /// </summary>
        public SpellStatTraits SpellStats
        {
            get => _spellStats;
            set => _spellStats = value;
        }

        /// <summary>
        /// The Spell data that appends data whenever a spell is casted.
        /// </summary>
        public StatData SpellData => _spellData ??= new StatData();

        /// <summary>
        /// The Character data that is appended to the character.
        /// </summary>
        public StatData CharacterData => _characterData ??= new StatData();
        
        internal SerializableDictionary<SerializableGUID, ObjectsPerSide> Sides => _sides;

        #endregion

        #region Events

        //public event Action OnAfterSetUp;
        //public event Action OnReady;

        public event Action OnDeath;

        #endregion

        #region Unity Methods

        private void OnEnable()
        {
            Equipment.OnEquip += Equipment_OnEquip;
            Equipment.OnUnEquip += Equipment_OnUnEquip;

            _bases = GetComponentsInChildren<BodyBase>(true).ToList();

            if (Application.isPlaying)
            {
                _inventory.ApplyEquippedBuffs(this);

                if (_characterStats == null)
                    throw new NullReferenceException("No Character traits were defined");
                
                
                if (!CharacterData.TryGetAttribute("_maxHealth", out IStatAttribute healthAttr))
                    CharacterData.AddNewAttribute(_characterStats.GetStatTypeByID("_maxHealth"), out healthAttr);
                
                if (!CharacterData.TryGetAttribute("_maxMana", out IStatAttribute manaAttr))
                    CharacterData.AddNewAttribute(_characterStats.GetStatTypeByID("_maxMana"), out manaAttr);

                if (_spellData != null)
                {
                    _spellData.TryGetAttribute("_defenceDamageType", out _defenceDamageTypeAttr);

                    if (_defenceDamageTypeAttr != null)
                    {
                        DefenceDamageTypeOnChange();
                        _defenceDamageTypeAttr.OnChange += DefenceDamageTypeOnChange;
                    }
                }
                
                _health.RegisterStatAttribute((IStatAttribute<float>) healthAttr);
                _mana.RegisterStatAttribute((IStatAttribute<float>) manaAttr);

                
                _health.OnBelowMinimum += Health_OnBelowMinimum;
                
            }
        }
        

        private void OnDisable()
        {
            Equipment.OnEquip -= Equipment_OnEquip;
            Equipment.OnUnEquip -= Equipment_OnUnEquip;

            if (Application.isPlaying)
            {
                _health.OnBelowMinimum -= Health_OnBelowMinimum;

                if (_spellData != null)
                {
                    if (_defenceDamageTypeAttr != null)
                        _defenceDamageTypeAttr.OnChange -= DefenceDamageTypeOnChange;
                }
            }
        }

        #endregion

        #region Event Methods

        private void DefenceDamageTypeOnChange()
        {
            _defenceDamageType = (DamageTypeModifier)(_defenceDamageTypeAttr.Modifiers.LastOrDefault());
        }

        #endregion
        
        #region Equipment Specific Methods

        /// <summary>
        /// Switch the current <see cref="BodySide">side</see> to the target <see cref="BodySide">side</see>.
        /// </summary>
        /// <param name="side"><see cref="BodySide">Side</see> ID</param>
        public void SwitchSide(SerializableGUID side)
        {
            CurrentSide = side;
            foreach (var baseObject in _bases)
                if (baseObject.SideID.Equals(side))
                    baseObject.gameObject.SetActive(true);
                else
                    baseObject.gameObject.SetActive(false);
        }

        private void Equipment_OnEquip(BodyPartFlag slot, SerializableGUID layerID, EquipmentItem equipment)
        {
            foreach (var baseObject in Bases)
            {
                GameObject obj = baseObject.GetSlot(slot)?.gameObject;
                if (obj == null)
                    throw new MissingReferenceException("Slot unassigned!");


                int layerIndex = baseObject.Body.GetLayerIndex(layerID);

                int iterations = 5;

                int sortOrder = -1;
                int index = -1;
                SerializableGUID tempID = layerID;

                do
                {
                    sortOrder = baseObject.Body.RenderOrder.GetSortOrder(baseObject.SideID, tempID, slot.id);
                    index = baseObject.Body.GetLayerIndex(tempID);

                    if (sortOrder >= 0 || index == 0) break;

                    tempID = baseObject.Body.GetLayerByIndex(--index).id;
                    
                } while (iterations-- > 0);

                if (sortOrder >= 0)
                    sortOrder += layerIndex;
                else
                    sortOrder = index;

                GameObject newObj = equipment.GetObjectForSide(baseObject.SideID, sortOrder);
                if (newObj != null)
                {
                    newObj.transform.SetParent(obj.transform, false);


                    if (!Sides.ContainsKey(baseObject.SideID))
                        Sides.Add(baseObject.SideID, new ObjectsPerSide());

                    Sides[baseObject.SideID].Objs.Add($"{slot.id.Value}__{layerID}", newObj);
                }
            }
            

            #if UNITY_EDITOR
            AssetDatabase.SaveAssets();
            #endif
        }

        private void Equipment_OnUnEquip(BodyPartFlag slot, SerializableGUID layerID, EquipmentItem equipment)
        {
            foreach (var side in Sides)
            {
                if (side.Value.Objs.ContainsKey($"{slot.id.Value}__{layerID}"))
                {
                    #if UNITY_EDITOR
                    DestroyImmediate(side.Value.Objs[$"{slot.id.Value}__{layerID}"]);
                    #else
                    Destroy(side.Value.Objs[$"{slot.id.Value}__{layerID}"]);
                    #endif

                    side.Value.Objs.Remove($"{slot.id.Value}__{layerID}");
                }
            }

            #if UNITY_EDITOR
            AssetDatabase.SaveAssets();
            #endif
            
        }

        /// <summary>
        /// Hide equipment by Body Part slot and layer.
        /// </summary>
        /// <param name="slot">The bodyPart slot</param>
        /// <param name="layerID">The <see cref="BodyLayer">layer</see> id</param>
        public void HideEquipment(BodyPartFlag slot, SerializableGUID layerID)
        {
            foreach (var side in Sides)
            {
                if (side.Value.Objs.ContainsKey($"{slot.id.Value}__{layerID}"))
                {
                    side.Value.Objs[$"{slot.id.Value}__{layerID}"].gameObject.SetActive(false);
                }
            }
        }
        
        /// <summary>
        /// Show equipment by Body Part slot and layer.
        /// </summary>
        /// <param name="slot">The bodyPart slot</param>
        /// <param name="layerID">The <see cref="BodyLayer">layer</see> id</param>
        public void ShowEquipment(BodyPartFlag slot, SerializableGUID layerID)
        {
            foreach (var side in Sides)
            {
                if (side.Value.Objs.ContainsKey($"{slot.id.Value}__{layerID}"))
                {
                    side.Value.Objs[$"{slot.id.Value}__{layerID}"].gameObject.SetActive(true);
                }
            }
        }
        

        #endregion

        #region Stat Methods

        private void Health_OnBelowMinimum()
        {
            OnDeath?.Invoke();
        }
        
        /// <summary>
        /// Deal damage to the character.
        /// </summary>
        /// <param name="damage">The damage.</param>
        /// <param name="type">The damage modifier.</param>
        public void DealDamage(float damage, DamageTypeModifier type)
        {
            float modifier = 1;
            
            if (_defenceDamageType != null && type is {Value: { }} && 
                _defenceDamageType.Value.HasValue && 
                type.Definition == _defenceDamageType.Definition)
            {
                modifier = _defenceDamageType.Definition.GetDamageTypeValue(type.Value.Value, _defenceDamageType.Value.Value);
            }
            DealDamage(damage * modifier);
        }
        
        /// <summary>
        /// Deal damage to the character.
        /// </summary>
        /// <param name="damage">The damage.</param>

        public void DealDamage(float damage)
        {
            if (damage <= 0)
                return;

            _health.Value -= damage;
        }

        
        /// <summary>
        /// Heal character.
        /// </summary>
        /// <param name="amount">The amount to be healed.</param>
        public void Heal(float amount)
        {
            if (amount > 0)
                _health.Value += amount;
        }

        #endregion

        #region Spell Methods

        /// <summary>
        /// UsedSpellArchetype adds SpellArchetype XP per use. 
        /// </summary>
        /// <param name="type">The archetype.</param>
        /// <param name="xp">The xp.</param>
        public void UsedSpellArchetype(SpellArchetype type, int xp = 1)
        {
            if (_levels.ContainsKey(type))
                _levels[type] = (_levels[type].Item1, _levels[type].Item2 + xp);
            else
                _levels.Add(type, (0, xp));

            var currentLevel = _levels[type].Item1;
            var currentXp = _levels[type].Item2;

            if (LEVEL_THRESHOLD.Length <= currentLevel + 1) return;
            
            var threshold = LEVEL_THRESHOLD[currentLevel + 1];

            if (currentXp >= threshold)
            {
                _levels[type] = (currentLevel + 1, currentXp);
                GlobalEventSystem.Instance.PlayerArchetypeHasLeveledUp(type, currentLevel + 1);
            }
        }
        
        /// <summary>
        /// Get the current level for a given archetype.
        /// </summary>
        /// <param name="type">The spellArchetype.</param>
        /// <returns>The level.</returns>
        public int GetLevelForSpellArchetype(SpellArchetype type)
        {
            return !_levels.ContainsKey(type) ? 0 : _levels[type].Item1;
        }

        #endregion
    }
}
