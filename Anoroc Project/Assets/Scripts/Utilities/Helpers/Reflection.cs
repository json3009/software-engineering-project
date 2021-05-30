/*
    MIT License

    Copyright (c) 2021 Marx Jason

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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using System.Collections;
using System.Text.RegularExpressions;

using UnityEditor;
using UnityEngine;

using Object = UnityEngine.Object;

namespace Utilities
{
    /// <summary>
    /// Helper Class for Reflection type of operations.
    /// </summary>
    public static class Reflection
    {
        private static readonly Regex arrayMatch = new Regex(@".Array.data\[([0-9]*)\]");
        private static readonly Regex arrayContentMatch = new Regex(@"\[([0-9]*)\]");

        #region Get Types
        /// <summary>
        /// Gets the type with the given name.
        /// </summary>
        /// <param name="name">The type name.</param>
        /// <returns>The type</returns>
        public static Type GetType(string name) { return GetType(name, AppDomain.CurrentDomain.GetAssemblies()); }

        /// <summary>
        /// Gets the type with the given name.
        /// </summary>
        /// <param name="name">The type name.</param>
        /// <param name="assemblies">The assemblies in which to look for the type</param>
        /// <returns>The type</returns>
        public static Type GetType(string name, Assembly[] assemblies)
        {
            var type = Type.GetType(name);

            if (type != null)
                return type;

            foreach (Assembly assembly in assemblies)
            {
                type = assembly.GetType(name);
                if (type != null)
                    return type;
            }
            return null;
        }

        /// <summary>
        /// Gets all the types and assignable types with the given type.
        /// </summary>
        /// <typeparam name="T">The Type</typeparam>
        /// <returns>The list of Types</returns>
        public static IEnumerable<Type> GetAllTypes<T>() { return GetAllTypes<T>(AppDomain.CurrentDomain.GetAssemblies()); }

        /// <summary>
        /// Gets all the types and assignable types with the given type.
        /// </summary>
        /// <typeparam name="T">The Type</typeparam>
        /// <param name="assemblies">The assemblies in which to look for the type</param>
        /// <returns>The list of Types</returns>
        public static IEnumerable<Type> GetAllTypes<T>(Assembly[] assemblies)
        {
            var parent = typeof(T);

            if (assemblies.Count() == 0) return new List<Type>();

            return assemblies.SelectMany(assembly => {
                return assembly.GetTypes().Where(type => {
                    return !(type.IsAbstract || type.IsInterface) && parent.IsAssignableFrom(type);
                });
            });
        }

        #endregion

        #region Create Instances
        /// <summary>
        /// Creates an instance of type 'T'.
        /// </summary>
        /// <typeparam name="T">The type to be instanced</typeparam>
        /// <returns>The instance of the object</returns>
        public static T CreateInstance<T>(params object[] parameters)
        {
            return (T)CreateInstance(typeof(T), parameters);
        }

        /// <summary>
        /// Creates an instance of the given type name.
        /// </summary>
        /// <param name="name">The type name to be instanced</param>
        /// <returns>The instance of the object</returns>
        public static object CreateInstance(string name, params object[] parameters) { return CreateInstance(name, parameters, AppDomain.CurrentDomain.GetAssemblies()); }

        /// <summary>
        /// Creates an instance of the given type name.
        /// </summary>
        /// <param name="name">The type name to be instanced</param>
        /// <param name="assemblies">The assemblies in which to look for the type</param>
        /// <returns>The instance of the object</returns>
        public static object CreateInstance(string name, Assembly[] assemblies, params object[] parameters)
        {
            Type assmblyLoaderType = GetType(name, assemblies);
            if (assmblyLoaderType == null) return null;

            return CreateInstance(assmblyLoaderType, parameters);
        }

        /// <summary>
        /// Creates an instance of the given type.
        /// </summary>
        /// <param name="type">The type to be instanced</param>
        /// <typeparam name="T">The type that is expected</typeparam>
        /// <returns>The instance of the object</returns>
        public static T CreateInstance<T>(Type type, params object[] parameters) { return (T)CreateInstance(type, parameters); }

        /// <summary>
        /// Creates an instance of the given type.
        /// </summary>
        /// <param name="type">The type to be instanced</param>
        /// <returns>The instance of the object</returns>
        public static object CreateInstance(Type type, params object[] parameters)
        {
            return Activator.CreateInstance(type, parameters);
        }


        #endregion

        #region Property Helpers (Serialization & Reflection)

        /// <summary>
        /// Get Field and value from given path 
        /// </summary>
        /// <param name="o">The object to get the field from</param>
        /// <param name="path">The path to the field</param>
        /// <returns>The searched Field; NULL if not found</returns>
        public static (FieldInfo, dynamic) GetFieldAndValue(object o, string path)
        {

            string[] fullPath = arrayMatch.Replace(path, ".[$1]").Split('.');
            FieldInfo field = null;

            foreach (string item in fullPath)
            {
                Match m = arrayContentMatch.Match(item);
                if (m.Success && m.Groups[1].Success)
                {
                    var enm = ((IEnumerable)o).GetEnumerator();
                    for (int i = 0; i <= int.Parse(m.Groups[1].Value); i++)
                    {
                        if (!enm.MoveNext())
                            throw new IndexOutOfRangeException($"Index ({i}) is out of bounds!");
                    }

                    o = enm.Current;
                }
                else
                {
                    field = o.GetType().GetAllFields().Where((e) => e.Name.Equals(item)).FirstOrDefault();

                    if (field == null)
                        return (null, null);

                    o = field.GetValue(o);
                }
            }

            return (field, o);
        }

        #if UNITY_EDITOR

        /// <summary>
        /// Get Field and value from given property 
        /// </summary>
        /// <param name="property">The property to get the field and value of</param>
        /// <returns>The searched Field; NULL if not found</returns>
        public static (FieldInfo, dynamic) GetFieldAndValue(this SerializedProperty property)
        {
            return GetFieldAndValue(property.serializedObject.targetObject, property.propertyPath);
        }

        /// <summary>
        /// Get Field from serialized object, can get inner fields and array indicies
        /// </summary>
        /// <param name="obj">The Serialized Object</param>
        /// <param name="prop">The field to get</param>
        /// <returns>The field</returns>
        public static FieldInfo GetField(this SerializedObject obj, string prop)
        {
            return obj.FindProperty(prop)?.GetField();
        }

        /// <summary>
        /// Get Field from property path, can get inner fields and array indicies
        /// </summary>
        /// <param name="prop"></param>
        /// <returns>The field</returns>
        public static FieldInfo GetField(this SerializedProperty prop)
        {
            if (prop == null) return default;

            prop.serializedObject.Update();

            var obj = GetFieldAndValue(prop.serializedObject.targetObject, prop.propertyPath);

            if (obj.Item1 == null)
                throw new KeyNotFoundException($"Could not find field {prop.propertyPath}!");

            return obj.Item1;
        }

        /// <summary>
        /// Get value of property, can get inner fields and array indicies.
        /// </summary>
        /// <param name="prop"></param>
        /// <returns>The value of the field</returns>
        public static T GetValue<T>(this SerializedProperty prop)
        {
            if (prop == null) return default;

            prop.serializedObject.Update();

            var obj = GetFieldAndValue(prop.serializedObject.targetObject, prop.propertyPath);

            if (obj.Item1 == null)
                throw new KeyNotFoundException($"Could not find field {prop.propertyPath}!");

            if (obj.Item2 == null)
                return default;

            return (T)obj.Item2;
        }

        /// <summary>
        /// Set Value of a property
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="prop"></param>
        /// <param name="value"></param>
        public static void SetValue<T>(this SerializedProperty prop, T value)
        {
            if (prop == null || value == null) return;

            prop.serializedObject.Update();

            var obj = GetFieldAndValue(prop.serializedObject.targetObject, prop.propertyPath);

            if (obj.Item1 == null)
                throw new KeyNotFoundException($"Could not find field {prop.propertyPath}!");

            if (!obj.Item1.FieldType.Equals(typeof(T)))
                throw new ArgumentException($"Given Value Type ({typeof(T)}) does not equal the field type ({obj.Item1.FieldType})!");


            switch (prop.propertyType)
            {
                case SerializedPropertyType.AnimationCurve:
                    prop.animationCurveValue = value as AnimationCurve;
                    break;
                case SerializedPropertyType.ArraySize:
                    prop.arraySize = (int)(object)value;
                    break;
                case SerializedPropertyType.Boolean:
                    prop.boolValue = (bool)(object)value;
                    break;
                case SerializedPropertyType.Bounds:
                    prop.boundsValue = (Bounds)(object)value;
                    break;
                case SerializedPropertyType.BoundsInt:
                    prop.boundsIntValue = (BoundsInt)(object)value;
                    break;
                case SerializedPropertyType.Character:
                    throw new System.InvalidOperationException("Can not handle Character types.");
                case SerializedPropertyType.Color:
                    prop.colorValue = (Color)(object)value;
                    break;
                case SerializedPropertyType.Enum:
                    throw new System.InvalidOperationException("Can not handle enum types.");
                case SerializedPropertyType.ExposedReference:
                    prop.exposedReferenceValue = value as Object;
                    break;
                case SerializedPropertyType.Float:
                    prop.floatValue = (float)(object)value;
                    break;
                case SerializedPropertyType.Gradient:
                    throw new System.InvalidOperationException("Can not handle Gradient types.");
                case SerializedPropertyType.Integer:
                    prop.intValue = (int)(object)value;
                    break;
                case SerializedPropertyType.LayerMask:
                    prop.intValue = (value is LayerMask) ? ((LayerMask)(object)value).value : (int)(object)value;
                    break;
                case SerializedPropertyType.ManagedReference:
                    prop.managedReferenceValue = value;
                    break;
                case SerializedPropertyType.ObjectReference:
                    prop.objectReferenceValue = value as Object;
                    break;
                case SerializedPropertyType.Quaternion:
                    prop.quaternionValue = (Quaternion)(object)value;
                    break;
                case SerializedPropertyType.Rect:
                    prop.rectValue = (Rect)(object)value;
                    break;
                case SerializedPropertyType.RectInt:
                    prop.rectIntValue = (RectInt)(object)value;
                    break;
                case SerializedPropertyType.String:
                    prop.stringValue = (string)(object)value;
                    break;
                case SerializedPropertyType.Vector2:
                    prop.vector2Value = (Vector2)(object)value;
                    break;
                case SerializedPropertyType.Vector2Int:
                    prop.vector2IntValue = (Vector2Int)(object)value;
                    break;
                case SerializedPropertyType.Vector3:
                    prop.vector3Value = (Vector3)(object)value;
                    break;
                case SerializedPropertyType.Vector3Int:
                    prop.vector3IntValue = (Vector3Int)(object)value;
                    break;
                case SerializedPropertyType.Vector4:
                    prop.vector4Value = (Vector4)(object)value;
                    break;
            }

            prop.serializedObject.ApplyModifiedProperties();

        }

        /// <summary>
        /// Get all Attributes given the requested type "<typeparamref name="T"/>" from a property
        /// </summary>
        /// <typeparam name="T">The Searched Attribute</typeparam>
        /// <param name="property">The property to get all attributes from</param>
        /// <returns>An array of Attributes</returns>
        public static T[] GetAttributes<T>(SerializedProperty property) where T : Attribute
        {
            return GetField(property).GetCustomAttributes(typeof(T), true).Cast<T>().ToArray();
        }


        public static void PrintAll(this SerializedObject obj, bool recursive = true)
        {
            SerializedProperty property = obj.GetIterator();
            SerializedProperty currentProperty = property.Copy();

            string result = "Printing Property:";

            while (currentProperty.Next(recursive))
            {
                result += ("\n\t" + currentProperty.propertyPath);
            }
            Debug.Log(result);
        }

        #endif

#endregion

        #region General Reflection Helper Functions

        /// <summary>
        /// Get all Fields in type (private and public)
        /// </summary>
        /// <param name="type">The type to get all fields from</param>
        /// <param name="flags">The flags to get field by</param>
        /// <returns>The fields</returns>
        public static FieldInfo[] GetAllFields(this Type type, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
        {
            return type.GetFields(flags);
        }

        /// <summary>
        /// Get Field  in type by filter
        /// </summary>
        /// <param name="type">The type to get all fields from</param>
        /// <param name="filter"></param>
        /// <returns>The field</returns>
        public static FieldInfo GetAllField(this Type type, string filter)
        {
            return GetAllFields(type)
                .Where((e)=>e.Name == filter)
                .FirstOrDefault();
        }

        #endregion

        #region Enum Helpers

        /// <summary>
        /// Get all values of an Enum Type
        /// </summary>
        /// <typeparam name="T">The Enum to get values from</typeparam>
        /// <returns>An array of the Enum values</returns>
        public static IEnumerable<T> GetEnumValues<T>() {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
        
        #endregion
    }
}
