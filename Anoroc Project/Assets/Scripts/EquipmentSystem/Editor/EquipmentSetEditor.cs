using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Scripts.BodySystem;
using CharacterSystem;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.UIElements;
using Utilities;
using Utilities.Extensions;

namespace EquipmentSystem.Editor
{
    [CustomEditor(typeof(EquipmentSet))]
    public class EquipmentSetEditor : UnityEditor.Editor
    {
        private EquipmentSet _equipmentSet;
        ObjectField iconField;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }

        public override VisualElement CreateInspectorGUI()
        {
            _equipmentSet = (EquipmentSet) target;

            VisualElement root = new VisualElement();
            Frame mainFrame = new Frame() {Label = "Main Settings"};

            iconField = new ObjectField("Icon") {objectType = typeof(Sprite), value = _equipmentSet.Icon};
            iconField.RegisterValueChangedCallback((e) =>
            {
                if(_equipmentSet.Icon && AssetDatabase.IsSubAsset(_equipmentSet.Icon))
                    AssetDatabase.RemoveObjectFromAsset(_equipmentSet.Icon);
                
                if(_equipmentSet.GeneratedIcon && AssetDatabase.IsSubAsset(_equipmentSet.GeneratedIcon))
                    AssetDatabase.RemoveObjectFromAsset(_equipmentSet.GeneratedIcon);
                
                _equipmentSet.Icon = (Sprite) e.newValue; 
                EditorUtility.SetDirty(target);
            });

            TextField nameField = new TextField("Equipment Name")
                {value = _equipmentSet.Name.IsEmpty() ? _equipmentSet.name : _equipmentSet.Name};
            nameField.RegisterValueChangedCallback<string>((e) => { _equipmentSet.Name = e.newValue; EditorUtility.SetDirty(target); });

            Button genIconBtn = new Button(() =>
            {
                EditorCoroutineUtility.StartCoroutine(GenerateIconAsync(), this);
                
            }) {text = "Generate Icon"};
            mainFrame.Add(nameField);
            mainFrame.Add(iconField);
            mainFrame.Add(genIconBtn);

            PropertyField itemsField = new PropertyField(serializedObject.FindProperty("_items"));
            itemsField.Bind(serializedObject);

            root.Add(mainFrame);
            root.Add(itemsField);

            return root;
        }


        /// <summary>
        /// Draws the custom preview thumbnail for the asset in the Project window
        /// </summary>
        /// <param name="assetPath">Path of the asset</param>
        /// <param name="subAssets">Array of children assets</param>
        /// <param name="width">Width of the rendered thumbnail</param>
        /// <param name="height">Height of the rendered thumbnail</param>
        /// <returns></returns>
        public override Texture2D RenderStaticPreview(string assetPath, UnityEngine.Object[] subAssets, int width, int height)
        {
            var obj = target as EquipmentSet;
            var icon = obj.Icon;

            if (icon == null)
                return null;

            var preview = AssetPreview.GetAssetPreview(icon);
            var final = new Texture2D(width, height, TextureFormat.ARGB32, false);

            if (preview == null)
                return null;

            EditorUtility.CopySerialized(preview, final);

            return final;
        }
        
        private IEnumerator GenerateIconAsync()
        {
            if (_equipmentSet.Items.Values.Length <= 0)
                yield break;

            GameObject cameraObject = null;
            GameObject obj = null;

            try
            {
                var bodyDefinition = _equipmentSet.Items.Values.First().Definition;
                var bodyFactory = BodyFactory.CreateBodyFactory(bodyDefinition);
                obj = bodyFactory.CreateGameObject();
                obj.transform.position = new Vector3(-900, -900);

                Character character = obj.AddComponent<Character>();
                character.Definition = bodyDefinition;

                cameraObject = new GameObject("Camera");
                Camera camera = cameraObject.AddComponent<Camera>();
                camera.backgroundColor = new Color(1f, 1f, 1f, 0f);
                camera.orthographic = true;
                camera.orthographicSize = 0.5f;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.nearClipPlane = 0;
                camera.farClipPlane = 1;
                
                int ImageSize = 128;
                var cameraTargetTexture = new RenderTexture(ImageSize, ImageSize, 32, RenderTextureFormat.ARGB32);
                cameraTargetTexture.Create();
                camera.targetTexture = cameraTargetTexture;

                Vector2 usedBodyParts = Vector2.zero;
                character.Equipment.Equip(_equipmentSet);

                var spriteRenderers = obj.GetComponentsInChildren<SpriteRenderer>();
                foreach (var part in spriteRenderers)
                {
                    var position = part.transform.position;
                    usedBodyParts.x += position.x;
                    usedBodyParts.y += position.y;
                    
                    part.material = new Material(Shader.Find("Sprites/Default"));
                }

                usedBodyParts /= spriteRenderers.Length;
                cameraObject.transform.position = usedBodyParts;
                
                camera.Render();

                if(_equipmentSet.Icon && AssetDatabase.IsSubAsset(_equipmentSet.Icon))
                    AssetDatabase.RemoveObjectFromAsset(_equipmentSet.Icon);
                
                if(_equipmentSet.GeneratedIcon && AssetDatabase.IsSubAsset(_equipmentSet.GeneratedIcon))
                    AssetDatabase.RemoveObjectFromAsset(_equipmentSet.GeneratedIcon);

                var texture2D = cameraTargetTexture.ToTexture2D();
                texture2D.name = "_GeneratedTexture";
                AssetDatabase.AddObjectToAsset(texture2D, target);

                var equipmentSetIcon = Sprite.Create(texture2D, new Rect(Vector2.zero, new Vector2(cameraTargetTexture.width, cameraTargetTexture.height)), Vector2.zero, 100);
                equipmentSetIcon.name = "_GeneratedIcon";
                AssetDatabase.AddObjectToAsset(equipmentSetIcon, target);
                AssetDatabase.SaveAssets();

                _equipmentSet.GeneratedIcon = texture2D;
                _equipmentSet.Icon = equipmentSetIcon;
                iconField.SetValueWithoutNotify(equipmentSetIcon);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            } 
            
            if(cameraObject)
                DestroyImmediate(cameraObject);
            
            if(obj)
                DestroyImmediate(obj);
        }
    }

}
