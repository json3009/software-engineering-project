using System.Collections.Generic;
using System.Linq;
using Scripts.BodySystem;
using UnityEditor;
using UnityEngine;
using Utilities;
using Utilities.Extensions;

namespace EquipmentSystem.Editor
{
    public class AutoImporter : UnityEditor.Editor
    {
        private static readonly string[] MainParts = new string[] {
            "Head",
            "Body",
            "LegR",
            "LegL",
            "ArmR",
            "ArmL",
            "HandR",
            "HandL",
            "FingersR",
            "FingersL",
            "SleeveL",
            "SleeveR"
        };

        private static readonly string[] SidesInTexture = new string[] {
            "Front",
            "Back",
            "Left"
        };

        // should really be automated!!!!!!
        private static readonly string[] SideInAsset = new string[]
        {
            "288bd822-cd91-4667-808a-995871ab1289",     // Front
            "b7754afe-55f2-4fa4-9197-cbbab19c3445",     // Back
            "fd326b7d-c939-43f9-bb7a-bece69a81e6d"      // Left
        };

        private static readonly string[] Slots = new string[] {
            "4630e305-2b3b-4bdc-be08-0f6c05045087",     // Helmet
            "5f67bfe0-294d-4ad8-9eab-b00b3d7afbfc",     // Torso
            "d1c2aadd-b733-46c3-a868-a4ce8ee221f9",     // Left Leg
            "88a5d502-d8cb-49d3-940f-48476e9f1340",     // Right Leg
            "99ac3314-543a-48fe-931b-0ffb4e4581aa",     // Left Arm
            "6e0acb0b-6a1e-441d-a24a-d25ecde46cb1",     // Right Arm
            "4de839b0-95f8-4a5e-9048-c9c4108e4be8",     // Left Hand
            "29cbb2d7-20d4-4355-82b5-525b7ba0241b",     // Right Hand
            "15ee508d-72cd-4fab-bd1a-1b25682ffe9c",     // Left Fingers
            "f80568b2-3d3a-42c3-b380-c5d8507e0095",     // Right Fingers
            "aafff783-e254-4e29-92a4-9d01bac7e5df",     // Left Sleeve
            "83857b88-8715-4f65-b355-26d251c30819"      // Right Sleeve
        };

        private const string EquipmentID =    "9893a39d-b622-4f95-a992-a1367b9f5a96"; // Equipment Slot ID
        private const string DefinitionGuid = "745ba23cdfb57e44e869237c63267edf";     // Body Definition Asset


        [MenuItem("Assets/Game/Equipment/Texture to Equipment")]
        private static void DoSomethingWithVariable()
        {
            Texture2D selectedTexture = Selection.activeObject as Texture2D;
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);

            if(importer.textureType != TextureImporterType.Sprite)
            {
                EditorUtility.DisplayDialog("Error", "Selected Texture is not set as a Sprite Type Texture!", "Close");
                return;
            }

            if(importer.spriteImportMode != SpriteImportMode.Multiple)
            {
                EditorUtility.DisplayDialog("Error", "Selected Sprite Texture is not set as a Multiple Sprite Texture!", "Close");
                return;
            }

            Dictionary<int, List<Sprite>> organizedSprites = new Dictionary<int, List<Sprite>>();

            string newPath = AssetDatabase.GUIDToAssetPath(AssetDatabase.CreateFolder(path.Substring(0, path.LastIndexOf("/")), selectedTexture.name));
            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();

            foreach (var part in MainParts.WithIndex())
            {
                var res = sprites.Where((e) => e.name.ToLower().Contains(part.item.ToLower())).ToList();
                if (res.Any() && !organizedSprites.ContainsKey(part.index))
                    organizedSprites.Add(part.index, res.ToList());
            }

            foreach (var part in organizedSprites)
            {
                var equipmentObject = ScriptableObject.CreateInstance<EquipmentItemSimple>();
                //equipmentObject.name = $"{selectedTexture.name}_{mainParts[part.Key]}";

                equipmentObject.LayerID = new SerializableGUID() { Value = EquipmentID };
                equipmentObject.EquipmentSlotID = new SerializableGUID() { Value = Slots[part.Key] };
                equipmentObject.Definition = AssetDatabase.LoadAssetAtPath<BodyDefinition>(AssetDatabase.GUIDToAssetPath(DefinitionGuid));

                foreach (var (item, index) in SidesInTexture.WithIndex())
                {
                    var usableSprite = part.Value.Find((e) => e.name.ToLower().Contains(item.ToLower()));
                    equipmentObject.Sprites.Add(new SerializableGUID() { Value = SideInAsset[index] }, usableSprite);

                    if (index == 0)
                        equipmentObject.Icon = usableSprite;
                }

                AssetDatabase.CreateAsset(equipmentObject, $"{newPath}/{selectedTexture.name}_{MainParts[part.Key]}.asset");
            }

        
        }

        [MenuItem("Assets/Game/Equipment/Texture to Equipment", true)]
        private static bool NewMenuOptionValidation()
        {
            return Selection.activeObject is Texture2D;
        }
    }
}
