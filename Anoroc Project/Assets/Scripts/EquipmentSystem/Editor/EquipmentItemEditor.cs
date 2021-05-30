using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.BodySystem;
using CharacterSystem;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Utilities;

namespace EquipmentSystem.Editor
{
    public class EquipmentItemEditor : UnityEditor.Editor
    {
        #region Fields
        private VisualElement mainSection;
        private VisualElement settingsSection;

        private EquipmentItem equipment;

        #endregion

        #region Properties
        public bool IsValid { get; private set; } = false;

        #endregion

        #region Events

        public event Action OnUpdate;

        #endregion

        #region InspectorGUI

        public override VisualElement CreateInspectorGUI()
        {
            OnUpdate += SetEditorDirty;
            OnUpdate += UpdateSettingsSection;

            equipment = (EquipmentItem)target;
            VisualElement root = new VisualElement();

            Frame mainFrame = new Frame() { Label = "Main Settings" };

            ObjectField iconField = new ObjectField("Icon") { objectType = typeof(Sprite), value = equipment.Icon };
            iconField.RegisterValueChangedCallback((e) => { equipment.Icon = (Sprite)e.newValue; OnUpdate?.Invoke(); });

            ObjectField settingsField = new ObjectField("Body Type") { objectType = typeof(BodyDefinition), value = equipment.Definition };
            settingsField.RegisterValueChangedCallback((e) => { equipment.Definition = (BodyDefinition)e.newValue; equipment.LayerID = Guid.Empty; equipment.EquipmentSlotID = Guid.Empty; OnUpdate?.Invoke(); });

            TextField nameField = new TextField("Equipment Name");
            nameField.value = equipment.Name.IsEmpty() ? equipment.name : equipment.Name;
            nameField.RegisterValueChangedCallback<string>((e) => { equipment.Name = e.newValue; OnUpdate?.Invoke(); });

            mainFrame.Add(nameField);
            mainFrame.Add(iconField);
            mainFrame.Add(settingsField);

            mainSection = new VisualElement();

            mainFrame.Add(mainSection);

            settingsSection = new VisualElement();
            UpdateSettingsSection();

            mainFrame.Add(settingsSection);

            root.Add(mainFrame);

            return root;
        }

        /// <summary>
        /// Draw the default Inspector
        /// </summary>
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }

        private void UpdateSettingsSection()
        {
            settingsSection.Clear();

            if (HasErrors(out HelpBox[] errors))
            {
                foreach (var errorbox in errors)
                    settingsSection.Add(errorbox);

                IsValid = false;
                return;
            }

            IsValid = true;

            List<BodyPartFlag> parts = new List<BodyPartFlag>() { BodyPartFlag.None }.Concat(equipment.Definition.GetAllBodyParts()).ToList();
            PopupField<BodyPartFlag> bodyParts = new PopupField<BodyPartFlag>("Slot", parts, BodyPartFlag.None, (e) => e.name, (e) => e.name);

            if (equipment.EquipmentSlot != null)
                bodyParts.value = equipment.EquipmentSlot;

            bodyParts.RegisterCallback<ChangeEvent<BodyPartFlag>>((e) =>
            {
                equipment.EquipmentSlot = (BodyPartFlag)e.newValue;
                EditorUtility.SetDirty(target);
            });
            settingsSection.Add(bodyParts);

            var equipmentField = new PopupField<BodyLayer>("Layer", equipment.Definition.GetAllLayers().ToList(), 0, (e) => e.name, (e) => e.name);
            if (equipment.Layer == null)
                equipment.Layer = equipment.Definition.GetAllLayers()[0];

            equipmentField.value = equipment.Layer;


            equipmentField.RegisterValueChangedCallback<BodyLayer>((e) => { equipment.Layer = e.newValue; EditorUtility.SetDirty(equipment); });

            settingsSection.Add(equipmentField);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Test wether the equipment Object has errors
        /// </summary>
        /// <param name="errorHelpBoxes">VisualBoxes to represent the errors, if any</param>
        /// <returns>True, if an error has been found; False otherwhise</returns>
        private bool HasErrors(out HelpBox[] errorHelpBoxes)
        {
            List<HelpBox> elements = new List<HelpBox>();

            if (equipment.Definition == null)
            {
                elements.Add(new HelpBox("Equipment Settings Reference Required!", HelpBoxMessageType.Warning));
            }
            else
            {
                if (!equipment.Definition.IsValid(out _))
                {
                    elements.Add(new HelpBox($"There is an error in The Body Definition Asset [{equipment.Definition.name}]!", HelpBoxMessageType.Error));
                }
            }

            errorHelpBoxes = elements.ToArray();

            return elements.Count != 0;
        }


        /// <summary>
        /// Set the target object as dirty (properties have changed and must be saved)
        /// </summary>
        private void SetEditorDirty()
        {
            EditorUtility.SetDirty(equipment);
        }

        #endregion

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
            var obj = target as EquipmentItem;
            var icon = obj.Icon;

            if (icon == null)
                return null;

            var preview = AssetPreview.GetAssetPreview(icon);
            var final = new Texture2D(width, height);

            if (preview == null)
                return null;

            EditorUtility.CopySerialized(preview, final);

            return final;
        }


        /// <summary>
        /// Handle on drag event for equipment
        /// </summary>
        /// <param name="sceneView">The SceneView</param>
        /// <param name="index">The index</param>
        internal void OnSceneDrag(SceneView sceneView, int index)
        {
            Event e = Event.current;
            var obj = target as EquipmentItem;
            GameObject characterGameObject = HandleUtility.PickGameObject(e.mousePosition, false);

            if (characterGameObject == null)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                e.Use();
                return;
            }

            Character character = characterGameObject.GetComponentInChildren<Character>() ??
                characterGameObject.GetComponentInParent<Character>();

            if (character == null)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                e.Use();
                return;
            }

            if (e.type == EventType.DragUpdated)
            {
                if (!obj.IsValid() || !character.Equipment.CanEquip(obj))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                }
                else
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                }

                e.Use();
            }
            else if (e.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                e.Use();

                if (obj.IsValid() && character.Equipment.CanEquip(obj))
                {
                    if (!character.Equipment.Equip(obj))
                    {
                        Debug.LogError($"Could not equip '{obj}' onto '{character.gameObject}'!");
                    }
                }
            }
        }


    }
}
