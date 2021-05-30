using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Utilities;

namespace Scripts.BodySystem.Editor
{
    [CustomEditor(typeof(BodyBase))]
    public class BodyBaseEditor : UnityEditor.Editor
    {
        BodyBase baseTarget;

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            baseTarget = (BodyBase)target;

            if (baseTarget.Body != null)
            {

                if (!baseTarget.Body.IsValid(out _))
                {
                    root.Add(new HelpBox($"There is an error in The Body Definition Asset [{baseTarget.Body.name}]!", HelpBoxMessageType.Error));
                    return root;
                }

                PopupField<BodySide> sideField = new PopupField<BodySide>("Side", baseTarget.Body.Sides, 0, (e) => e.name, (e) => e.name);
                if (baseTarget.SideID.Value.IsEmpty())
                    baseTarget.SideID = baseTarget.Body.Sides[0].id;

                sideField.value = baseTarget.GetSide();
                sideField.RegisterValueChangedCallback((e)=> { baseTarget.SideID = e.newValue.id; EditorUtility.SetDirty(target); });

                root.Add(sideField);

                foreach (var item in baseTarget.Body.GetAllBodyParts())
                {
                    if (item.Equals(BodyPartFlag.None))
                        continue;

                    ObjectField field = new ObjectField(item.name) { objectType = typeof(BodyPart), allowSceneObjects = true };
                    field.value = baseTarget.GetSlot((BodyPartFlag)item);

                    field.RegisterValueChangedCallback((e) =>
                    {
                        baseTarget.SetSlot((BodyPartFlag)item, (BodyPart)e.newValue);
                    });

                    root.Add(field);
                }
            }
            else
            {
                root.Add(new HelpBox("Body Definition not set!", HelpBoxMessageType.Warning));
            }

            return root;
        }
    }
}
