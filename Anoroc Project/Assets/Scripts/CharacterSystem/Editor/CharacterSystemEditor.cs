using System.Linq;
using Scripts.BodySystem;
using CombatSystem.SpellSystem;
using EquipmentSystem;
using InventorySystem;
using StatSystem.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Utilities;
using Utilities.UI;

namespace CharacterSystem.Editor
{
    [CustomEditor(typeof(Character))]
    public class CharacterSystemEditor : UnityEditor.Editor
    {
        Character system;
        VisualElement content;
        VisualElement stats;

        private VisualElement spellDataContainer;
        private VisualElement characterDataContainer;

        private Frame _inventory;

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            system = (Character)target;

            stats = new VisualElement();
            stats.style.marginBottom = 15;
            CreateStatAttributes();

            root.Add(stats);

            Frame settings = new Frame() {Label = "Settings"};

            spellDataContainer = new VisualElement();
            characterDataContainer = new VisualElement();


            ObjectField spellTraitsField = new ObjectField("Spell Traits")
            {
                objectType = typeof(SpellStatTraits), 
                value = system.SpellStats
            };
            spellTraitsField.RegisterValueChangedCallback((e) =>
            {
                if (e.previousValue == e.newValue) return;

                system.SpellStats = (SpellStatTraits) e.newValue;
                UpdateSpellData();
            });
            
            
            ObjectField characterStatsField = new ObjectField("Character Traits")
            {
                objectType = typeof(CharacterStats), 
                value = system.CharacterStats
            };
            characterStatsField.RegisterValueChangedCallback((e) =>
            {
                if (e.previousValue == e.newValue) return;

                system.CharacterStats = (CharacterStats) e.newValue;
                UpdateCharacterData();
            });
            
            settings.Add(spellTraitsField);
            settings.Add(characterStatsField);
            
            UpdateSpellData();
            UpdateCharacterData();
            
            settings.Add(spellDataContainer);
            settings.Add(characterDataContainer);
            
            root.Add(settings);

            Frame equipmentFrame = new Frame { Label = "Equipment", IsCollapsed = true};

            ObjectField equipmentSettingsField = new ObjectField("Body Definition") { objectType = typeof(BodyDefinition), value = system.Definition };
            equipmentSettingsField.style.flexGrow = 1;
            equipmentSettingsField.RegisterValueChangedCallback(e=> { system.Definition = (BodyDefinition)e.newValue; CreateContent(); EditorUtility.SetDirty(target); });

            equipmentFrame.Add(equipmentSettingsField);

            content = new VisualElement();
            CreateContent();

            equipmentFrame.Add(content);
            root.Add(equipmentFrame);

            _inventory = new Frame {Label = "Inventory", IsCollapsed = true };
            CreateInventory();
            root.Add(_inventory);

            return root;
        }

        public void CreateContent()
        {
            content.Clear();

            if (system.Definition != null)
            {

                if (!system.Definition.IsValid(out _))
                {
                    content.Add(new HelpBox($"There is an error in The Body Definition Asset [{system.Definition.name}]!", HelpBoxMessageType.Error));
                    return;
                }

                PopupField<BodySide> sideField = new PopupField<BodySide>("Current Side", system.Definition.Sides, 0, e => e.name, e => e.name);
                if (system.CurrentSide.Value.IsEmpty())
                    system.CurrentSide = system.Definition.Sides[0].id;

                sideField.value = system.Definition.GetSide(system.CurrentSide);

                sideField.RegisterValueChangedCallback(e =>
                {
                    system.SwitchSide(e.newValue.id);
                    EditorUtility.SetDirty(target);
                });

                content.Add(sideField);

                ObjectField equipmentChoiceField = new ObjectField { objectType = typeof(EquipmentItem) };
                equipmentChoiceField.style.flexGrow = 1;

                Button addbtn = new Button(() =>
                {
                    if (equipmentChoiceField.value != null)
                    {

                        system.Equipment.Equip((EquipmentItem)equipmentChoiceField.value);
                        EditorUtility.SetDirty(target);
                    }
                })
                { text = "Add" };

                VisualElement row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.flexGrow = 1;

                row.Add(equipmentChoiceField);
                row.Add(addbtn);

                PropertyField equipmentField = new PropertyField(serializedObject.FindProperty("_equipment"));
                equipmentField.Bind(serializedObject);

                //root.Add(baseChoiceField);
                content.Add(row);
                content.Add(equipmentField);
            }
            else
            {
                content.Add(new HelpBox("Body Definition must be set!", HelpBoxMessageType.Warning));
            }
        }

