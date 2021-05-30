using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Utilities;

namespace Scripts.BodySystem
{
    /// <summary>
    /// The BodyFactory class allows a the generation of a given body type.
    /// </summary>
    /// <example>
    /// <code>
    ///var bodyFactory = BodyFactory.CreateBodyFactory(humanBodyStructure);
    ///var generatedBody = bodyFactory.CreateGameObject();
    ///
    ///var resultStructure = bodyFactory.PrintNodeStructure();
    ///
    /// /*====== resultStructure ======
    ///Root
    ///| 	Head
    ///| 	| 	Beard
    ///| 	| 	Mouth
    ///| 	| 	Eyes
    ///| 	| 	| 	EyeBrows
    ///| 	| 	Ears
    ///| 	| 	| 	EarL
    ///| 	| 	| 	EarR
    ///| 	| 	Hair
    ///| 	| 	Helmet
    ///| 	Torso
    ///| 	| 	Left Leg
    ///| 	| 	Right Leg
    ///| 	| 	Left Arm
    ///| 	| 	| 	Left Hand
    ///| 	| 	| 	| 	Left Fingers
    ///| 	| 	| 	Left Sleeve
    ///| 	| 	Right Arm
    ///| 	| 	| 	Right Hand
    ///| 	| 	| 	| 	Right Fingers
    ///| 	| 	| 	Right Sleeve
    /// */
    /// </code>
    /// </example>
    public class BodyFactory
    {
        private class Node
        {
            public SerializableGUID bodyPartID;
            public Node parent;

            private List<Node> children = new List<Node>();

            public List<Node> Children { get => children; }

            public void AddChild(Node node)
            {
                children.Add(node);
            }
        }

        private BodyDefinition definition;

        private Node root;

        /// <summary>
        /// Create a new BodyFactory Instance.
        /// </summary>
        /// <param name="definition">The Body definition to use.</param>
        /// <returns>The BodyFactory.</returns>
        public static BodyFactory CreateBodyFactory(BodyDefinition definition)
        {
            BodyFactory bd = new BodyFactory
            {
                definition = definition
            };

            bd.root = bd.BuildBodyStructure();
            return bd;
        }

        /// <summary>
        /// Print the Node Structure.
        /// </summary>
        public void PrintNodeStructure()
        {
            Debug.Log(PrintNodeStructure(root, 0));
        }
        
        private string PrintNodeStructure(Node n, int depth = 0)
        {
            string res = (depth > 0?"\n":"") + "| \t".Repeat(depth) + definition.GetPartByID(n.bodyPartID).name;
            
            foreach (var item in n.Children)
                res += PrintNodeStructure(item, depth + 1);

            return res;
        }

        /// <summary>
        /// Create a gameObject from the generated structure.
        /// </summary>
        /// <returns>The generated gameObject.</returns>
        public GameObject CreateGameObject()
        {
            GameObject RootGameObject = new GameObject(definition.name);
            SortingGroup group = RootGameObject.AddComponent<SortingGroup>();
            group.sortingOrder = 1;

            foreach (var side in definition.Sides)
            {
                GameObject sideRoot = new GameObject(side.name);
                if (!side.Equals(definition.DefaultSide))
                    sideRoot.SetActive(false);

                BodyBase equipmentBase = sideRoot.AddComponent<BodyBase>();
                equipmentBase.Body = definition;
                equipmentBase.SideID = side.id;

                GameObject o = CreateGameObject_Child(root, equipmentBase, side.id);
                o.transform.SetParent(sideRoot.transform);

                sideRoot.transform.SetParent(RootGameObject.transform);
            }
            
            return RootGameObject;
        }


        private GameObject CreateGameObject_Child(Node parentNode, BodyBase equipmentBase, SerializableGUID sideID)
        {
            BodyPartFlag bodyPartFlag = definition.GetPartByID(parentNode.bodyPartID);

            BodyRenderOptions.RenderTrasform renderTrasform =
                definition.RenderOrder.GetRenderTrasform(sideID, bodyPartFlag.id);

            GameObject parentGameObject = new GameObject(bodyPartFlag.name);
            if (renderTrasform != null)
            {
                parentGameObject.transform.position = renderTrasform.Position;
                parentGameObject.transform.rotation = Quaternion.Euler(renderTrasform.Rotation);
                parentGameObject.transform.localScale = renderTrasform.Scale;
            }

            BodyPart partComponent = parentGameObject.AddComponent<BodyPart>();
            partComponent.BodyBase = equipmentBase;
            partComponent.FlagID = parentNode.bodyPartID;

            equipmentBase.SetSlot(bodyPartFlag, partComponent);

            foreach (var child in parentNode.Children)
            {
                GameObject childGameObject = CreateGameObject_Child(child, equipmentBase, sideID);
                childGameObject.transform.SetParent(parentGameObject.transform, false);
            }

            return parentGameObject;
        }

        public Texture2D CreateIcon()
        {
            return null;
        }

        private Node BuildBodyStructure(BodyPartFlag bodyPart)
        {
            Node node = new Node() { bodyPartID = bodyPart.id };

            if (bodyPart.Children != null)
            {
                foreach (var item in bodyPart.Children)
                {
                    node.AddChild(BuildBodyStructure(item));
                }
            }

            return node;
        }

        private Node BuildBodyStructure()
        {
            Node node = new Node
            {
                bodyPartID = definition.RootBodyPart.id
            };

            if (definition.BodyParts.Count > 0)
            {
                foreach (var item in definition.BodyParts)
                {
                    node.AddChild(BuildBodyStructure(item));
                }
            }


            return node;
        }

    }
}
