using System;
using System.Collections.Generic;
using System.Text;
using Scripts.CombatSystem;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Utilities;
using Utilities.UI;

namespace CombatSystem.Editor
{

    [CustomEditor(typeof(DamageDefinition))]
    public class DamageDefinitionEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            DamageDefinition def = target as DamageDefinition;

            Frame damageTypesListContainer = new Frame() { Label = "Damage Types" };

            BoundListView<DamageType> damageTypeListView = new BoundListView<DamageType>(serializedObject.FindProperty("_types"), newDamageTypeUIElement);
            BoundList<DamageType> damageTypeView = new BoundList<DamageType>(damageTypeListView) { Label = "Types" };
            damageTypeView.CreateNewItem = () => new DamageType() { ID = Guid.NewGuid(), Name = "New Damage Type" };
            damageTypeListView.style.maxHeight = 200;

            damageTypesListContainer.Add(damageTypeView);
            root.Add(damageTypesListContainer);

            Frame matrixDamage = new Frame() { Label = "Damage Matrix" };
            matrixDamage.contentContainer.style.flexDirection = FlexDirection.Row;

            Dictionary<SerializableGUID, float> attackerEffective = new Dictionary<SerializableGUID, float>();
            Dictionary<SerializableGUID, float> defenderEffective = new Dictionary<SerializableGUID, float>();

            for (int column = -1; column < def.Types.Count; column++)
            {
                VisualElement columnElement = new VisualElement();
                columnElement.style.flexDirection = FlexDirection.Column;
                columnElement.style.flexGrow = 1;

                if (column > -1)
                {
                    Label columnLabel = new Label(def.Types[column].Name);
                    columnLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                    columnElement.Add(columnLabel);

                    for (int row = 0; row < def.Types.Count; row++)
                    {
                        SerializableGUID attackerId = def.Types[row].ID;
                        SerializableGUID defenderId = def.Types[column].ID;

                        float val = def.GetDamageTypeValue(attackerId, defenderId);

                        if (attackerEffective.ContainsKey(attackerId))
                            attackerEffective[attackerId] += val;
                        else
                            attackerEffective.Add(attackerId, val);

                        if (defenderEffective.ContainsKey(defenderId))
                            defenderEffective[defenderId] += val;
                        else
                            defenderEffective.Add(defenderId, val);

                        FloatField damageField = new FloatField() { value = val };
                        damageField.RegisterValueChangedCallback((e) => { def.SetDamageTypeValue(attackerId, defenderId, e.newValue); EditorUtility.SetDirty(target); });
                        columnElement.Add(damageField);
                    }
                }
                else
                {
                    columnElement.style.flexGrow = 0;

                    VisualElement l = new VisualElement();
                    l.style.height = 16;
                    columnElement.Add(l);

                    for (int row = 0; row < def.Types.Count; row++)
                    {
                        Label rowLabel = new Label(def.Types[row].Name);
                        rowLabel.style.height = 21;
                        rowLabel.style.unityTextAlign = TextAnchor.MiddleRight;

                        columnElement.Add(rowLabel);
                    }
                }
                matrixDamage.Add(columnElement);
            }

            Frame overviewDamageEffective = new Frame() { Label = "Effectives", IsCollapsable = false };
            overviewDamageEffective.contentContainer.style.flexDirection = FlexDirection.Row;

            VisualElement UILabelsEffective = new VisualElement();
            VisualElement UIattackersEffective = new VisualElement();
            UIattackersEffective.style.backgroundColor = new Color(1, 0, 0, 0.3f);
            UIattackersEffective.style.unityFontStyleAndWeight = FontStyle.BoldAndItalic;
            UIattackersEffective.style.flexGrow = 1;
            VisualElement UIdefendersEffective = new VisualElement();
            UIdefendersEffective.style.backgroundColor = new Color(0, 0.43f, 1, 0.35f);
            UIdefendersEffective.style.unityFontStyleAndWeight = FontStyle.BoldAndItalic;
            UIdefendersEffective.style.flexGrow = 1;
            VisualElement UIEffective = new VisualElement();
            UIEffective.style.backgroundColor = new Color(0, 1, 0.21f, 0.35f);
            UIEffective.style.unityFontStyleAndWeight = FontStyle.BoldAndItalic;
            UIEffective.style.flexGrow = 1;

            VisualElement l1 = new VisualElement();
            l1.style.height = 21;
            UILabelsEffective.Add(l1);

            Label l2 = new Label("Defence");
            l2.style.height = 21;
            l2.style.unityTextAlign = TextAnchor.MiddleCenter;
            UIdefendersEffective.Add(l2);

            Label l3 = new Label("Attack");
            l3.style.height = 21;
            l3.style.unityTextAlign = TextAnchor.MiddleCenter;
            UIattackersEffective.Add(l3);

            Label l4 = new Label("Effective");
            l4.style.height = 21;
            l4.style.unityTextAlign = TextAnchor.MiddleCenter;
            UIEffective.Add(l4);

            overviewDamageEffective.Add(UILabelsEffective);
            overviewDamageEffective.Add(UIattackersEffective);
            overviewDamageEffective.Add(UIdefendersEffective);
            overviewDamageEffective.Add(UIEffective);

            // Effectifs
            foreach (var item in def.Types)
            {
                // labels
                Label l = new Label(item.Name);
                l.style.height = 21;
                l.style.unityTextAlign = TextAnchor.MiddleRight;
                UILabelsEffective.Add(l);

                // attack effective
                float attacker = attackerEffective?[item.ID] ?? 0;
                FloatField attackEffective = new FloatField() { isReadOnly = true, value = attacker };
                UIattackersEffective.Add(attackEffective);

                // defend effective
                float defender = defenderEffective?[item.ID] ?? 0;
                FloatField defendEffective = new FloatField() { isReadOnly = true, value = defender };
                UIdefendersEffective.Add(defendEffective);

                // defend effective
                FloatField effective = new FloatField() { isReadOnly = true, value = attacker - defender };
                UIEffective.Add(effective);
            }



            root.Add(matrixDamage);
            root.Add(overviewDamageEffective);

            return root;
        }

        private VisualElement newDamageTypeUIElement(DamageType item, int index, SerializedProperty prop)
        {
            Foldout f = new Foldout() { text = item.Name, value = false };
            f.style.marginLeft = 15;

            TextField nameField = new TextField("Name") { value = item.Name };
            nameField.RegisterValueChangedCallback((e) => { item.Name = e.newValue; EditorUtility.SetDirty(target); });

            ColorField colorField = new ColorField("Color") { value = item.Color, hdr = true};
            colorField.RegisterValueChangedCallback((e) => { item.Color = (Color)e.newValue; EditorUtility.SetDirty(target); });

            ObjectField iconField = new ObjectField("Icon") { value = item.Icon, objectType = typeof(Sprite) };
            iconField.RegisterValueChangedCallback((e) => { item.Icon = (Sprite)e.newValue; EditorUtility.SetDirty(target); });

            PropertyField texturesField = new PropertyField(prop.FindPropertyRelative("_textures"), "Textures");
            texturesField.Bind(prop.serializedObject);

            f.Add(nameField);
            f.Add(iconField);
            f.Add(colorField);
            f.Add(texturesField);

            return f;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }

        private string VerticalText(string input)
        {
            var s = new StringBuilder(input.Length * 2);
            for (int i = 0; i < input.Length; i++)
            {
                s.Append(input[i]).Append("\n");
            }
            return s.ToString();
        }


    }

}