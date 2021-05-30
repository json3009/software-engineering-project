using CombatSystem.SpellSystem;
using StatSystem;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CombatSystem.Editor.SpellSystem
{
    [CustomEditor(typeof(SpellBehaviour), true)]
    public class SpellBehaviourEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            SpellBehaviour spellBehaviour = (SpellBehaviour)target;

            VisualElement root = new VisualElement();
            Frame baseSettings = new Frame() { Label = "Base Settings" };

            IntegerField levelField = new IntegerField("Level") { value = spellBehaviour.Level };
            levelField.RegisterValueChangedCallback((e) => { spellBehaviour.Level = e.newValue; EditorUtility.SetDirty(target); });
            baseSettings.Add(levelField);

            ObjectField mainTypeField = new ObjectField() { label = "Main Type", objectType = typeof(SpellArchetype), value = spellBehaviour.MainType };
            mainTypeField.RegisterValueChangedCallback((e) => { spellBehaviour.MainType = (SpellArchetype)e.newValue; EditorUtility.SetDirty(target); });
            baseSettings.Add(mainTypeField);

            ObjectField spellVisualField = new ObjectField() { label = "Visuals", objectType = typeof(GameObject), value = spellBehaviour.PrefabVisual, allowSceneObjects = false };
            spellVisualField.RegisterValueChangedCallback((e) => { spellBehaviour.PrefabVisual = (GameObject)e.newValue; EditorUtility.SetDirty(target); });
            baseSettings.Add(spellVisualField);

            root.Add(baseSettings);
            
            Frame modifiersFrame = CreateModifiersFrame(spellBehaviour.CombinedData, spellBehaviour.MainType);
            modifiersFrame.Label = "Combined Modifiers";

            Frame inputFrame = CreateModifiersFrame(spellBehaviour.DataInput, spellBehaviour.MainType);
            inputFrame.Label = "Input Modifiers";

            root.Add(inputFrame);
            root.Add(modifiersFrame);
            
            return root;
        }

        private static Frame CreateModifiersFrame(StatData data, SpellArchetype main)
        {
            Frame inputFrame = new Frame();

            if (data != null && main != null)
            {
                foreach (var attr in data.Modifiers)
                {
                    VisualElement attrRow = new VisualElement();
                    attrRow.style.flexDirection = FlexDirection.Row;

                    Label name = new Label($"{attr.Type.Name}: ");
                    name.style.unityTextAlign = TextAnchor.MiddleRight;
                    name.style.fontSize = 14;
                    name.style.width = new StyleLength(new Length(50, LengthUnit.Percent));

                    Label value = new Label(attr.Attribute.Value?.ToString() ?? "NO VALUE");
                    value.style.unityTextAlign = TextAnchor.MiddleLeft;

                    if(attr.Attribute.Value == null)
                        value.style.color = Color.red;
                    else
                        value.style.color = Color.green;

                    value.style.fontSize = 14;
                    value.style.flexGrow = 1;

                    if (attr.Attribute.Modifiers.Length > 0)
                    {
                        string appliedMods = "Applied Modifiers:";
                        foreach (var appliedMod in attr.Attribute.Modifiers)
                        {
                            appliedMods += $"\n{appliedMod}";
                        }
                        attrRow.tooltip = appliedMods;
                    }
                    else
                    {
                        attrRow.tooltip = "No Modifiers applied";
                    }

                    attrRow.style.borderBottomColor = Color.gray;
                    attrRow.style.borderBottomWidth = 1;

                    attrRow.Add(name);
                    attrRow.Add(value);

                    inputFrame.Add(attrRow);
                }
            }

            return inputFrame;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }
    }
}
