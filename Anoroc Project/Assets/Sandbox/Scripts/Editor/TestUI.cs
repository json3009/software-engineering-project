using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


using Sandbox;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[CustomEditor(typeof(UITestScript))]
public class TestUI : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = new VisualElement();

        Frame test = new Frame() { Label = "Hello World" };

        TextField f = new TextField("My Field");

        var property = new PropertyField(serializedObject.GetIterator().Copy());
        property.Bind(serializedObject);

        test.Add(property);
        test.Add(f);

        root.Add(test);

        return root;
    }
}
