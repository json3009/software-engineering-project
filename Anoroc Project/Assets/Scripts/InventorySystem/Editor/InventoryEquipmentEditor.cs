using CharacterSystem;
using CombatSystem.SpellSystem;
using StatSystem;
using StatSystem.Editor;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace InventorySystem.Editor
{
    [CustomEditor(typeof(InventoryEquipment))]
    public class InventoryEquipmentEditor : UnityEditor.Editor
    {
        private InventoryEquipment inventoryEquipment;
        private VisualElement spellTraitsInnerView;
        
        private VisualElement spellDataContainer;
        private VisualElement characterDataContainer;
        
        public override void OnInspectorGUI()
        {
            CreateInspectorGUI();
        }

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            inventoryEquipment = (InventoryEquipment) target;

            Frame baseSettings = new Frame() {Label = "Settings"};

            PropertyField dropPrefab = new PropertyField(serializedObject.FindProperty("_dropPrefab"));
            dropPrefab.Bind(serializedObject);
            
            PropertyField isDroppable = new PropertyField(serializedObject.FindProperty("_isDroppable"));
            isDroppable.Bind(serializedObject);
            
            PropertyField isStackable = new PropertyField(serializedObject.FindProperty("_isStackable"));
            isStackable.Bind(serializedObject);
            
            PropertyField maxStack = new PropertyField(serializedObject.FindProperty("_maxStack"));
            maxStack.Bind(serializedObject);
            
            PropertyField item = new PropertyField(serializedObject.FindProperty("_item"));
            item.Bind(serializedObject);
            
            PropertyField slot = new PropertyField(serializedObject.FindProperty("_slot"));
            slot.Bind(serializedObject);

            
            baseSettings.Add(dropPrefab);
            baseSettings.Add(isDroppable);
            baseSettings.Add(isStackable);
            baseSettings.Add(maxStack);
            baseSettings.Add(item);
            baseSettings.Add(slot);
            
            root.Add(baseSettings);

            
            Frame buffs = new Frame() {Label = "Buffs"};

            spellDataContainer = new VisualElement();
            characterDataContainer = new VisualElement();
            
            ObjectField spellTraitsField = new ObjectField("Spell Traits")
            {
                objectType = typeof(SpellStatTraits), 
                value = inventoryEquipment.SpellTraits
            };
            spellTraitsField.RegisterValueChangedCallback((e) =>
            {
                if (e.previousValue == e.newValue) return;

                inventoryEquipment.SpellTraits = (SpellStatTraits) e.newValue;
                UpdateSpellData();
            });
            
            
            ObjectField characterStatsField = new ObjectField("Character Traits")
            {
                objectType = typeof(CharacterStats), 
                value = inventoryEquipment.CharacterTraits
            };
            characterStatsField.RegisterValueChangedCallback((e) =>
            {
                if (e.previousValue == e.newValue) return;

                inventoryEquipment.CharacterTraits = (CharacterStats) e.newValue;
                UpdateCharacterData();
            });
            
            UpdateSpellData();
            UpdateCharacterData();
            
            buffs.Add(spellTraitsField);
            buffs.Add(characterStatsField);
            buffs.Add(spellDataContainer);
            buffs.Add(characterDataContainer);
            
            root.Add(buffs);

            return root;
        }

        private void UpdateCharacterData()
        {
            characterDataContainer.Clear();
            if (inventoryEquipment.CharacterTraits == null)
            {
                characterDataContainer.Add(new HelpBox("Cannot have Character data, no Character traits have been defined!", HelpBoxMessageType.Warning));
            }
            else
            {
                var element = StatDataDrawer.CreateGUI(inventoryEquipment.CharacterData, serializedObject, inventoryEquipment.CharacterTraits,
                    () =>
                    {
                        EditorUtility.SetDirty(target);
                    });
                element.IsCollapsed = true;
                characterDataContainer.Add(element);
            }   
        }

        private void UpdateSpellData()
        {
            spellDataContainer.Clear();
            if (inventoryEquipment.SpellTraits == null)
            {
                spellDataContainer.Add(new HelpBox("Cannot have Spell data, no Spell traits have been defined!", HelpBoxMessageType.Warning));
            }
            else
            {
                var element = StatDataDrawer.CreateGUI(inventoryEquipment.SpellData, serializedObject, inventoryEquipment.SpellTraits,
                    () =>
                    {
                        EditorUtility.SetDirty(target);
                    });
                element.IsCollapsed = true;
                
                spellDataContainer.Add(element);
            }
        }

        private void UpdateSpellTraitsFrame()
        {
            spellTraitsInnerView.Clear();

            if (inventoryEquipment.SpellTraits == null)
            {
                spellTraitsInnerView.Add(new HelpBox("Cannot add Spell buffs! SpellTraits asset missing!", HelpBoxMessageType.Warning));
            }
            else
            {
                var spellDataView = StatDataDrawer.CreateGUI(inventoryEquipment.SpellData, serializedObject, inventoryEquipment.SpellTraits, ONChange);

                spellTraitsInnerView.Add(spellDataView);
            }
        }

        private void ONChange()
        {
            EditorUtility.SetDirty(target);
        }
    }
}
