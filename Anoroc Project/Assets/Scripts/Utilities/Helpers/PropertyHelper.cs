#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Utilities.Extensions
{
    public static class PropertyHelper
    {
        public static SerializedProperty GetArrayByPath(this SerializedProperty prop)
        {
            SerializedProperty currentProperty = prop.Copy();
            SerializedProperty siblingProperty = prop.Copy();

            if (prop.isArray)
                return prop;

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

        public static SerializedProperty GetArrayByPath(string prop, SerializedObject obj)
        {
            SerializedProperty property = obj.FindProperty(prop);

            Debug.Assert(property != null, $"Property '{prop}' not found!");
            return GetArrayByPath(property);
        }
    }
}

#endif