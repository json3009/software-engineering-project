using System;
using System.Collections.Generic;
using System.Linq;
using CombatSystem.SpellSystem;
using CombatSystem.SpellSystem.Attributes;
using StatSystem;
using StatSystem.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Utilities;
using Utilities.UI;

namespace CombatSystem.Editor.SpellSystem
{
    [CustomEditor(typeof(SpellArchetype), true)]
    public class SpellArchetypeEditor : UnityEditor.Editor
    {
        private static class HelpBoxes
        {
            public static readonly HelpBox noSystemSelected = new HelpBox("Spell System must be defined!", HelpBoxMessageType.Error);
            public static readonly HelpBox noBehaviourSelected = new HelpBox("Spell Behaviour must be defined!", HelpBoxMessageType.Error);
            public static readonly HelpBox noBehaviourVariables = new HelpBox("Spell Behaviour has no definable settings!", HelpBoxMessageType.Info);
            public static readonly HelpBox noVisualSelected = new HelpBox("Spell Visuals must be defined!", HelpBoxMessageType.Error);
            public static readonly HelpBox noVisualScriptAssociated = new HelpBox("Spell Visual Prefab does not contain any Visual Script!", HelpBoxMessageType.Info);
            public static readonly HelpBox noVisualVariables = new HelpBox("Spell Visual has no definable settings!", HelpBoxMessageType.Info);
        }

        SpellArchetype spellArchetype;

        BoundList<SpellArchetypeLevelData> LevelView;
        Frame behaviourSettingsFrame;
        Frame visualSettingsFrame;

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            spellArchetype = (SpellArchetype)target;

            Frame baseSettingsFrame = new Frame() { Label = "Base Settings" };

            ObjectField systemField = new ObjectField("System") { objectType = typeof(CombatSystem.SpellSystem.SpellSystem), value = spellArchetype.System };
            systemField.RegisterValueChangedCallback((e) =>
            {
                if (e.newValue == null)
                    baseSettingsFrame.Add(HelpBoxes.noSystemSelected);
                else
                    baseSettingsFrame.Remove(HelpBoxes.noSystemSelected);

                if (spellArchetype.System != null)
                    EditorUtility.SetDirty(spellArchetype.System);

                spellArchetype.System = (CombatSystem.SpellSystem.SpellSystem)e.newValue;

                if (spellArchetype.System != null)
                    EditorUtility.SetDirty(spellArchetype.System);

                LevelView.SetEnabled(spellArchetype.System != null);
            });

            baseSettingsFrame.Add(systemField);

            TextField nameField = new TextField("Name") { value = spellArchetype.Name };
            nameField.RegisterValueChangedCallback((e) => { spellArchetype.Name = e.newValue; EditorUtility.SetDirty(target); });

            TextField descField = new TextField("Description") { multiline = true, maxLength = 300, value = spellArchetype.Description };
            descField.RegisterValueChangedCallback((e) => { spellArchetype.Description = e.newValue; EditorUtility.SetDirty(target); });
            descField.style.whiteSpace = WhiteSpace.Normal;
            descField.style.height = 100;

            List<Type> choices = GetBehaviourTypes();
            PopupField<Type> behavioursField = new PopupField<Type>("Behaviour", choices, 0, (e) => ObjectNames.NicifyVariableName(e.Name), (e) => ObjectNames.NicifyVariableName(e.Name));
            behavioursField.RegisterValueChangedCallback((e) => { spellArchetype.Behaviour = e.newValue; spellArchetype.BehaviourModifiers.Clear(); UpdateBehaviourFrame(); EditorUtility.SetDirty(target); });
            if (spellArchetype.Behaviour == null)
                spellArchetype.Behaviour = choices.FirstOrDefault();

            behavioursField.value = spellArchetype.Behaviour;

            ObjectField spellVisualField = new ObjectField() { label = "Visuals", objectType = typeof(GameObject), value = spellArchetype.PrefabVisual, allowSceneObjects = false };
            spellVisualField.RegisterValueChangedCallback((e) => { spellArchetype.PrefabVisual = (GameObject)e.newValue; EditorUtility.SetDirty(target); });

            baseSettingsFrame.Add(behavioursField);
            baseSettingsFrame.Add(spellVisualField);
            baseSettingsFrame.Add(nameField);
            baseSettingsFrame.Add(descField);

            if (spellArchetype.System == null)
                baseSettingsFrame.Add(HelpBoxes.noSystemSelected);

            root.Add(baseSettingsFrame);

            behaviourSettingsFrame = new Frame() { Label = "Behaviour Settings" };

            UpdateBehaviourFrame();

            root.Add(behaviourSettingsFrame);

            visualSettingsFrame = new Frame() { Label = "Visual Settings" };

            UpdateVisualFrame();

            root.Add(visualSettingsFrame);


