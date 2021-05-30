using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Utilities.UI;

namespace Scripts.BodySystem.Editor
{
    [CustomEditor(typeof(BodyDefinition))]
    public class BodyDefinitionEditor : UnityEditor.Editor
    {

        private static class HelpBoxes
        {
            public static readonly HelpBox errorUnknown = new HelpBox("Unknown Error!", HelpBoxMessageType.Error);
            public static readonly HelpBox errorNoBodyPartsDefined = new HelpBox("No body parts have been defined!", HelpBoxMessageType.Error);
            public static readonly HelpBox errorNoLayers = new HelpBox("No layers have been created, consider adding at least one!", HelpBoxMessageType.Error);
            public static readonly HelpBox errorNoSides = new HelpBox("No sides have been created, consider adding at least one!", HelpBoxMessageType.Error);
        }


        BodyDefinition definition;

        public static bool IsValid(BodyDefinition def, out VisualElement[] errorVisuals)
        {
            List<VisualElement> errors = new List<VisualElement>();
            if (!def.IsValid(out BodyDefinition.BodyDefinitionErrorCode[] codes))
            {
                foreach (var code in codes)
                {
                    switch (code)
                    {
                        case BodyDefinition.BodyDefinitionErrorCode.NoChildren:
                            errors.Add(HelpBoxes.errorNoBodyPartsDefined);
                            break;

                        case BodyDefinition.BodyDefinitionErrorCode.NoLayers:
                            errors.Add(HelpBoxes.errorNoLayers);
                            break;

                        case BodyDefinition.BodyDefinitionErrorCode.NoSides:
                            errors.Add(HelpBoxes.errorNoSides);
                            break;
                        default:
                            errors.Add(HelpBoxes.errorUnknown);
                            break;
                    }

                }
            }

            errorVisuals = errors.ToArray();
            return errors.Count == 0;
        }

        public override VisualElement CreateInspectorGUI()
        {
            definition = (BodyDefinition)serializedObject.targetObject;
            VisualElement root = new VisualElement();

            PropertyField field = new PropertyField(serializedObject.FindProperty("_bodyParts"));
            field.Bind(serializedObject);
            root.Add(field);

            BoundListView<BodyLayer> layersList = new BoundListView<BodyLayer>(serializedObject.FindProperty("layers"), NewItemLayer);
            //layersList.style.minHeight = 100;
            BoundList<BodyLayer> layerListContainer = new BoundList<BodyLayer>(layersList)
            {
                Label = "Body Layers",
                CreateNewItem = () => new BodyLayer() { id = Guid.NewGuid() }
            };

            BoundListView<BodySide> sidesList = new BoundListView<BodySide>(serializedObject.FindProperty("sides"), NewItemSide);
            BoundList<BodySide> sidesListContainer = new BoundList<BodySide>(sidesList)
            {
                Label = "Body Sides",
                CreateNewItem = () => new BodySide() { id = Guid.NewGuid() }
            };


            root.Add(sidesListContainer);
            root.Add(layerListContainer);

            if (!IsValid(definition, out VisualElement[] errorVisuals))
            {
                foreach (var err in errorVisuals)
                {
                    root.Add(err);
                }

                return root;
            }


            PopupField<BodySide> defaultSideField = new PopupField<BodySide>("Default Side", definition.Sides, 0, (e) => e.name, (e) => e.name);
            if (definition.DefaultSide == null)
                definition.DefaultSide = definition.Sides[0];

            defaultSideField.value = definition.DefaultSide;
            defaultSideField.RegisterValueChangedCallback((e) => { definition.DefaultSide = e.newValue; EditorUtility.SetDirty(target); });

            root.Add(defaultSideField);

            PopupField<BodyLayer> defaultLayerField = new PopupField<BodyLayer>("Default Layer", definition.GetAllLayers(), 0, (e) => e.name, (e) => e.name);
            if (definition.DefaultLayer == null)
                definition.DefaultLayer = definition.GetAllLayers()[0];

            defaultLayerField.value = definition.DefaultLayer;
            defaultLayerField.RegisterValueChangedCallback((e) => { definition.DefaultLayer = e.newValue; EditorUtility.SetDirty(target); });

            root.Add(defaultLayerField);


            root.Add(CreateRenderOrder());

            Button btnCreateBody = new Button(() =>
            {
                BodyFactory bodyFactory = BodyFactory.CreateBodyFactory(definition);
                GameObject b = bodyFactory.CreateGameObject();
            })
            { text = "Create Body Structure" };
            
            Button btnPrintNodeStructure = new Button(() =>
            {
                BodyFactory bodyFactory = BodyFactory.CreateBodyFactory(definition);
                bodyFactory.PrintNodeStructure();
            })
            { text = "Print Body Structure" };


            root.Add(btnCreateBody);
            root.Add(btnPrintNodeStructure);

            return root;
        }

