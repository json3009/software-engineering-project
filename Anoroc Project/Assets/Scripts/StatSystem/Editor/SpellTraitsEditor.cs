using UnityEditor;
using UnityEngine.UIElements;
using Utilities.UI;

namespace StatSystem.Editor
{
    [CustomEditor(typeof(StatTraits), true)]
    public class StatTraitsEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            StatTraits traits = (StatTraits)target;
            root.style.height = 200;
            root.style.maxHeight = 200;

            
            BoundListView<StatType> statTypeListView = new BoundListView<StatType>(traits.StatTypes, NewStatTypeUIElement)
            {
                CanCopy = false,
                CanChoose = false,
                CanDelete = false,
                CanDuplicate = false,
                CanLink = false,
                CanMove = false,
                CanReorder = false
            };

            statTypeListView.style.height = 200;
            root.Add(statTypeListView);

            return root;
        }


        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }


        private VisualElement NewStatTypeUIElement(StatType item, int index, SerializedProperty prop)
        {
            Foldout f = new Foldout() { text = item.Name, value = false };
            f.style.marginLeft = 15;

            TextField idField = new TextField("ID") { isReadOnly = true, value = item.ID };
            idField.SetEnabled(false);

            TextField nameField = new TextField("Name") { value = item.Name };
            nameField.RegisterValueChangedCallback((e) => { item.Name = e.newValue; EditorUtility.SetDirty(target); });

            TextField descField = new TextField("Description") { multiline = true, maxLength = 200, value = item.Description };
            descField.RegisterValueChangedCallback((e) => { item.Description = e.newValue; EditorUtility.SetDirty(target); });

            descField.style.height = 100;

            f.Add(idField);
            f.Add(nameField);
            f.Add(descField);

            return f;
        }
    }
}