            BoundListView<SpellArchetypeLevelData> innerView = new BoundListView<SpellArchetypeLevelData>(spellArchetype.Level.Levels, NewLevelRow)
            {
                CanCopy = false,
                CanChoose = false,
                CanDelete = true,
                CanDuplicate = false,
                CanLink = false,
                CanMove = false,
                CanReorder = false
            };
            innerView.AddNewItem = (e) => { spellArchetype.Level.Levels.Add(e); spellArchetype.Level.Levels.Sort(); return true; };
            innerView.DeleteItemByIndex = (e) => { spellArchetype.Level.Levels.RemoveAt(e); spellArchetype.Level.Levels.Sort(); return true; };
            innerView.OnChange += (e) => { spellArchetype.Level.RecalculateCache(); EditorUtility.SetDirty(target); };

            LevelView = new BoundList<SpellArchetypeLevelData>(innerView) { Label = "Levels", IsCollapsed = spellArchetype.System == null };

            LevelView.CreateNewItem = () =>
            {
                return new SpellArchetypeLevelData() { Level = 0 };
            };
            LevelView.SetEnabled(spellArchetype.System != null);

            root.Add(LevelView);



            // Draw Recap of Levels
            Frame overviewLevels = new Frame() { Label = "Overview", IsCollapsed = true };
            overviewLevels.style.marginTop = 10;

            SpellArchetypeLevelData maxLevelData = spellArchetype.Level.GetLevelData(int.MaxValue);

            foreach (var mod in maxLevelData.Modifiers.Modifiers)
            {
                Frame modFrame = new Frame() { Label = mod.Type.Name, IsCollapsed = true };

                foreach (var level in spellArchetype.Level.CachedLevels)
                {
                    VisualElement row = new VisualElement();
                    row.style.flexDirection = FlexDirection.Row;

                    Label levelLabel = new Label($"Level {level.Level}");
                    levelLabel.style.width = 100;
                    levelLabel.style.unityTextAlign = TextAnchor.MiddleRight;
                    levelLabel.style.fontSize = 15;

                    Label increaseLabel = new Label($"{(level.Modifiers.TryGetAttribute(mod.Type.ID, out IStatAttribute attribute) ? attribute?.Value?.ToString() ?? "NO VALUE" : "/")}");
                    increaseLabel.style.flexGrow = 1;
                    increaseLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                    if (attribute != null)
                        increaseLabel.style.backgroundColor = new Color(0, 1, 0.21f, 0.35f);
                    if(attribute.Value == null)
                        increaseLabel.style.backgroundColor = new Color(1, 0, 0, 0.35f);

                    increaseLabel.style.fontSize = 15;

                    if (attribute != null)
                    {
                        if (attribute.Modifiers.Length > 0)
                        {
                            string appliedMods = "Applied Modifiers:";
                            foreach (var appliedMod in attribute.Modifiers)
                            {
                                appliedMods += $"\n{appliedMod}";
                            }
                            increaseLabel.tooltip = appliedMods;
                        }
                        else
                        {
                            increaseLabel.tooltip = "No Modifiers applied";
                        }
                    }


                    row.style.borderBottomColor = Color.gray;
                    row.style.borderBottomWidth = 1;

                    row.Add(levelLabel);
                    row.Add(increaseLabel);

                    modFrame.Add(row);
                }

                overviewLevels.Add(modFrame);
            }

            //SpellArchetypeLevelData maxLevelData = spellArchetype.Level.GetLevelData(int.MaxValue);

            //Dictionary<StatType, Dictionary<int,DataWrapper>> modifiers = new Dictionary<StatType, Dictionary<int, DataWrapper>> ();

            /*foreach (var mod in maxLevelData.Modifiers.Modifiers)
            {

            }*/

            LevelView.contentContainer.Add(overviewLevels);

            //var t = SerializationUtility.SerializeValue(spellArchetype.Levels, DataFormat.JSON);
            //Debug.Log(System.Text.Encoding.UTF8.GetString(t));

            return root;
        }

        private void UpdateBehaviourFrame()
        {
            behaviourSettingsFrame.Clear();

            if (spellArchetype.Behaviour == null)
            {
                behaviourSettingsFrame.Add(HelpBoxes.noBehaviourSelected);
            }
            else
            {
                var fields = spellArchetype.Behaviour
                    .GetAllFields()
                    .Where(field => field.IsDefined(typeof(SpellBehaviourInputAttribute), false));

                if (fields.Count() > 0)
                {
                    foreach (var field in fields)
                    {
                        if (!typeof(IStatModifier).IsAssignableFrom(field.FieldType))
                        {
                            behaviourSettingsFrame.Add(new HelpBox($"Field [{field.Name}] is not assignable of type IStatModifier, field is currently of type [{field.FieldType.Name}]", HelpBoxMessageType.Error));
                        }
                        else
                        {
                            VisualElement row = new VisualElement();
                            row.style.flexDirection = FlexDirection.Row;
                            row.AddToClassList("unity-base-field");

                            Label nameField = new Label($"{ObjectNames.NicifyVariableName(field.Name)}");
                            nameField.AddToClassList("unity-base-field__label");
                            row.Add(nameField);

                            if (!spellArchetype.BehaviourModifiers.TryGetValue(field.Name, out IStatModifier mod))
                            {
                                mod = (IStatModifier)Activator.CreateInstance(field.FieldType);
                                spellArchetype.BehaviourModifiers.Add(field.Name, mod);
                            }

                            VisualElement valueField = mod.CreateGUI(() =>
                            {
                                if (spellArchetype.BehaviourModifiers.ContainsKey(field.Name))
                                    spellArchetype.BehaviourModifiers[field.Name] = mod;
                                else
                                    spellArchetype.BehaviourModifiers.Add(field.Name, mod);

                                EditorUtility.SetDirty(target);
                            });
                            valueField.style.flexGrow = 1;
                            row.Add(valueField);

                            behaviourSettingsFrame.Add(row);
                        }
                    }
                }
                else
                {
                    behaviourSettingsFrame.Add(HelpBoxes.noBehaviourVariables);
                }
            }
        }

