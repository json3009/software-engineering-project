/*
 * 
 * Copyright 2019 Marx Jason
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
 * documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, 
 * and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or 
 * substantial portions of the Software.

 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
 * TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
 * CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

using Utilities.Collections;
using Utilities.Extensions;

using Object = UnityEngine.Object;
using Component = UnityEngine.Component;

namespace Utilities.UI
{
    public class BoundListView<T> : VisualElement
    {
        private static DragableElement<T> copiedElement;

        protected enum DropType
        {
            Undefined,      // Undefined Drop Type
            Copy,           // Copy from one list to another
            Move,           // Move from one list to another
            Reorder,        // Reorder within a list
            Duplicate,      // Duplicate within a list
            Link            // Linq both elements
        }

        protected enum Side
        {
            Undefined,
            Top,
            Bottom
        }

        public enum VisualState
        {
            Visible,
            Hidden,
            Disabled
        }

        public struct ContextItem
        {
            readonly GUIContent content;
            readonly Func<bool> isOn;

            readonly OnContextMenuSelectDelegate onSelect;
            readonly OnContextMenuConditionalDelegate predicate;

            public GUIContent Content { get => content; }
            public Func<bool> IsOn => isOn;

            public OnContextMenuSelectDelegate OnSelect => onSelect;
            public OnContextMenuConditionalDelegate Predicate => predicate;

            public ContextItem(string content, OnContextMenuSelectDelegate onSelect, OnContextMenuConditionalDelegate predicate = null)
                : this(new GUIContent(content), null, onSelect, predicate) { }

            public ContextItem(GUIContent content, OnContextMenuSelectDelegate onSelect, OnContextMenuConditionalDelegate predicate = null)
                : this(content, null, onSelect, predicate) { }

            public ContextItem(string content, Func<bool> isOn, OnContextMenuSelectDelegate onSelect, OnContextMenuConditionalDelegate predicate = null)
                : this(new GUIContent(content), isOn, onSelect, predicate) { }

            public ContextItem(GUIContent content, Func<bool> isOn, OnContextMenuSelectDelegate onSelect, OnContextMenuConditionalDelegate predicate = null)
            {
                this.content = content ?? throw new ArgumentNullException(nameof(content));
                this.isOn = isOn ?? (() => { return false; });
                this.onSelect = onSelect ?? throw new ArgumentNullException(nameof(onSelect));
                this.predicate = predicate ?? ((e, y) => { return VisualState.Visible; });
            }

        }

        private static class HelpBoxes
        {
            public static readonly HelpBox errorPropertyCannotBeNull = new HelpBox("Given Property is null!", HelpBoxMessageType.Error);
            public static readonly HelpBox errorObjectCannotBeNull = new HelpBox("Given Object is null!", HelpBoxMessageType.Error);
            public static readonly HelpBox errorItemDrawingFailure = new HelpBox("There was a problem drawing this item!", HelpBoxMessageType.Error);
        }

        #region Delegates 

        /// <summary>
        /// Function wll be called each time a new row must be created.
        /// Use this function to cusomize the layout of each row. 
        /// </summary>
        /// <param name="item">The item to be drawn.</param>
        /// <param name="index">The index at which the item will be drawn.</param>
        /// <param name="prop">The optional serialized Property, will be NULL if elements in array are non serializable!</param>
        /// <returns></returns>
        public delegate VisualElement NewItemRowDelegate(T item, int index, SerializedProperty prop = null);

        /// <summary>
        /// On Select of Item in Context Menu.
        /// </summary>
        /// <param name="index">Index of row over mouse.</param>
        /// <param name="item">Item of row over mouse</param>
        public delegate void OnContextMenuSelectDelegate(int index, DragableElement<T> item);

        /// <summary>
        /// Conditional delegate to test if element should be shown, disabled or hidden.
        /// </summary>
        /// <param name="index">Index of row over mouse.</param>
        /// <param name="item">Item of row over mouse</param>
        /// <returns>How to display the item.</returns>
        public delegate VisualState OnContextMenuConditionalDelegate(int index, DragableElement<T> item);

        #endregion

        #region Fields

        private VisualElement _borderDrawn;
        private VisualElement _overlay;

        private IList<T> _externalSource;
        private ObservableList<T> _source;

        private readonly SerializedProperty _serializedProperty;

        private ScrollView _view;

        private NewItemRowDelegate _newItem;
        private Func<T, bool> validateNewItem = (e) => true;
        private Func<T, bool> addNewItem;
        private Func<T, bool> deleteItem;
        private Func<int, bool> deleteItemByIndex;

        private List<ContextItem> _contextItems = new List<ContextItem>();

        private bool _canCopy = true;
        private bool _canDuplicate = true;
        private bool _canLink = true;

        private bool _canChoose = true;

        private int[] _selected = new int[0];

        #endregion

        #region Properties

        public bool IsObject { get { return (typeof(Object).IsAssignableFrom(typeof(T))); } }                       //Test wether T is of Type Object
        public bool IsClonable { get { return (typeof(ICloneable).IsAssignableFrom(typeof(T))); } }                 //Test wether T can be cloned
        public bool IsStruct { get { return typeof(T).IsValueType; } }                                              //Test wether T is a Struct

        public bool IsLinkable { get { return typeof(ILinkable).IsAssignableFrom(typeof(T)); } }                    // Test wether T can be linked

        public bool CanCopy { get { return (IsClonable || IsStruct || IsObject) && _canCopy; } set { _canCopy = value; } }                     //Copy from one view to another
        public bool CanDuplicate { get { return (IsClonable || IsStruct) && _canDuplicate; } set { _canDuplicate = value; } }      //Copy within the same view

        public bool CanMove { get; set; } = true;           //Move from one view to another
        public bool CanReorder { get; set; } = true;        //Move within the same view

        public bool CanDelete { get; set; } = true;
        public bool CanChoose { get { return (OnItemsChosen != null && _canChoose); } set { _canChoose = value; } }

        public bool CanLink { get { return IsLinkable && _canLink && _serializedProperty != null && _serializedProperty.serializedObject != null; } set { _canLink = value; } }                     //Link object from one view to another

        public NewItemRowDelegate NewItem { get => _newItem; set { _newItem = value; PopulateList(); } }
        public Func<T, bool> ValidateNewItem { get => validateNewItem; set => validateNewItem = value; }
        public Func<T, bool> AddNewItem { get => addNewItem; set => addNewItem = value; }
        public Func<T, bool> DeleteItem { get => deleteItem; set => deleteItem = value; }
        public Func<int, bool> DeleteItemByIndex { get => deleteItemByIndex; set => deleteItemByIndex = value; }

        public int[] Selected { get => _selected; set => _selected = value; }
        public T[] SelectedItems { get { return Selected.Select(e => { return _source[e]; }).ToArray(); } }

        protected ScrollView View { get => _view; }


        #endregion

        #region Events

        public event Action<IEnumerable<T>> OnSelectionChange;
        public event Action<IEnumerable<T>> OnItemsChosen;
        public event Action<IEnumerable<T>> OnChange;
        public event Action<T> OnItemAdded;
        public event Action<T> OnItemRemoved;


        #endregion


        #region Constructors

        public BoundListView(SerializedProperty property, NewItemRowDelegate newItem = null)
        //: this(property.serializedObject.targetObject, property.propertyPath)
        {
            Debug.Assert(property != null, "Property cannot be NULL!");

            _serializedProperty = property;

            var fieldAndValue = property.GetFieldAndValue();

            FieldInfo field = fieldAndValue.Item1;


            if (field == null)
                throw new Exception($"Could not find '{property.propertyPath}' field!");

            if (!typeof(IList<T>).IsAssignableFrom(field.FieldType))
                throw new Exception($"'{property.propertyPath}' must be assignable from type '{typeof(IList)}<{typeof(T)}>'!\n {field.FieldType}");

            IList<T> arr = fieldAndValue.Item2;

            if (arr is ObservableList<T>)
            {
                ((ObservableList<T>)arr).CollectionChanged += (source, y) =>
                {

                    try
                    {
                        //Debug.Log(property.serializedObject);
                        property.serializedObject.ApplyModifiedProperties();
                        using (_source.IgnoreChange())
                        {
                            _source.Clear();
                            _source.AddRange(arr);
                        }
                        PopulateList();
                    }
                    catch (InvalidOperationException) { }

                };
            }


            var list = new ObservableList<T>(arr);

            list.CollectionChanged += (s, y) =>
            {

                try
                {
                    switch (y.Action)
                    {
                        case NotifyCollectionChangedAction.Add:

                            //Undo.RegisterCompleteObjectUndo(obj, "Added Item(s)");
                            foreach (T item in y.NewItems)
                            {
                                bool success = true;
                                if (AddNewItem == null)
                                    arr.Add(item);
                                else
                                    success = AddNewItem.Invoke(item);

                                if (success)
                                    OnItemAdded?.Invoke(item);
                                else
                                {
                                    Debug.LogError($"Could not add {item} to list.");
                                    using (_source.IgnoreChange())
                                    {
                                        _source.Remove(item);
                                    }
                                }
                            }
                            break;
                        case NotifyCollectionChangedAction.Move:
                            //Undo.RegisterCompleteObjectUndo(obj, "Moved Item(s)");
                            arr.Move(y.OldStartingIndex, y.NewStartingIndex);
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            //Undo.RegisterCompleteObjectUndo(obj, "Removed Item(s)");
                            foreach (int index in y.OldItems)
                            {
                                bool success = true;
                                T toRemove = arr[index];

                                if (DeleteItem == null)
                                    arr.RemoveAt(index);
                                else
                                    success = DeleteItem.Invoke(toRemove);

                                if (success)
                                    OnItemRemoved?.Invoke(toRemove);
                                else
                                {
                                    Debug.LogError($"Could not remove {toRemove} from list.");

                                    using (_source.IgnoreChange())
                                    {
                                        _source.Insert(index, toRemove);
                                    }
                                }
                            }
                            break;
                        case NotifyCollectionChangedAction.Replace:
                            throw new NotSupportedException("Replace action not yet supported");
                        //break;
                        case NotifyCollectionChangedAction.Reset:
                            //Undo.RegisterCompleteObjectUndo(obj, "Cleared Item(s)");
                            arr.Clear();
                            break;
                    }

                    Undo.FlushUndoRecordObjects();
                    EditorUtility.SetDirty(property.serializedObject.targetObject);
                }
                catch (InvalidOperationException) { }
            };

            Setup(list, newItem);
        }


        // todo: Add posibility to provide path to variable! (ex: myObject.subObject.myList[])
        // todo:        Does *kinda* work, 'NewItemRowDelegate' needs to provide correct item<T> to function (ex: would need to supply function with 'subObject' Object)
        // 
        /// <summary>
        /// Creates a new listView. Will use the property on the provided object.
        /// </summary>
        /// <param name="obj">The Object in which the property is stored</param>
        /// <param name="prop">The property to get (Does NOT allow for paths)</param>
        /// <param name="newItem">New Item function wll be called each time a new row must be created.
        /// Use this function to cusomize the layout of each row. 
        /// </param>
        //[Obsolete("Use 'Serialized Property' instead!")]
        public BoundListView(Object obj, string prop, NewItemRowDelegate newItem = null)
        {
            SerializedObject serializedObj = new SerializedObject(obj);

            var fieldAndValue = Reflection.GetFieldAndValue(obj, prop);

            FieldInfo field = fieldAndValue.Item1;


            if (field == null)
                throw new Exception($"Could not find '{prop}' field!");

            if (!typeof(IList<T>).IsAssignableFrom(field.FieldType))
                throw new Exception($"'{prop}' must be assignable from type '{typeof(IList)}<{typeof(T)}>'!\n {field.FieldType}");

            SerializedProperty property = PropertyHelper.GetArrayByPath(prop, serializedObj);


            if (property != null)
            {
                _serializedProperty = property;
            }

            IList<T> arr = fieldAndValue.Item2;

            if (arr is ObservableList<T>)
            {
                ((ObservableList<T>)arr).CollectionChanged += (source, y) =>
                {

                    try
                    {
                        serializedObj.ApplyModifiedProperties();
                        using (_source.IgnoreChange())
                        {
                            _source.Clear();
                            _source.AddRange(arr);
                        }
                        PopulateList();
                    }
                    catch (InvalidOperationException) { }

                };
            }


            var list = new ObservableList<T>(arr);

            list.CollectionChanged += (s, y) =>
            {

                try
                {
                    switch (y.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            //Undo.RegisterCompleteObjectUndo(obj, "Added Item(s)");
                            foreach (T item in y.NewItems)
                                arr.Add(item);
                            break;
                        case NotifyCollectionChangedAction.Move:
                            //Undo.RegisterCompleteObjectUndo(obj, "Moved Item(s)");
                            arr.Move(y.OldStartingIndex, y.NewStartingIndex);
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            //Undo.RegisterCompleteObjectUndo(obj, "Removed Item(s)");
                            foreach (int index in y.OldItems)
                                arr.RemoveAt(index);
                            break;
                        case NotifyCollectionChangedAction.Replace:
                            throw new NotSupportedException("Replace action not yet supported");
                        //break;
                        case NotifyCollectionChangedAction.Reset:
                            //Undo.RegisterCompleteObjectUndo(obj, "Cleared Item(s)");
                            arr.Clear();
                            break;
                    }

                    Undo.FlushUndoRecordObjects();
                    EditorUtility.SetDirty(obj);
                }
                catch (InvalidOperationException) { }
            };

            Setup(list, newItem);
        }

        /// <summary>
        /// Creates a new listView. Will use the provided array. Provided array will NOT be bound, and will not be automatically updated!
        /// </summary>
        /// <param name="source">The array to use.</param>
        /// <param name="newItem">New Item function wll be called each time a new row must be created.
        /// Use this function to cusomize the layout of each row. 
        /// </param>
        public BoundListView(IList<T> source, NewItemRowDelegate newItem = null)
        {
            ObservableList<T> observableLists = new ObservableList<T>(source);
            //_externalSource = source;
            //Debug.Log(source);

            observableLists.CollectionChanged += (s, y) =>
            {
                try
                {
                    switch (y.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            //Undo.RegisterCompleteObjectUndo(obj, "Added Item(s)");
                            foreach (T item in y.NewItems)
                                AddNewItem?.Invoke(item);
                            break;
                        case NotifyCollectionChangedAction.Move:
                            //Undo.RegisterCompleteObjectUndo(obj, "Moved Item(s)");
                            
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            //Undo.RegisterCompleteObjectUndo(obj, "Removed Item(s)");
                            foreach (int index in y.OldItems)
                                deleteItemByIndex?.Invoke(index);
                            break;
                        case NotifyCollectionChangedAction.Replace:
                            throw new NotSupportedException("Replace action not yet supported");
                        //break;
                        /*case NotifyCollectionChangedAction.Reset:
                            //Undo.RegisterCompleteObjectUndo(obj, "Cleared Item(s)");
                            _externalSource.Clear();*/
                            //break;
                    }
                }
                catch (InvalidOperationException) { }
            };

            Setup(observableLists, newItem);
        }

        /// <summary>
        /// Setup the list View.
        /// </summary>
        /// <param name="source">The list to use.</param>
        /// <param name="newItem">New Item function wll be called each time a new row must be created.
        /// Use this function to cusomize the layout of each row. 
        /// </param>
        private void Setup(ObservableList<T> source, NewItemRowDelegate newItem)
        {
            focusable = true;
            string path = FileHelper.GetAssetDirectoryPathByType(typeof(BoundListView<T>));

            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>($"{path}/BoundListView.uss"));

            _source = source;


            if (newItem != null)
                _newItem = newItem;
            else
                _newItem = NewItemDefault;

            _view = new ScrollView(ScrollViewMode.Vertical);
            _view.style.flexGrow = new StyleFloat(1);
            _view.verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible;

            Add(_view);

            _overlay = new VisualElement();
            _overlay.AddToClassList("TopMessageOverlay");
            _overlay.AddToClassList("hidden");

            /*_overlay.RegisterCallback<DragUpdatedEvent>((ev) =>
            {
                //ev.StopPropagation();
            });*/

            Add(_overlay);

            AddContextItems(new ContextItem[] {
                new ContextItem("Select", (index, item)=>{ OnChoosen(index); }, (index, item)=>{ return CanChoose?(index > -1)?VisualState.Visible:VisualState.Disabled:VisualState.Hidden; }),

                new ContextItem("Copy", (index, item)=>{ copiedElement = item; }, (index, item)=>{
                    if (!CanCopy) return VisualState.Hidden;

                    return index > -1 ? VisualState.Visible : VisualState.Disabled;
                }),

                new ContextItem("Paste", (index, item)=>{ DropItem(copiedElement, _source.Count, DropType.Copy); }, (index, item)=>{ if(!CanCopy && !CanDuplicate) return VisualState.Hidden; return copiedElement != null ? VisualState.Visible : VisualState.Disabled; }),
                new ContextItem("Delete", (index, item)=>{ Remove(_selected); }, (index, item)=>{
                    if (!CanDelete) return VisualState.Hidden;

                    return index > -1?VisualState.Visible:VisualState.Disabled;
                })
            });

            SetupListViewEvents();
        }

        #endregion

        #region Puplic Methods

        /// <summary>
        /// Set Value of item at given index.
        /// </summary>
        /// <param name="index">The index of the item.</param>
        /// <param name="value">The new value of the item.</param>
        public void SetValue(int index, T value)
        {
            _source[index] = value;
        }

        /// <summary>
        /// Remove Items from list given their respective indicies.
        /// </summary>
        /// <param name="indiciesToRemove">The items indicies to remove from list.</param>
        public void Remove(params int[] indiciesToRemove)
        {
            _source.RemoveAt(indiciesToRemove);
            PopulateList();
        }

        /// <summary>
        /// Delete currently selected Items
        /// </summary>
        public void RemoveCurrentlySelected()
        {
            if (_selected != null && _selected.Length > 0 && CanDelete)
            {
                Remove(_selected);
            }
        }

        /// <summary>
        /// Create new Item
        /// </summary>
        public void CreateNewItem(T item)
        {
            _source.Add(item);
            PopulateList();
        }

        #endregion

        #region Helper Methods

        /*
        private SerializedProperty GetArrayByPath(SerializedProperty prop)
        {
            SerializedProperty currentProperty = prop.Copy();
            SerializedProperty siblingProperty = prop.Copy();

            siblingProperty.Next(false);

            while (currentProperty.Next(true))
            {
                if (SerializedProperty.EqualContents(currentProperty, siblingProperty))
                    break;

                if (currentProperty.isArray)
                    return currentProperty;
            }

            return null;
        }

        private SerializedProperty GetArrayByPath(string prop, SerializedObject obj)
        {
            SerializedProperty property = obj.FindProperty(prop);

            Debug.Assert(property != null, $"Property '{prop}' not found!");
            return GetArrayByPath(property);
        }
        */

        private void SetVisualAid<Y>(DropType state, MouseEventBase<Y> ev) where Y : MouseEventBase<Y>, new()
        {
            SetVisualAid(state);
            ev.PreventDefault();
        }

        /// <summary>
        /// Set Visual Aid (Cursor, Overlay) when Dragging, depending on the given state.
        /// </summary>
        /// <param name="state">Drag State</param>
        private void SetVisualAid(DropType state)
        {
            switch (state)
            {
                case DropType.Move:
                    DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                    SetOverlay("CollabMoved Icon");
                    break;
                case DropType.Reorder:
                    DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                    HideOverlay();
                    break;
                case DropType.Copy:
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    SetOverlay("CollabCreate Icon");
                    break;
                case DropType.Duplicate:
                    //case DropType.Duplicate:
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    HideOverlay();
                    break;
                case DropType.Link:
                    DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                    SetOverlay("d_Linked");
                    break;
                case DropType.Undefined:
                    DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                    SetOverlay("CollabConflict Icon");
                    break;
                default:
                    HideOverlay();
                    break;
            }
        }

        /// <summary>
        /// Get Drop Type
        /// </summary>
        /// <typeparam name="Y">Mouse Event Type.</typeparam>
        /// <param name="ev">The event.</param>
        /// <returns>Drop type</returns>
        private DropType GetDropType<Y>(MouseEventBase<Y> ev) where Y : MouseEventBase<Y>, new()
        {
            if (!IsDropAllowed(out List<DragableElement<T>> data))
                return DropType.Undefined;

            //_view.Children().Any()
            // todo: allow multiple dropped items
            var isThisView = _view.Contains(data[0]);


            if (CanCopy && !isThisView && (ev.ctrlKey || !(CanMove)) && ValidateNewItem.Invoke(data[0].Item))
                return DropType.Copy;
            else if (CanDuplicate && isThisView && (ev.ctrlKey || !(CanReorder)) && ValidateNewItem.Invoke(data[0].Item))
                return DropType.Duplicate;
            else if (CanLink && !isThisView && ev.shiftKey)
                return DropType.Link;
            else if (CanMove && !isThisView && ValidateNewItem.Invoke(data[0].Item))
                return DropType.Move;
            else if (CanReorder && isThisView)
                return DropType.Reorder;

            return DropType.Undefined;
        }


        /// <summary>
        /// Get the Drop index
        /// </summary>
        /// <param name="target">Target (under which the mouse is)</param>
        /// <param name="mousePosition">The local mouse position</param>
        /// <returns>The index at which the mouse is clossest</returns>
        private int GetDropIndex<Y>(VisualElement target, MouseEventBase<Y> ev) where Y : MouseEventBase<Y>, new()
        {
            if (target == null) return _source.Count;

            int index = GetRowIndex(target);
            if (index < 0)
                return _source.Count;

            if (GetSide(target, ev) == Side.Bottom)
                index++;

            return index;
        }

        /// <summary>
        /// Get the side of the target which is closest to the mouse position.
        /// </summary>
        /// <param name="target">The target to get the side of.</param>
        /// <param name="mousePos">The local mouse position</param>
        /// <returns></returns>
        private Side GetSide<Y>(VisualElement target, MouseEventBase<Y> ev) where Y : MouseEventBase<Y>, new()
        {
            if (target == null) return Side.Undefined;
            return (ev.localMousePosition.y - target.localBound.y) < (target.contentRect.height / 2) ? Side.Top : Side.Bottom;
        }

        /// <summary>
        /// Get Row by Mouse Position
        /// </summary>
        /// <param name="t">The Mouse Event</param>
        /// <returns>The Row</returns>
        private DragableElement<T> GetRowByTarget<Y>(MouseEventBase<Y> ev) where Y : MouseEventBase<Y>, new()
        {
            return GetRowByTarget((VisualElement)ev.target);
        }

        /// <summary>
        /// Get Row by Mouse Position
        /// </summary>
        /// <param name="t">The Mouse Event</param>
        /// <returns>The Row, if found; Null otherwhise!</returns>
        private DragableElement<T> GetRowByTarget(VisualElement t)
        {
            if (t == null) return null;
            return t.GetFirstAncestorOfType<DragableElement<T>>();
        }

        /// <summary>
        /// Get the index of a Row.
        /// </summary>
        /// <param name="d">The Element to get the index of.</param>
        /// <returns>The index, if found; -1 otherwhise!</returns>
        private int GetRowIndex(VisualElement d)
        {
            return _view.IndexOf(d);
        }


        /// <summary>
        /// Checks whether Drag&Drop is valid!
        /// </summary>
        /// <param name="data">The data carried by the Drag&Drop.</param>
        /// <returns>True, if allowed; False otherwhise.</returns>
        private bool IsDropAllowed(out List<DragableElement<T>> data)
        {
            data = new List<DragableElement<T>>();

            // Test whether the dragged item is a VisualElement 
            DragableElement<T> dragableItem = (DragableElement<T>)DragAndDrop.GetGenericData(typeof(DragableElement<T>).FullName);
            if (dragableItem != null)
                data.Add(dragableItem);
            //if(ValidateNewItem.Invoke(draggedItem.Item))

            // If it is not test if item is assignable to generic <T>
            if (data.Count == 0)
            {

                Object[] references = DragAndDrop.objectReferences;
                if (typeof(Component).IsAssignableFrom(typeof(T)))
                {
                    references = references.
                        Where(e => e.GetType().Equals(typeof(GameObject))).
                        Select((e) => (Object)(object)((GameObject)e).GetComponent<T>()).
                        Where(e => e != null).
                        ToArray();
                }
                else if (typeof(Object).IsAssignableFrom(typeof(T)))
                {
                    references = references
                        .Where(e => typeof(T).IsAssignableFrom(e.GetType()))
                        .ToArray();
                }
                else
                    references = new Object[0];

                foreach (var item in references)
                {
                    DragableElement<T> elem = new DragableElement<T>(() => { return (dynamic)item; });
                    data.Add(elem);
                }
            }


            return data.Count != 0;
        }

        /// <summary>
        /// Check if DraggedItem is Adjacent or Equal the given index.
        /// </summary>
        /// <param name="draggedItem">The item being dragged.</param>
        /// <param name="index">The index at which to test for adjacency.</param>
        /// <returns>True, if draggedItem equals or is adjacent to index; False otherwhise!</returns>
        private bool IsAdjacentOrEqual(DragableElement<T> draggedItem, int index)
        {

            if (_source.Count == 0 || index < 0) return false;

            // Test for item at index
            if (index < _source.Count && ((DragableElement<T>)_view[index]).Equals(draggedItem))
                return true;

            // Test for previous item
            if (index > 0 && ((DragableElement<T>)_view[index - 1]).Equals(draggedItem))
                return true;

            return false;
        }

        /// <summary>
        /// Clear Selection
        /// </summary>
        private void ClearSelection()
        {
            foreach (var selected in _selected)
            {
                _view[selected].RemoveFromClassList("List_row_selected");
            }
            _selected = new int[0];
        }

        /// <summary>
        /// Add item to Selection
        /// </summary>
        /// <param name="index">The index of the item to add.</param>
        private void AddToSelection(int index)
        {
            SelectRow(new HashSet<int>(_selected) { index }.ToArray());
        }

        /// <summary>
        /// Add Item to context menu.
        /// </summary>
        /// <param name="items">The Item to add.</param>
        public void AddContextItems(params ContextItem[] items)
        {
            _contextItems.AddRange(items);
        }

        /// <summary>
        /// Get Copy of Object.
        /// </summary>
        /// <param name="toClone">The object to clone.</param>
        /// <returns>A copy of the given item.</returns>
        private T GetCopy(T toClone)
        {
            if (IsClonable)
                return (T)((ICloneable)toClone).Clone();
            else if (IsStruct || IsObject)
                return toClone;

            return default;
        }

        #endregion

        #region Visual Element Methods

        /// <summary>
        /// Reset the overlay as well as the borders for dropping items.
        /// </summary>
        private void ResetVisuals()
        {
            if (_borderDrawn != null)
            {
                _borderDrawn.RemoveFromClassList("TopInsertBorder");
                _borderDrawn.RemoveFromClassList("BottomInsertBorder");
            }

            if (!_overlay.ClassListContains("hidden"))
                _overlay.AddToClassList("hidden");
        }

        /// <summary>
        /// Set overlay no top of list view
        /// </summary>
        /// <param name="icontxt">The icon to show</param>
        private void SetOverlay(string icontxt)
        {
            _overlay.RemoveFromClassList("hidden");

            _overlay.Clear();
            VisualElement icon = new VisualElement();
            icon.AddToClassList("icon");
            icon.style.backgroundImage = new StyleBackground((Texture2D)EditorGUIUtility.IconContent(icontxt).image);
            icon.style.unityBackgroundScaleMode = new StyleEnum<ScaleMode>(ScaleMode.ScaleToFit);

            _overlay.Add(icon);
        }

        /// <summary>
        /// Hide overlay
        /// </summary>
        private void HideOverlay()
        {
            if (!_overlay.ClassListContains("hidden"))
                _overlay.AddToClassList("hidden");
        }

        /// <summary>
        /// Show critical error on Overlay!
        /// </summary>
        /// <param name="ex"></param>
        private void ShowErrorOnOverlay(Exception ex, VisualElement extra = null)
        {
            SetVisualAid(DropType.Undefined);
            _overlay.Add(new HelpBox("There was a problem drawing an item in the list!\n" + ex.GetType() + ": " + ex.Message, HelpBoxMessageType.Error));

            if (extra != null)
                _overlay.Add(extra);
        }

        /// <summary>
        /// Create new Row
        /// </summary>
        /// <param name="obj">The object</param>
        /// <param name="index">The index</param>
        /// <returns></returns>
        private VisualElement NewItemInList(T obj, int index)
        {
            if (obj == null) return null;

            if (_serializedProperty != null)
                _serializedProperty.serializedObject.Update();

            VisualElement result;

            if (obj is SerializedProperty && !typeof(T).Equals(typeof(SerializedProperty)))
            {
                SerializedProperty property = (SerializedProperty)(object)obj;
                result = new PropertyField(property);
                result.Bind(property.serializedObject);
            }
            else
                result = _newItem.Invoke(obj, index, _serializedProperty?.GetArrayByPath()?.GetArrayElementAtIndex(index));

            result.AddToClassList("List_view");

            return result;
        }

        /// <summary>
        /// Select Row given the index.
        /// </summary>
        /// <param name="index">The index of the row to select.</param>
        private void SelectRow(int index)
        {
            SelectRow(new int[1] { index });
        }

        /// <summary>
        /// Select Rows given the indicies.
        /// </summary>
        /// <param name="indicies">The indicies of the rows to select</param>
        private void SelectRow(int[] indicies)
        {

            ClearSelection();

            _selected = new int[indicies.Length];
            List<T> selectedItems = new List<T>(indicies.Length);

            foreach ((int item, int index) in indicies.WithIndex())
            {
                if (item < 0 || item >= _view.childCount)
                    continue;

                VisualElement selectedRow = _view[item];

                selectedItems.Add(_source[item]);
                _selected[index] = item;

                selectedRow.AddToClassList("List_row_selected");
                _view.ScrollTo(selectedRow);
            }

            OnSelectionChange?.Invoke(selectedItems);
        }

        #endregion

        #region Event Registrations

        /// <summary>
        /// Register all events on the listView.
        /// </summary>
        private void SetupListViewEvents()
        {
            _source.CollectionChanged += (s, y) => { OnChange?.Invoke(_source); };

            RegisterCallback<MouseDownEvent>(e =>
            {
                if (e.pressedButtons != 2) return;
                OnRightClick(-1, e);
            });

            RegisterCallback<KeyDownEvent>((e) => OnKeyDown(e));


            RegisterCallback<DragLeaveEvent>(OnDragLeaveEvent);

            RegisterCallback<DragExitedEvent>(OnDragExitedEvent);

            RegisterCallback<DragPerformEvent>(OnDragPerformEvent);

            RegisterCallback<DragUpdatedEvent>(OnDragUpdatedEvent);

            RegisterCallback<AttachToPanelEvent>((target) => { /*Debug.Log("Populating List");*/ PopulateList(); });
        }


        #endregion

        #region Event Handlers

        /// <summary>
        /// On Drag left List View.
        /// </summary>
        /// <param name="e">The Event.</param>
        private void OnDragLeaveEvent(DragLeaveEvent e)
        {
            ResetVisuals();
        }

        /// <summary>
        /// On Drag Exited. (When drop has been perforemed)
        /// </summary>
        /// <param name="e">The Event.</param>
        private void OnDragExitedEvent(DragExitedEvent e)
        {
            ResetVisuals();
        }

        /// <summary>
        /// On Drag Performed. (When drop has finished)
        /// </summary>
        /// <param name="e">The Event</param>
        private void OnDragPerformEvent(DragPerformEvent e)
        {
            ResetVisuals();

            // todo: allow multiple drop items
            if (!IsDropAllowed(out List<DragableElement<T>> draggedElement)) return;

            DropType type = GetDropType(e);
            if (DropType.Undefined == type)
                return;

            int index = GetDropIndex(_borderDrawn, e);

            if (!IsAdjacentOrEqual(draggedElement[0], index) || _source.Count <= 1 || index == _source.Count)
                DropItem(draggedElement[0], index, type);
        }

        /// <summary>
        /// On Drag Update.
        /// </summary>
        /// <param name="e">The Event.</param>
        private void OnDragUpdatedEvent(DragUpdatedEvent e)
        {
            DropType type = GetDropType(e);
            SetVisualAid(type);

            // todo: allow multiple drop items
            if (!IsDropAllowed(out List<DragableElement<T>> draggedElement))
                return;


            if (type == DropType.Undefined)
                return;



            VisualElement target = GetRowByTarget(e);

            if (target != null && type != DropType.Move && type != DropType.Copy)
            {
                if (_borderDrawn != null)
                {
                    _borderDrawn.RemoveFromClassList("TopInsertBorder");
                    _borderDrawn.RemoveFromClassList("BottomInsertBorder");
                }

                //if(type == DropType.Move)

                int index = GetDropIndex(target, e);

                if (IsAdjacentOrEqual(draggedElement[0], index))
                    return;

                Side side = GetSide(target, e);
                if (side == Side.Top)
                {
                    if (!target.ClassListContains("TopInsertBorder"))
                        target.AddToClassList("TopInsertBorder");

                }
                else if (side == Side.Bottom)
                {
                    if (!target.ClassListContains("BottomInsertBorder"))
                        target.AddToClassList("BottomInsertBorder");
                }

                _borderDrawn = target;
            }
        }

        /// <summary>
        /// On Right Click.
        /// </summary>
        /// <typeparam name="Y">The Event Type</typeparam>
        /// <param name="index">The index of the current Row.</param>
        /// <param name="e">The event.</param>
        private void OnRightClick<Y>(int index, MouseEventBase<Y> e) where Y : MouseEventBase<Y>, new()
        {
            GenericMenu menu = new GenericMenu();
            Vector2 pos = e.localMousePosition;

            DragableElement<T> item = null;
            if (index > -1)
            {
                item = (DragableElement<T>)_view[index];
                pos = new Vector2(item.layout.xMin + e.localMousePosition.x, item.layout.yMin + e.localMousePosition.y);
                e.StopPropagation();
            }

            foreach ((ContextItem contextItem, int contextindex) in _contextItems.WithIndex())
            {
                switch (contextItem.Predicate.Invoke(index, item))
                {
                    case VisualState.Visible:
                        menu.AddItem(contextItem.Content, contextItem.IsOn.Invoke(), () => contextItem.OnSelect.Invoke(index, item));
                        break;
                    case VisualState.Disabled:
                        menu.AddDisabledItem(contextItem.Content);
                        break;
                }
            }

            menu.DropDown(new Rect(this.LocalToWorld(pos), Vector2.zero));
        }

        /// <summary>
        /// On Arrow keyDown
        /// </summary>
        /// <param name="e">The Event.</param>
        private void OnKeyDown(KeyDownEvent e)
        {
            var selected = (_selected.Length > 0) ? _selected[_selected.Length - 1] : 0;
            switch (e.keyCode)
            {
                case KeyCode.UpArrow:
                    SelectRow(Mathf.Clamp(selected - 1, 0, _view.childCount - 1));
                    break;
                case KeyCode.DownArrow:
                    SelectRow(Mathf.Clamp(selected + 1, 0, _view.childCount - 1));
                    break;
                case KeyCode.Delete:
                    if (CanDelete) Remove(_selected);
                    break;
                case KeyCode.C:
                    if (e.ctrlKey && CanCopy) copiedElement = (DragableElement<T>)_view[selected];
                    break;
                case KeyCode.V:
                    if (e.ctrlKey && e.shiftKey) DropItem(copiedElement, _source.Count, DropType.Copy);
                    else if (e.ctrlKey) DropItem(copiedElement, _source.Count, DropType.Copy);
                    break;
                case KeyCode.Return:
                    if (CanChoose) OnChoosen(_selected);
                    break;
            }
        }

        /// <summary>
        /// On Items Choosen Event
        /// </summary>
        /// <param name="indicies">The indicies that were choosen.</param>
        private void OnChoosen(params int[] indicies)
        {
            OnItemsChosen?.Invoke(indicies.Select(e => { return _source[e]; }).ToArray());
        }

        /// <summary>
        /// Drop Item Event Handler.
        /// </summary>
        /// <param name="draggedItem">The Item that is being dragged.</param>
        /// <param name="index">The index at which to drop.</param>
        /// <param name="dropType">The drop type.</param>
        private void DropItem(DragableElement<T> draggedItem, int index, DropType dropType)
        {
            if (DropType.Undefined == dropType)
                throw new InvalidOperationException("Undefined Drag type!");

            switch (dropType)
            {
                case DropType.Copy:
                case DropType.Duplicate:
                    if (ValidateNewItem.Invoke(draggedItem.Item))
                        _source.Insert(index, GetCopy(draggedItem.Item));
                    break;

                case DropType.Move:
                    if (ValidateNewItem.Invoke(draggedItem.Item))
                    {
                        _source.Insert(index, draggedItem.Item);
                        draggedItem.Remove();
                    }
                    break;

                case DropType.Reorder:
                    if (_view.Children().Contains(draggedItem))
                        index = _source.Move(_view.IndexOf(draggedItem), index);
                    break;

                case DropType.Link:
                    T toLink = (T)Activator.CreateInstance(draggedItem.Item.GetType());

                    //((ILinkable)toLink).LinkReference(new ReferenceLink(draggedItem.InspectedProperty));
                    _source.Insert(index, toLink);
                    break;
            }

            DragAndDrop.AcceptDrag();
            PopulateList();
            SelectRow(index);
            Focus();
        }

        #endregion


        /// <summary>
        /// Populate list with elements!
        /// </summary>
        private void PopulateList()
        {
            ClearSelection();
            _view.Clear();

            int lastIndex = -1;

            try
            {
                foreach (var (item, index) in _source.WithIndex())
                {
                    lastIndex = index;

                    DragableElement<T> e = new DragableElement<T>();
                    e.SetItem(() => { return _source[index]; });
                    if (_serializedProperty != null && _serializedProperty.GetValue<IList>().Count > index)
                        e.InspectedProperty = _serializedProperty?.GetArrayByPath()?.GetArrayElementAtIndex(index);

                    e.style.flexGrow = new StyleFloat(1);
                    e.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);

                    if (CanReorder || CanMove || CanCopy || CanDuplicate || CanLink)
                    {
                        VisualElement icon = new VisualElement();
                        icon.AddToClassList("List_row_reorder_icon");
                        icon.style.backgroundImage = new StyleBackground((Texture2D)EditorGUIUtility.IconContent("align_vertically_center").image);
                        e.Add(icon);
                    }
                    else
                    {
                        e.CanDrag = false;
                    }

                    VisualElement element = NewItemInList(item, index);

                    element.style.flexGrow = new StyleFloat(1);

                    if (index % 2 == 0)
                        e.AddToClassList("List_row");
                    else
                        e.AddToClassList("List_row_inverse");

                    e.Add(element);
                    _view.Add(e);

                    e.OnMoved += (() =>
                    {
                        _source.RemoveAt(index);
                        PopulateList();
                    });

                    e.RegisterCallback<MouseDownEvent>((elem) =>
                    {
                        switch (elem.pressedButtons)
                        {
                            case 1:
                                if (elem.ctrlKey)
                                    AddToSelection(index);
                                else
                                    SelectRow(index);
                                break;
                            case 2:
                                OnRightClick(index, elem);
                                break;
                        }
                        return;
                    });

                    e.SetupDragEvents();
                }
            }
            catch (Exception e)
            {
                Button btn = new Button(() => { _source.RemoveAt(lastIndex); PopulateList(); }) { text = "FIX" };
                ShowErrorOnOverlay(e, btn);
                Debug.LogError(e);
                //todo: Make error layer system persistent!!!
            }
        }

        /// <summary>
        /// Create new Default Row element
        ///     Should be used if no NewItem function was provided!
        /// </summary>
        /// <param name="item">The Item to draw</param>
        /// <param name="index">The index at which to draw</param>
        /// <param name="prop">The property of the item to draw (Can be empty!)</param>
        /// <returns></returns>
        private VisualElement NewItemDefault(T item, int index, SerializedProperty prop)
        {
            //Debug.Log(item);

            if (item != null)
            {
                if (prop == null)
                    return new Label(item.ToString());
                else
                {
                    PropertyField f = new PropertyField(prop);

                    f.Bind(prop.serializedObject);
                    return f;
                }

            }

            return HelpBoxes.errorPropertyCannotBeNull;
        }


    }
}