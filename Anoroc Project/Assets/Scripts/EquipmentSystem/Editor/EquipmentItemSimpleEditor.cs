using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace EquipmentSystem.Editor
{
    [CustomEditor(typeof(EquipmentItemSimple))]
    public class EquipmentItemSimpleEditor : EquipmentItemEditor
    {

        VisualElement section;
        EquipmentItemSimple item;

        private void EquipmentItemSimpleEditor_OnUpdate()
        {
            section.Clear();
            if (base.IsValid)
            {
                foreach (var side in item.Definition.Sides)
                {
                    ObjectField sideField = new ObjectField($"Sprite - {side.name}") { objectType = typeof(Sprite), value = item.GetSprite(side.id) };
                    sideField.RegisterValueChangedCallback((e) => { item.SetSprite(side.id, (Sprite)e.newValue); EditorUtility.SetDirty(target); });
                    section.Add(sideField);
                }
            }
            else
            {
                section.Add(new HelpBox("Invalid Base Settings, please verify!", HelpBoxMessageType.Error));
            }
        }

        public override VisualElement CreateInspectorGUI()
        {
            section = new Frame() { Label = "Sprites" };
            item = target as EquipmentItemSimple;

            VisualElement root = base.CreateInspectorGUI();
            OnUpdate += EquipmentItemSimpleEditor_OnUpdate;

            root.Add(section);

            EquipmentItemSimpleEditor_OnUpdate();

            return root;
        }
    }
}
