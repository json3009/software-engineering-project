using System.Collections.Generic;
using System.Linq;
using InventorySystem;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Utilities;
using Utilities.Extensions;
using Utilities.UI;

namespace EquipmentSystem.Editor
{


    [CustomPropertyDrawer(typeof(EquipmentManager))]
    public class EquipmentSystemDrawer : PropertyDrawer
    {
        private BoundListView<EquipmentItem> _equipmentList;
        private BoundListView<EquipmentSet> _equipmentSet;
        private EquipmentManager _manager;

        private VisualElement _root;


        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            _manager = property.GetValue<EquipmentManager>();

            _root = new VisualElement();
            _root.style.flexGrow = 1;

            _equipmentList = new BoundListView<EquipmentItem>(property.FindPropertyRelative("values"), newItem)
            {
                CanChoose = false,
                CanCopy = true,
                CanMove = true,
                CanReorder = true,
                CanDelete = true,
                CanDuplicate = false
            };
            _equipmentList.style.flexGrow = 1;
            _equipmentList.ValidateNewItem = (obj) => _manager.CanEquip(obj);
            _equipmentList.style.minHeight = 100;
            _equipmentList.style.maxHeight = 200;

            _equipmentList.AddNewItem = (e) => _manager.Equip(e);
            _equipmentList.DeleteItem = (e) => _manager.UnEquip(e);
            _equipmentList.OnChange += EquipmentList_OnChange;

            BoundList<EquipmentItem> equipmentListContainer = new BoundList<EquipmentItem>(_equipmentList)
            {
                Label = "Equipment Objects",
                IsCollapsed = true
            };
            
            ObjectField equipmentAddValue = new ObjectField() {objectType = typeof(EquipmentItem), allowSceneObjects = false};
            equipmentAddValue.style.width = 150;
            equipmentListContainer.CreateNewItem = () => (EquipmentItem) equipmentAddValue.value;
            equipmentListContainer.Header.Add(equipmentAddValue);

            _equipmentSet = new BoundListView<EquipmentSet>(property.FindPropertyRelative("_sets"), newSetItem)
            {
                CanChoose = false,
                CanCopy = true,
                CanMove = true,
                CanReorder = true,
                CanDelete = true,
                CanDuplicate = false
            };
            _equipmentSet.style.flexGrow = 1;
            _equipmentSet.ValidateNewItem = (obj) => _manager.CanEquip(obj);
            _equipmentSet.style.minHeight = 100;
            _equipmentSet.style.maxHeight = 200;

            _equipmentSet.AddNewItem = (e) => _manager.Equip(e);
            _equipmentSet.DeleteItem = (e) => _manager.UnEquip(e);
            _equipmentSet.OnChange += EquipmentSet_OnChange;

            BoundList<EquipmentSet> equipmentSetContainer = new BoundList<EquipmentSet>(_equipmentSet)
            {
                Label = "Equipment Sets",
                IsCollapsed = true
            };

            ObjectField equipmentSetAddValue = new ObjectField() {objectType = typeof(EquipmentSet), allowSceneObjects = false};
            equipmentSetAddValue.style.width = 150;
            equipmentSetContainer.CreateNewItem = () => (EquipmentSet) equipmentSetAddValue.value;
            equipmentSetContainer.Header.Add(equipmentSetAddValue);
            
            _root.Add(equipmentSetContainer);
            _root.Add(equipmentListContainer);

            return _root;
        }

        private VisualElement newItem(EquipmentItem item, int index, SerializedProperty prop)
        {
            VisualElement element = new VisualElement();

            Foldout foldout = new Foldout() { value = false, text = $"  [{item.Layer.name}] - " + (item.Name.IsEmpty()? item.name : item.Name) };
            foldout.Add(UnityEditor.Editor.CreateEditor(item).CreateInspectorGUI());
            foldout.style.marginLeft = 15;

            element.Add(foldout);
            return element;
        }
        
        private VisualElement newSetItem(EquipmentSet item, int index, SerializedProperty prop)
        {
            VisualElement element = new VisualElement();

            Foldout foldout = new Foldout() { value = false, text = (item.Name.IsEmpty()? item.name : item.Name) };
            foldout.Add(UnityEditor.Editor.CreateEditor(item).CreateInspectorGUI());
            foldout.style.marginLeft = 15;

            element.Add(foldout);
            return element;
        }


        private void EquipmentList_OnChange(IEnumerable<EquipmentItem> obj)
        {
            _root.SendChangeEvent(null, obj.First());
        }
        
        private void EquipmentSet_OnChange(IEnumerable<EquipmentSet> obj)
        {
            _root.SendChangeEvent(null, obj.FirstOrDefault());
        }
    }
}