        private void CreateStatAttributes()
        {
            stats.Clear();

            stats.Add(CharacterStatDrawer.CreatePropertyGUI(serializedObject.FindProperty("_health"), "Health", Color.green));
            stats.Add(CharacterStatDrawer.CreatePropertyGUI(serializedObject.FindProperty("_mana"), "Mana", new Color(0,1,1)));
        }

        private void CreateInventory()
        {
            _inventory.Clear();

            if(system.Inventory == null)
                return;

            foreach (var val in Reflection.GetEnumValues<EquipmentSlot>())
            {
                var inventoryEquipment = system.Inventory.Equipment.FirstOrDefault(e => e.Slot == val);
                ObjectField slotField = new ObjectField($"{val} Equipment")
                {
                    objectType = typeof(InventoryEquipment),
                    value = inventoryEquipment
                };
                slotField.RegisterValueChangedCallback(e =>
                {
                    if (e.newValue == inventoryEquipment)
                        return;
                    
                    if (e.newValue == null)
                    {
                        if(inventoryEquipment != null)
                            inventoryEquipment.Use(system.Inventory, system);
                        
                        inventoryEquipment = system.Inventory.Equipment.FirstOrDefault(b => b.Slot == val);
                        EditorUtility.SetDirty(target);
                        return;
                    }
                    
                    InventoryEquipment newEquipment;
                    if (typeof(InventoryEquipment) != e.newValue.GetType() ||
                        !val.Equals((newEquipment = (InventoryEquipment) e.newValue).Slot))
                    {
                        slotField.SetValueWithoutNotify(inventoryEquipment);
                        return;
                    }

                    newEquipment.Use(system.Inventory, system);
                    inventoryEquipment = system.Inventory.Equipment.FirstOrDefault(b => b.Slot == val);

                    EditorUtility.SetDirty(target);
                });
                
                _inventory.Add(slotField);
            }

            BoundListView<InventoryObject> objectsInnerView = new BoundListView<InventoryObject>(serializedObject.FindProperty("_inventory").FindPropertyRelative("_objects"))
            {
                CanChoose = false,
                CanCopy = true,
                CanMove = true,
                CanReorder = true,
                CanDelete = true,
                CanDuplicate = false
            };
            objectsInnerView.style.height = 100;
            objectsInnerView.style.maxHeight = 200;
            
            objectsInnerView.AddNewItem = (e) => { system.Inventory.Pickup(e); return true; };
            objectsInnerView.DeleteItem = (e) => { system.Inventory.Delete(e); return true; };

            BoundList<InventoryObject> objectsView = new BoundList<InventoryObject>(objectsInnerView) {Label = "Objects", IsCollapsed = true };
            ObjectField toAddField = new ObjectField {objectType = typeof(InventoryObject), allowSceneObjects = false};

            objectsView.CreateNewItem = () => (InventoryObject) toAddField.value;
            
            objectsView.Header.Add(toAddField);
            
            _inventory.Add(objectsView);
        }

        private void UpdateSpellData()
        {
            spellDataContainer.Clear();
            if (system.SpellStats == null)
            {
                spellDataContainer.Add(new HelpBox("Cannot have Spell data, no Spell traits have been defined!", HelpBoxMessageType.Warning));
            }
            else
            {
                var element = StatDataDrawer.CreateGUI(system.SpellData, serializedObject, system.SpellStats,
                    () =>
                    {
                        EditorUtility.SetDirty(target);
                    });
                element.IsCollapsed = true;
                
                spellDataContainer.Add(element);
            }
        }

        private void UpdateCharacterData()
        {
            characterDataContainer.Clear();
            if (system.CharacterStats == null)
            {
                characterDataContainer.Add(new HelpBox("Cannot have Character data, no Character traits have been defined!", HelpBoxMessageType.Warning));
            }
            else
            {
                var element = StatDataDrawer.CreateGUI(system.CharacterData, serializedObject, system.CharacterStats,
                    () =>
                    {
                        EditorUtility.SetDirty(target);
                    });
                element.IsCollapsed = true;
                characterDataContainer.Add(element);
            }   
        }
    }
}
