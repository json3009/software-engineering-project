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
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Utilities.Extensions
{
    public static class UIElementsHelper
    {

		/// <summary>
		/// Send a change event down & up the hierarchy chain 
		/// </summary>
		/// <typeparam name="T">The type of chnaged element</typeparam>
		/// <param name="element">The Visual element that has changed</param>
		/// <param name="previous">The previous value of the item</param>
		/// <param name="current">The new value of the item</param>
		public static void SendChangeEvent<T>(this VisualElement element, T previous, T current)
		{
            using var changeEvent = ChangeEvent<T>.GetPooled(previous, current);
            changeEvent.target = element;
            element.SendEvent(changeEvent);
        }


		/// <summary>
		/// Add Stylesheet to element
		/// </summary>
		/// <param name="element">The visualElement to add stylesheet to.</param>
		/// <param name="filename">The file name</param>
		public static void AddUss(this VisualElement element, string filename, [CallerFilePath] string callerFilename = "")
		{
			var file = $"{FileHelper.GetAssetPath(callerFilename) ?? "PATHERROR/"}{filename}.uss" ;
			StyleSheet sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(file);
			if (sheet != null)
				element.styleSheets.Add(sheet);
			else
				Debug.LogError($"Could not find stylesheet! [Path: '{file}']");
		}

		/// <summary>
		/// Add UXML to element
		/// </summary>
		/// <param name="element">The visualElement to clone the tree structure to.</param>
		/// <param name="filename">The file name</param>
		public static void AddUxml(this VisualElement element, string filename, [CallerFilePath] string callerFilename = "")
		{
			var file = $"{FileHelper.GetAssetPath(callerFilename) ?? "PATHERROR/"}{filename}.uxml";
			VisualTreeAsset tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(file);
			if (tree != null)
				tree.CloneTree(element);
			else
				Debug.LogError($"Could not find UXML! [Path: '{file}']");
		}


		/*
		public static VisualElement GetDefaultField(Type t, string label, object value = null)
        {
			VisualElement result = null;
            if (t.Equals(typeof(int)))
            {
				IntegerField intField = new IntegerField(label)
            }

			ObjectField

			return result;
        }
		
		*/

	}
}