        private void UpdateVisualFrame()
        {
            visualSettingsFrame.Clear();



            if (spellArchetype.PrefabVisual == null)
            {
                visualSettingsFrame.Add(HelpBoxes.noVisualSelected);
            }else if (spellArchetype.PrefabVisual.GetComponent<SpellVisual>() == null)
            {
                visualSettingsFrame.Add(HelpBoxes.noVisualScriptAssociated);
            }
            else
            {
                var fields = spellArchetype.PrefabVisual
                    .GetComponent<SpellVisual>()
                    .GetType()
                    .GetAllFields()
                    .Where(field => field.IsDefined(typeof(SpellVisualInputAttribute), false));

                //Debug.Log(fields.Count());

                if (fields.Any())
                {
                    foreach (var field in fields)
                    {
                        if (!typeof(IStatModifier).IsAssignableFrom(field.FieldType))
                        {
                            visualSettingsFrame.Add(new HelpBox($"Field [{field.Name}] is not assignable of type IStatModifier, field is currently of type [{field.FieldType.Name}]", HelpBoxMessageType.Error));
                        }
                        else
                        {
                            VisualElement row = new VisualElement();
                            row.style.flexDirection = FlexDirection.Row;
                            row.AddToClassList("unity-base-field");

                            Label nameField = new Label($"{ObjectNames.NicifyVariableName(field.Name)}");
                            nameField.AddToClassList("unity-base-field__label");
                            row.Add(nameField);

                            if (!spellArchetype.VisualModifiers.TryGetValue(field.Name, out IStatModifier mod))
                            {
                                mod = (IStatModifier)Activator.CreateInstance(field.FieldType);
                                spellArchetype.VisualModifiers.Add(field.Name, mod);
                            }

                            if(mod == null)
                                continue;
                            
                            VisualElement valueField = mod.CreateGUI(() =>
                            {
                                if (spellArchetype.VisualModifiers.ContainsKey(field.Name))
                                    spellArchetype.VisualModifiers[field.Name] = mod;
                                else
                                    spellArchetype.VisualModifiers.Add(field.Name, mod);

                                EditorUtility.SetDirty(target);
                            });
                            valueField.style.flexGrow = 1;
                            row.Add(valueField);

                            visualSettingsFrame.Add(row);
                        }
                    }
                }
                else
                {
                    visualSettingsFrame.Add(HelpBoxes.noVisualVariables);
                }
            }
        }

        private VisualElement NewLevelRow(SpellArchetypeLevelData item, int index, SerializedProperty prop)
        {
            Frame root = new Frame() { Label = $"Level: {item.Level}{(string.IsNullOrEmpty(item.Title) ? "" : $" - {item.Title}")}", IsCollapsed = true };

            IntegerField levelField = new IntegerField("Level") { value = item.Level };
            levelField.RegisterValueChangedCallback((e) => { item.Level = e.newValue; spellArchetype.Level.Levels.Sort(); spellArchetype.Level.RecalculateCache(); EditorUtility.SetDirty(target); });

            TextField titleField = new TextField("Name") { value = item.Title };
            titleField.RegisterValueChangedCallback((e) => { item.Title = e.newValue; EditorUtility.SetDirty(target); });

            TextField descField = new TextField("Description") { multiline = true, maxLength = 200, value = item.Description };
            descField.RegisterValueChangedCallback((e) => { item.Description = e.newValue; EditorUtility.SetDirty(target); });
            descField.style.whiteSpace = WhiteSpace.Normal;
            descField.style.height = 100;

            root.Add(levelField);
            root.Add(titleField);
            root.Add(descField);


            BoundList<StatData.DataWrapper> boundList = StatDataDrawer.CreateGUI(item.Modifiers, serializedObject, spellArchetype.System.Traits, () =>
            {
                spellArchetype.Level.RecalculateCache();
            });
            boundList.IsCollapsed = true;

            root.Add(boundList);

            return root;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }

        private List<Type> GetBehaviourTypes()
        {
            return Reflection.GetAllTypes<SpellBehaviour>()
                .Where((e) => !e.Equals(typeof(SpellBehaviour)))
                .ToList();
        }
    }
}
