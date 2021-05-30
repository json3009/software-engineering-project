using System;
using System.Collections.Generic;
using System.Linq;
using CombatSystem.SpellSystem;
using CombatSystem.SpellSystem.Attributes;
using Scripts.CombatSystem;
using StatSystem;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Utilities;

namespace CombatSystem.Editor.SpellSystem
{
    [CustomEditor(typeof(CombatSystem.SpellSystem.SpellSystem))]
    public class SpellSystemEditor : UnityEditor.Editor
    {
        private static class HelpBoxes
        {
            public static readonly HelpBox noTraitsAlert = new HelpBox("Traits Asset must be created!", HelpBoxMessageType.Error);
            public static readonly HelpBox noDamageDefinition = new HelpBox("Damage Definition Asset must be assigned!", HelpBoxMessageType.Error);
        }

        private CombatSystem.SpellSystem.SpellSystem spellSystem;

        private Frame generalSettings;
        private Frame traitsFrame;


        public override VisualElement CreateInspectorGUI()
        {
            spellSystem = target as CombatSystem.SpellSystem.SpellSystem;
            VisualElement root = new VisualElement();

            generalSettings = new Frame() { Label = "General Settings" };

            CreateGeneralSettings();

            root.Add(generalSettings);

            traitsFrame = new Frame() { Label = "Stat Types" };

            Button refreshStatTypes = new Button(() =>
            { 
                ResetTraits();

                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();

                CreateOrUpdate_TraitsFrame();
            })
            { text = "Refresh" };
            
            Button resetStatTypes = new Button(() =>
            {
                AssetDatabase.RemoveObjectFromAsset(spellSystem.Traits);

                CreateNewTraitsAsset();
                
                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
                
                CreateOrUpdate_TraitsFrame();
            })
            { text = "Reset" };

            traitsFrame.Header.Add(refreshStatTypes);
            traitsFrame.Header.Add(resetStatTypes);
            
            CreateOrUpdate_TraitsFrame();

            root.Add(traitsFrame);

            return root;
        }

        private void ResetTraits()
        {
            HashSet<StatType> statTypes = new HashSet<StatType>();
            var registeredArchetypes = Reflection.GetAllTypes<SpellBehaviour>();

            foreach (var archetype in registeredArchetypes)
            {
                var fields = archetype
                    .GetAllFields(
                        System.Reflection.BindingFlags.Static |
                        System.Reflection.BindingFlags.Public |
                        System.Reflection.BindingFlags.NonPublic)
                    .Where(field => field.IsDefined(typeof(SpellBehaviourStatAttribute), false));

                foreach (var field in fields)
                {
                    if (typeof(IList<StatType>).IsAssignableFrom(field.FieldType))
                        foreach (var item in ((IList<StatType>)field.GetValue(null)))
                            statTypes.Add(item);
                    else if (typeof(StatType) == field.FieldType)
                        statTypes.Add((StatType)field.GetValue(null));
                    else
                        throw new InvalidCastException($"Field [{field.Name}] designated as " +
                            $"({nameof(SpellBehaviourStatAttribute)}) must either be of type " +
                            $"[{nameof(StatType)}] or " +
                            $"[IList<{nameof(StatType)}>]!");
                }
            }

            spellSystem.Traits.UpdateTraits(statTypes.ToArray());
        }

        private void CreateGeneralSettings()
        {
            generalSettings.Clear();

            if (spellSystem.DamageDefinition == null)
                generalSettings.Add(HelpBoxes.noDamageDefinition);

            ObjectField damageDefinitionField = new ObjectField("Damage Definition")
            {
                allowSceneObjects = false, 
                objectType = typeof(DamageDefinition), bindingPath = "_damageDefinition"
            };
            damageDefinitionField.Bind(serializedObject);

            damageDefinitionField.RegisterValueChangedCallback((e) =>
            {
                if (e.newValue == null)
                    generalSettings.Add(HelpBoxes.noDamageDefinition);
                else if (generalSettings.Contains(HelpBoxes.noDamageDefinition))
                    generalSettings.Remove(HelpBoxes.noDamageDefinition);
            });

            generalSettings.Add(damageDefinitionField);
        }

        private void CreateOrUpdate_TraitsFrame()
        {
            traitsFrame.Clear();
            if (spellSystem.Traits == null)
            {
                HelpBox noTraitsAlert = new HelpBox("Traits Asset must be created!", HelpBoxMessageType.Error);
                Button createBTN = new Button(() =>
                {
                    CreateNewTraitsAsset();

                    CreateOrUpdate_TraitsFrame();
                })
                { text = "Create new Traits Asset" };

                traitsFrame.Add(noTraitsAlert);
                traitsFrame.Add(createBTN);
            }
            else
            {
                UnityEditor.Editor data = UnityEditor.Editor.CreateEditor(spellSystem.Traits);
                var content = data.CreateInspectorGUI();

                if (content != null && content.childCount > 0)
                    traitsFrame.Add(content);
                else
                    traitsFrame.Add(new IMGUIContainer(() => { data.OnInspectorGUI(); }));
            }
        }

        private void CreateNewTraitsAsset()
        {
            var instance = ScriptableObject.CreateInstance<SpellStatTraits>();
            instance.name = "Traits";
            spellSystem.Traits = instance;
            AssetDatabase.AddObjectToAsset(instance, spellSystem);
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }
    }
}
