using UnityEditor;
using UnityEngine.UIElements;

namespace Scripts.BodySystem.Editor
{
    [CustomEditor(typeof(BodyPart))]
    public class BodyPartEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            BodyPart bodyPart = (BodyPart)target;
            VisualElement root = new VisualElement();


            //Character character = bodyPart?.BodyBase?.GetComponent<Character>();


            Button setPosBtn = new Button(() =>
            {
                bodyPart.BodyBase.Body.RenderOrder.SetRenderTrasform(bodyPart.BodyBase.SideID, bodyPart.FlagID,
                    new BodyRenderOptions.RenderTrasform()
                    {
                        Position = bodyPart.transform.localPosition,
                        Rotation = bodyPart.transform.localRotation.eulerAngles,
                        Scale = bodyPart.transform.localScale
                    });
                    //bodyPart.Flag.relativeOrder = bodyPart.Renderer.sortingOrder;
                    EditorUtility.SetDirty(bodyPart.BodyBase.Body);
            })
            { text = "Set Transform for Body Part" };

            root.Add(setPosBtn);

            return root;
        }
    }
}
