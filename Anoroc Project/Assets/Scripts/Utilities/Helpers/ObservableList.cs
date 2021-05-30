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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using Utilities.Extensions;

namespace Utilities.Collections
{
    /// <summary>
    /// Unity does not serialize Observable collection, thus the need for a custom class deriving from ObservableCollection
    /// </summary>
    /// <typeparam name="T">The type of collection</typeparam>

    [Serializable]
    public class ObservableList<T>: ObservableCollection<T>, ISerializationCallbackReceiver
    {
        /// <summary>
        /// Disposable Class which purpose, is to auto-enable and disable change notifications. 
        /// </summary>
        private class DisposableShouldNotify : IDisposable
        {
            private bool previousValue;
            private ObservableList<T> list;

            public DisposableShouldNotify(ObservableList<T> list)
            {
                this.previousValue = list.shouldNotify;
                this.list = list;
                this.list.shouldNotify = false;
            }

            public void Dispose()
            {
                list.shouldNotify = previousValue;
            }
        }

        #region fields
        [SerializeField]
        private T[] _list;

        public bool shouldNotify = true;
        #endregion

        #region Constructors
        public ObservableList() : base()
        {
            OnBeforeSerialize();
        }

        public ObservableList(IList<T> list) : base(list ?? new List<T>())
        {
            OnBeforeSerialize();
        }

        public ObservableList(IEnumerable<T> collection) : base (collection)
        {
            OnBeforeSerialize();
        }

        #endregion

        #region functions

        /// <summary>
        /// Ignore change until disposed!
        /// </summary>
        /// <example>
        /// using(IgnoreChange()){ code here }
        /// </example>
        /// <returns></returns>
        public IDisposable IgnoreChange()
        {
            return new DisposableShouldNotify(this);
        }

        /// <summary>
        /// Add range of items to collection.
        /// </summary>
        /// <param name="items">The items to add.</param>
        public void AddRange(IList<T> items)
        {
            int startIndex = Count - 1;

            using (IgnoreChange())
            {
                foreach (var item in items)
                {
                    Add(item);
                }
            }

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, (IList)items, startIndex));
        }


        public new int Move(int oldIndex, int newIndex)
        {
            T item = this[oldIndex];

            using (IgnoreChange())
                base.RemoveAt(oldIndex);

            var arg = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex);

            if (newIndex > oldIndex) newIndex--;

            using (IgnoreChange())
                base.Insert(newIndex, item);

            OnCollectionChanged(arg);

            return newIndex;
        }

        /// <summary>
        /// Remove At positions (can be an array of indicies)
        /// </summary>
        /// <param name="indicies">The indicies to remove!</param>
        public void RemoveAt(params int[] indicies)
        {

            using (IgnoreChange())
            {
                var toDelete = this
                    .WithIndex()
                    .Where((e) => indicies.Contains(e.index))
                    .Select((e) => e.item)
                    .ToArray();

                for (int i = 0; i < toDelete.Count(); i++)
                {
                    Remove(toDelete[i]);
                }
            }

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, (IList)indicies));
        }


        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (shouldNotify) base.OnCollectionChanged(e);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (shouldNotify) base.OnPropertyChanged(e);
        }


        // Serialize this collection
        public void OnBeforeSerialize()
        {
            _list = this.ToArray();
        }


        // Deserialize collection
        public void OnAfterDeserialize()
        {
            using (IgnoreChange())
            {
                this.Clear();
                this.AddRange(_list);
            }
        }

        #endregion
    }
}