        private VisualElement NewItemSide(BodySide item, int index, SerializedProperty prop)
        {
            TextField field = new TextField("Name");
            field.bindingPath = prop.FindPropertyRelative("name").propertyPath;
            field.Bind(prop.serializedObject);

            return field;
        }

        private VisualElement NewItemLayer(BodyLayer item, int index, SerializedProperty prop)
        {

            TextField field = new TextField("Name");
            field.bindingPath = prop.FindPropertyRelative("name").propertyPath;
            field.Bind(prop.serializedObject);

            return field;
        }

        private VisualElement CreateRenderOrder()
        {
            Foldout container = new Foldout() { text = "Render Order", value = false };


            foreach (var side in definition.Sides)
            {
                Foldout sideFoldout = new Foldout() { text = side.name, value = false };

                Foldout transformFoldout = new Foldout() { text = "Transforms", value = false };

                foreach (var bodyPart in definition.GetAllBodyParts())
                {
                    Foldout bodyPartFoldout = new Foldout() { text = bodyPart.name, value = false };

                    Vector3Field pos = new Vector3Field("Position");
                    Vector3Field rot = new Vector3Field("Rotation");
                    Vector3Field scl = new Vector3Field("Scale");

                    BodyRenderOptions.RenderTrasform renderTrasform = definition.RenderOrder.GetRenderTrasform(side.id, bodyPart.id);
                    if (renderTrasform != null)
                    {
                        pos.value = renderTrasform.Position;
                        rot.value = renderTrasform.Rotation;
                        scl.value = renderTrasform.Scale;
                    }

                    pos.RegisterValueChangedCallback((e) => { definition.RenderOrder.SetRenderTrasformPosition(side.id, bodyPart.id, e.newValue); EditorUtility.SetDirty(target); });
                    rot.RegisterValueChangedCallback((e) => { definition.RenderOrder.SetRenderTrasformRotation(side.id, bodyPart.id, e.newValue); EditorUtility.SetDirty(target); });
                    scl.RegisterValueChangedCallback((e) => { definition.RenderOrder.SetRenderTrasformScale(side.id, bodyPart.id, e.newValue); EditorUtility.SetDirty(target); });

                    bodyPartFoldout.Add(pos);
                    bodyPartFoldout.Add(rot);
                    bodyPartFoldout.Add(scl);

                    transformFoldout.Add(bodyPartFoldout);
                }


                Foldout sortOrderFoldout = new Foldout() { text = "Sort Orders", value = false };

                sideFoldout.Add(transformFoldout);
                sideFoldout.Add(sortOrderFoldout);

                foreach (var layer in definition.GetAllLayers())
                {

                    Foldout layerFoldout = new Foldout() { text = layer.name, value = false };



                    foreach (var item in definition.GetAllBodyParts())
                    {
                        IntegerField field = new IntegerField(item.name)
                        {
                            value = definition.RenderOrder.GetSortOrder(side.id, layer.id, item.id),
                        };
                        field.RegisterValueChangedCallback((e) =>
                        {
                            if (e.newValue == e.previousValue)
                                return;

                            definition.RenderOrder.SetSortOrder(side.id, layer.id, item.id, e.newValue);
                                //layer.SetSortOrder(item.id, e.newValue);
                                EditorUtility.SetDirty(definition);
                        });

                        layerFoldout.Add(field);
                    }


                    sortOrderFoldout.Add(layerFoldout);
                }

                container.Add(sideFoldout);
            }



            return container;
        }



    }
}
