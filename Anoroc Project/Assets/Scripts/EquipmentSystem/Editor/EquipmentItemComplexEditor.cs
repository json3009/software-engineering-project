using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace EquipmentSystem.Editor
{
    [CustomEditor(typeof(EquipmentItemComplex))]
    public class EquipmentItemComplexEditor : EquipmentItemEditor
    {

        VisualElement section;
        EquipmentItemComplex item;

        private void EquipmentItemSimpleEditor_OnUpdate()
        {
            section.Clear();
            if (!base.IsValid) return;

            ObjectField mainObjectField = new ObjectField($"Main Object") { objectType = typeof(GameObject), value = item.TheObject, allowSceneObjects = false };
            mainObjectField.RegisterValueChangedCallback((e) => { item.TheObject = (GameObject)e.newValue; EditorUtility.SetDirty(target); });
            section.Add(mainObjectField);

            Foldout f = new Foldout() { text = "Sprites" };
            foreach (var side in item.Definition.Sides)
            {
                ObjectField sideField = new ObjectField($"Object - {side.name}") { objectType = typeof(GameObject), value = item.GetObject(side.id) };
                sideField.RegisterValueChangedCallback((e) =>
                {
                    var val = (GameObject)e.newValue;
                    if (e.newValue == null)
                        return;

                    /*if (!val.transform.IsChildOf(item.TheObject.transform))
                    {
                        section.Add(new HelpBox($"Cannot add '{val}', gameobject is not a descendant of '{item.TheObject}'!", HelpBoxMessageType.Warning));
                        sideField.value = null;
                        return;
                    }*/

                    item.SetObject(side.id, val);
                    EditorUtility.SetDirty(target);
                });
                f.Add(sideField);
            }

            section.Add(f);
        }

        public override VisualElement CreateInspectorGUI()
        {
            section = new VisualElement();
            item = target as EquipmentItemComplex;

            VisualElement root = base.CreateInspectorGUI();
            OnUpdate += EquipmentItemSimpleEditor_OnUpdate;

            root.Add(section);

            EquipmentItemSimpleEditor_OnUpdate();

            return root;
        }
    }
}
