using System;
using UnityEditor;
using UnityEngine.UIElements;
using Utilities;
using Utilities.UI;

namespace Scripts.BodySystem.Editor
{
    [CustomPropertyDrawer(typeof(BodyPartFlag))]
    public class BodyPartFlagDrawer : PropertyDrawer
    {
        //BodyPartFlag flag;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            BodyPartFlag flag = property.GetValue<BodyPartFlag>();

            Foldout root = new Foldout() { value = false, text = "  " + (flag.name.IsEmpty() ? "New Item" : flag.name) };
            root.style.marginLeft = 15;

            TextField name = new TextField("Name") { value = flag.name };
            name.RegisterValueChangedCallback((e) => { flag.name = e.newValue; EditorUtility.SetDirty(property.serializedObject.targetObject); });

            BoundListView<BodyPartFlag> listView = new BoundListView<BodyPartFlag>(property.FindPropertyRelative("children"));

            listView.AddNewItem = (e) => flag.AddChild(e);

            BoundList<BodyPartFlag> bodyPartsView = new BoundList<BodyPartFlag>(listView)
            {
                Label = "Body Parts",
                CreateNewItem = () => new BodyPartFlag() { name = "New Body Part", id = Guid.NewGuid() }
            };

            root.Add(name);
            //root.Add(row);
            root.Add(bodyPartsView);

            return root;
        }





    }
}
