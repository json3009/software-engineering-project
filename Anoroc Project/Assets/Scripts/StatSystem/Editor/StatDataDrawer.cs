using System;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Utilities.UI;
using static StatSystem.StatData;

namespace StatSystem.Editor
{
    public static class StatDataDrawer
    {
        public static BoundList<DataWrapper> CreateGUI(StatData data, SerializedObject obj, StatTraits system, Action onChange = null)
        {

            BoundListView<DataWrapper> ListView = new BoundListView<DataWrapper>(data.Modifiers, (e, index, prop) => CreateGUI(e, obj, onChange))
            {
                CanCopy = false,
                CanChoose = false,
                CanDelete = true,
                CanDuplicate = false,
                CanLink = false,
                CanMove = false,
                CanReorder = false
            };
            ListView.style.flexGrow = 1;
            ListView.AddNewItem = (e) => { data.Modifiers.Add(e); return true; };
            ListView.DeleteItemByIndex = (e) => { data.Modifiers.RemoveAt(e); return true; };
            ListView.OnChange += (e) => { onChange?.Invoke(); EditorUtility.SetDirty(obj.targetObject); };

            BoundList<DataWrapper> list = new BoundList<DataWrapper>(ListView) { Label = "Attributes" };
            list.Content.style.height = 300;

            PopupField<StatType> statField = new PopupField<StatType>(system.StatTypes.ToList(), 0, (e) => e.Name, (e) => e.Name);
            list.Header.Add(statField);

            Button add = new Button(() =>
            {
                DataWrapper item = new DataWrapper(statField.value, (IStatAttribute)Activator.CreateInstance(statField.value.Type));
                item.Attribute.AddModifier((IStatModifier)Activator.CreateInstance(item.Attribute.ValueType));
                ListView.CreateNewItem(item);
            })
            { text = "+", tooltip = "Add new Item" };
            add.style.backgroundImage = new StyleBackground((Texture2D)EditorGUIUtility.IconContent("d_Toolbar Plus").image);

            list.Header.Add(add);

            return list;
        }

        public static VisualElement CreateGUI(DataWrapper dataWrapper, SerializedObject obj, Action onChange)
        {
            if (dataWrapper.Type == null)
                return new VisualElement();

            BoundListView<IStatModifier> ListView = new BoundListView<IStatModifier>(dataWrapper.Attribute.Modifiers, (item, index, prop) => NewStatTypeUIElement(item, index, obj, dataWrapper, onChange))
            {
                CanCopy = false,
                CanChoose = false,
                CanDelete = true,
                CanDuplicate = false,
                CanLink = false,
                CanMove = false,
                CanReorder = false
            };
            ListView.style.flexGrow = 1;
            ListView.AddNewItem = (e) => { dataWrapper.Attribute.AddModifier(e); return true; };
            ListView.DeleteItemByIndex = (e) => { dataWrapper.Attribute.RemoveModifier(e); return true; };
            ListView.OnChange += (e) => { onChange?.Invoke(); EditorUtility.SetDirty(obj.targetObject); };

            BoundList<IStatModifier> list = new BoundList<IStatModifier>(ListView) { Label = dataWrapper.Type.Name, IsCollapsed = true, IsCollapsable = dataWrapper.Attribute.Modifiers.Length > 0 };
            list.Content.style.maxHeight = 200;

            list.CreateNewItem = () =>
            {
                return (IStatModifier)Activator.CreateInstance(dataWrapper.Attribute.ValueType);
            };
            //list.Header.Add(new Label(levels.GetLevelData(dataWrapper.)));
            //list.Header.Add(new Label(dataWrapper.Attribute.Value.ToString()));

            return list;
        }

        private static VisualElement NewStatTypeUIElement(IStatModifier item, int index, SerializedObject obj, DataWrapper wrapper, Action onChange)
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;

            Label indexlabel = new Label(index.ToString());
            indexlabel.style.width = 20;
            indexlabel.style.unityTextAlign = TextAnchor.MiddleRight;
            row.Add(indexlabel);

            VisualElement content = item.CreateGUI(() =>
            {
                wrapper.Attribute.RequestRecalculation();
                onChange?.Invoke();
                EditorUtility.SetDirty(obj.targetObject);
            });
            content.style.flexGrow = 1;

            row.Add(content);

            return row;
        }
    }
}
