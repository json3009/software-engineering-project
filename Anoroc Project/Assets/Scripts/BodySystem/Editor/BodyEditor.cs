using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


// Can not be done yet, reason being that unity does not yet fully support the necessary features 
//      features are designated as internal, thus can not be access without hacking the code!


/*
public class BodyEditor : EditorWindow
{
    private readonly string k_SplitterClassName = "unity-samples-explorer";
    private readonly int k_SplitterLeftPaneStartingWidth = 200;

    private readonly string k_TreeViewName = "tree-view";
    private readonly string k_ContentPanelName = "content-container";

    private readonly int k_TreeViewSelectionRestoreDelay = 400;
    private readonly int k_TreeViewInitialSelectionDelay = 500;


    private VisualElement m_ContentPanel;


    [MenuItem("Tools/Game/Body/Editor")]
    public static void Init()
    {
        EditorWindow.GetWindow(typeof(BodyEditor), false, "Body Editor");
    }



    internal class SampleTreeItem :  TreeViewItem<string>
    {
        private static int m_NextId = 0;

        public string name => data;

        public Func<SampleTreeItem, VisualElement> makeItem { get; private set; }

        public SampleTreeItem(string name, Func<SampleTreeItem, VisualElement> makeItem, List<TreeViewItem<string>> children = null)
            : base(m_NextId++, name, children)
        {
            this.makeItem = makeItem;
        }

        static public void ResetNextId()
        {
            m_NextId = 0;
        }
    }

    public void OnEnable()
    {
        var root = rootVisualElement;

        var styleSheet = EditorGUIUtility.Load(s_StyleSheetPath) as StyleSheet;
        root.styleSheets.Add(styleSheet);

        var themedStyleSheet = EditorGUIUtility.isProSkin
            ? EditorGUIUtility.Load(s_DarkStyleSheetPath) as StyleSheet
            : EditorGUIUtility.Load(s_LightStyleSheetPath) as StyleSheet;
        root.styleSheets.Add(themedStyleSheet);

        SampleTreeItem.ResetNextId();
        var items = new List<ITreeViewItem>()
            {
                new SampleTreeItem("Styles", StylesExplorer.Create),
                new SampleTreeItem("Button", ButtonSnippet.Create),
                new SampleTreeItem("Scroller", ScrollerSnippet.Create),
                new SampleTreeItem("Toggle", ToggleSnippet.Create),
                new SampleTreeItem("Label", LabelSnippet.Create),
                new SampleTreeItem("Text Field", TextFieldSnippet.Create),
                new SampleTreeItem("HelpBox", HelpBoxSnippet.Create),
                new SampleTreeItem("Object Field", ObjectFieldSnippet.Create),
                new SampleTreeItem("List View", ListViewSnippet.Create),
                new SampleTreeItem("Numeric Fields", MakeNumericFieldsPanel, new List<TreeViewItem<string>>()
                {
                    new SampleTreeItem("Integer", IntegerFieldSnippet.Create),
                    new SampleTreeItem("Float", FloatFieldSnippet.Create),
                    new SampleTreeItem("Long", LongFieldSnippet.Create),
                    new SampleTreeItem("MinMaxSlider", MinMaxSliderSnippet.Create),
                    new SampleTreeItem("Slider", SliderSnippet.Create),
                    new SampleTreeItem("Vector2", Vector2FieldSnippet.Create),
                    new SampleTreeItem("Vector3", Vector3FieldSnippet.Create),
                    new SampleTreeItem("Vector4", Vector4FieldSnippet.Create),
                    new SampleTreeItem("Rect", RectFieldSnippet.Create),
                    new SampleTreeItem("Bounds", BoundsFieldSnippet.Create),
                    new SampleTreeItem("SliderInt", SliderIntSnippet.Create),
                    new SampleTreeItem("Vector2Int", Vector2IntFieldSnippet.Create),
                    new SampleTreeItem("Vector3Int", Vector3IntFieldSnippet.Create),
                    new SampleTreeItem("RectInt", RectIntFieldSnippet.Create),
                    new SampleTreeItem("BoundsInt", BoundsIntFieldSnippet.Create)
                }),
                new SampleTreeItem("Value Fields", MakeValueFieldsPanel, new List<TreeViewItem<string>>()
                {
                    new SampleTreeItem("Color", ColorFieldSnippet.Create),
                    new SampleTreeItem("Curve", CurveFieldSnippet.Create),
                    new SampleTreeItem("Gradient", GradientFieldSnippet.Create)
                }),
                new SampleTreeItem("Choice Fields", MakeChoiceFieldsPanel, new List<TreeViewItem<string>>()
                {
                    new SampleTreeItem("Enum", EnumFieldSnippet.Create),
                    new SampleTreeItem("EnumFlags", EnumFlagsFieldSnippet.Create),
                    new SampleTreeItem("Popup", PopupFieldSnippet.Create),
                    new SampleTreeItem("Tag", TagFieldSnippet.Create),
                    new SampleTreeItem("Mask", MaskFieldSnippet.Create),
                    new SampleTreeItem("Layer", LayerFieldSnippet.Create),
                    new SampleTreeItem("LayerMask", LayerMaskFieldSnippet.Create)
                }),
            };

        Func<VisualElement> makeItem = () =>
        {
            var box = new VisualElement();
            box.AddToClassList(k_TreeItemClassName);

            var label = new Label();
            label.AddToClassList(k_TreeItemLabelClassName);

            box.Add(label);
            return box;
        };

        Action<VisualElement, ITreeViewItem> bindItem = (element, item) =>
        {
            (element.ElementAt(0) as Label).text = (item as SampleTreeItem).data;
            element.userData = item;
        };

        Action<IEnumerable<ITreeViewItem>> onSelectionChanged = selectedItems =>
        {
            var item = (SampleTreeItem)selectedItems.FirstOrDefault();
            if (item == null)
                return;

            m_ContentPanel.Clear();
            m_ContentPanel.Add(item.makeItem(item));
        };

        var treeView = new TreeView() { name = k_TreeViewName };
        treeView.AddToClassList(k_TreeViewClassName);
        m_ContentPanel = new VisualElement() { name = k_ContentPanelName };
        m_ContentPanel.AddToClassList(k_ContentPanelClassName);

        var splitter = new DebuggerSplitter();
        splitter.AddToClassList(k_SplitterClassName);
        splitter.leftPane.style.width = k_SplitterLeftPaneStartingWidth;
        root.Add(splitter);

        splitter.leftPane.Add(treeView);
        splitter.rightPane.Add(m_ContentPanel);

        treeView.viewDataKey = "samples-tree";
        treeView.itemHeight = 20;
        treeView.rootItems = items;
        treeView.makeItem = makeItem;
        treeView.bindItem = bindItem;
        treeView.onSelectionChange += onSelectionChanged;
        treeView.Refresh();

        // Force TreeView to call onSelectionChanged when it restores its own selection from view data.
        treeView.schedule.Execute(() =>
        {
            onSelectionChanged(treeView.selectedItems);
        }).StartingIn(k_TreeViewSelectionRestoreDelay);

        // Force TreeView to select something if nothing is selected.
        treeView.schedule.Execute(() =>
        {
            if (treeView.selectedItems.Count() > 0)
                return;

            treeView.SetSelection(0);

            // Auto-expand all items on load.
            foreach (var item in treeView.rootItems)
                treeView.ExpandItem(item.id);
        }).StartingIn(k_TreeViewInitialSelectionDelay);
    }


}

*/
