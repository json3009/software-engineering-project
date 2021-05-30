using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Utilities.UI
{
    public class BoundList<T> : Frame
    {
        private readonly BoundListView<T> internalView;

        private VisualElement internalHeader;
        private Func<T> _createNewItem;

        public BoundList(BoundListView<T> internalView) : base()
        {

            this.internalView = internalView;

            if (string.IsNullOrEmpty(Label))
            {
                Label = ObjectNames.NicifyVariableName(typeof(T).Name);
            }

            string path = FileHelper.GetAssetDirectoryPathByType(typeof(BoundList<T>));
            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>($"{path}/BoundList.uss"));

            internalHeader = new VisualElement();
            internalHeader.AddToClassList("BoundListContainer_header");
            internalHeader.style.flexDirection = FlexDirection.Row;

            Header.Add(internalHeader);

            Build();
        }

        public Func<T> CreateNewItem { get => _createNewItem; set { _createNewItem = value; Build(); } }

        private void Build()
        {
            //this.Clear();
            Content.Clear();

            internalView.style.flexGrow = 1;
            
            Content.Add(internalView);

            UpdateHeader();
        }

        private void UpdateHeader()
        {
            internalHeader.Clear();

            if(_createNewItem != null)
            {
                Button add = new Button(() =>
                    {
                        var item = CreateNewItem.Invoke();
                        if(item == null)
                            return;
                        
                        internalView.CreateNewItem(item);
                    })
                { text = "+", tooltip = "Add new Item" };
                add.style.backgroundImage = new StyleBackground((Texture2D)EditorGUIUtility.IconContent("d_Toolbar Plus").image);

                internalHeader.Add(add);
            }

            if (internalView.CanDelete)
            {
                Button delete = new Button(() =>
                {
                    internalView.RemoveCurrentlySelected();
                })
                { text = "-", tooltip = "Delete currently selected Items" };
                delete.style.backgroundImage = new StyleBackground((Texture2D)EditorGUIUtility.IconContent("d_Toolbar Minus").image);
                internalHeader.Add(delete);
            }
        }
    }
}
