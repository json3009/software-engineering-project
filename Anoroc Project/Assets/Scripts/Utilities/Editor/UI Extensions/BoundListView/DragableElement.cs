/*
    MIT License

    Copyright (c) 2019 Marx Jason

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.

*/

using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace Utilities.UI
{
    /// <summary>
    /// Visual Element that can be dragged!
    /// </summary>
    /// <typeparam name="T">Type of Value the Visual Element is carring</typeparam>
    public class DragableElement<T> : VisualElement
    {
        private bool isDragging = false;
        private bool isReadyForDrag = false;

        private bool _canDrag = true;

        private Func<T> _item = () => { throw new Exception("Item value retieval function not set! SET with 'SetItem();'."); };
        private readonly string _name;

        private SerializedProperty _inspectedProperty;

        public bool IsDragging { get => isDragging; set => isDragging = value; }
        public bool IsReadyForDrag { get => isReadyForDrag; set => isReadyForDrag = value; }

        public T Item { get { return _item.Invoke(); } }
        public string Name { get { return _name; } }
        public bool CanDrag { get => _canDrag; set => _canDrag = value; }
        public SerializedProperty InspectedProperty { get => _inspectedProperty; set => _inspectedProperty = value; }

        public event Action OnMoved = () => { }; 
        public event Action<VisualElement> OnCopied = (e) => { };


        public DragableElement(string name = null)
        {
            _name = name ?? _item?.ToString() ?? "Undefined";
        }

        public DragableElement(string name, Func<T> item) : this()
        {
            SetItem(item);
        }

        public DragableElement(Func<T> item) : this()
        {
            SetItem(item);
        }

        public void Remove()
        {
            OnMoved.Invoke();
        }

        /// <summary>
        /// Set function to retrieve Item Value, MUST be implemented!
        /// </summary>
        /// <param name="item"></param>
        public void SetItem(Func<T> item)
        {
            _item = item;
        }

        /// <summary>
        /// Set up events for Visual Element
        /// </summary>
        public void SetupDragEvents()
        {
            RegisterCallback<MouseUpEvent>(elem => {
                IsReadyForDrag = false;
                IsDragging = false;
            });


            RegisterCallback<MouseMoveEvent>((elem) => {
                if (IsReadyForDrag && !IsDragging)
                {
                    IsDragging = true;
                    DragAndDrop.PrepareStartDrag();
                    DragAndDrop.SetGenericData(typeof(DragableElement<T>).FullName, this);
                    DragAndDrop.StartDrag(Item.ToString());
                }
            });

            RegisterCallback<MouseDownEvent>((elem) => {
                if (!CanDrag) return;

                switch (elem.pressedButtons)
                {
                    case 1:
                        IsReadyForDrag = true;
                        break;
                }
                return;
            });
        }
    }
}
