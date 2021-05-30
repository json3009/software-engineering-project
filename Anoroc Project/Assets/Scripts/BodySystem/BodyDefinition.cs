using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;

namespace Scripts.BodySystem
{

    /// <summary>
    /// Data Structure that determines how a Body should work.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Body/New Body Type")]
    public class BodyDefinition : ScriptableObject
    {
        public enum BodyDefinitionErrorCode
        {
            /// <summary>No Children have been defined.</summary>
            NoChildren,

            /// <summary>No Body Sides have been defined.</summary>
            NoSides,

            /// <summary>No Body Layers have been defined.</summary>
            NoLayers
        }

        #region Fields

        [SerializeField] private BodyPartFlag _bodyParts = new BodyPartFlag() { name = "Root" };

        [SerializeField] private List<BodySide> sides = new List<BodySide>();
        [SerializeField] private List<BodyLayer> layers = new List<BodyLayer>();
        [SerializeField] private SerializableGUID _defaultLayer;
        [SerializeField] private SerializableGUID _defaultSide;

        [SerializeField] private BodyRenderOptions renderOrder = new BodyRenderOptions();

        #endregion

        #region Properties
        /// <summary>Gets all *direct* <see cref="BodyPartFlag">BodyPart</see> children from root.</summary>
        [SerializeField] public List<BodyPartFlag> BodyParts { get => _bodyParts.Children; }

        /// <summary>Gets root <see cref="BodyPartFlag">BodyPart</see>.</summary>
        [SerializeField] public BodyPartFlag RootBodyPart { get => _bodyParts; }

        private Lazy<Dictionary<SerializableGUID, BodyPartFlag>> BodyPartsByID => new Lazy<Dictionary<SerializableGUID, BodyPartFlag>>(() => Init_LazyLoad_BodyPartsByID());


        /// <summary>
        /// Get All Body Sides.
        /// </summary>
        public List<BodySide> Sides { get => sides; }

        /// <summary>
        /// Get the Render Order.
        /// </summary>
        public BodyRenderOptions RenderOrder { get => renderOrder; }

        /// <summary>
        /// The default Layer to be used on the Body.
        /// </summary>
        public BodyLayer DefaultLayer { get => layers.Find((e) => e.id.Equals(_defaultLayer)); set => _defaultLayer = value.id; }

        /// <summary>
        /// The default Side to be used on the Body.
        /// </summary>
        public BodySide DefaultSide { get => sides.Find((e) => e.id.Equals(_defaultSide)); set => _defaultSide = value.id; }

        #endregion

        #region Methods

        /// <summary>
        /// Checks if the Body Definition is valid or not. Outputs an array of errors.
        /// </summary>
        /// <param name="errNr">The array of found problems.</param>
        /// <returns>True, if valid; False, otherwhise.</returns>
        public bool IsValid(out BodyDefinitionErrorCode[] errNr)
        {
            List<BodyDefinitionErrorCode> errors = new List<BodyDefinitionErrorCode>();
            if (_bodyParts.Children == null || _bodyParts.Children.Count == 0)
                errors.Add(BodyDefinitionErrorCode.NoChildren);

            if (sides.Count == 0)
                errors.Add(BodyDefinitionErrorCode.NoSides);

            if (layers.Count == 0)
                errors.Add(BodyDefinitionErrorCode.NoLayers);

            errNr = errors.ToArray();
            return errors.Count == 0;
        }

        #endregion

        #region Accessor Methods

        /// <summary>
        /// Get the Body Part by ID.
        /// </summary>
        /// <param name="id">The ID of the Part to retrieve.</param>
        /// <returns>The Body Part, if found; <see cref="BodyPartFlag.None">None</see>, if not found. </returns>
        public BodyPartFlag GetPartByID(SerializableGUID id)
        {
            if (BodyPartsByID.Value.TryGetValue(id, out BodyPartFlag part))
                return part;

            return BodyPartFlag.None;
        }

        /// <summary>
        /// Get all Body parts (recursively).
        /// </summary>
        /// <returns>The List of BodyParts.</returns>
        public List<BodyPartFlag> GetAllBodyParts()
        {
            var partlist = GetAllBodyParts(_bodyParts.Children);
            partlist.Add(_bodyParts);

            return partlist;
        }

        /// <summary>
        /// Get Layer from provided GUID
        /// </summary>
        /// <param name="guid">The GUID of the Layer.</param>
        /// <returns>The Layer if found; Null, otherwhise.</returns>
        public BodyLayer GetLayer(SerializableGUID guid)
        {
            if (guid.Value.IsEmpty() || guid.Equals(Guid.Empty))
                return null;

            BodyLayer layer = layers.Where((e) => e.id.Equals(guid)).FirstOrDefault();

            return layer;
        }

        /// <summary>
        /// Get the index of the layer.
        /// </summary>
        /// <param name="layer">The Layer to retrieve the index from.</param>
        /// <returns>The index if found; -1, otherwhise.</returns>
        public int GetLayerIndex(BodyLayer layer)
        {
            return layers.IndexOf(layer);
        }

        /// <summary>
        /// Get the index of the layer by ID.
        /// </summary>
        /// <param name="layer">The Layer ID to retrieve the index from.</param>
        /// <returns>The index if found; -1, otherwhise.</returns>
        public int GetLayerIndex(SerializableGUID layerID)
        {
            return GetLayerIndex(GetLayer(layerID));
        }

        /// <summary>
        /// Get the layer by the index.
        /// </summary>
        /// <param name="index">The index to get the layer from.</param>
        /// <returns>The Body Part, if found; Null, otherwhise.</returns>
        public BodyLayer GetLayerByIndex(int index)
        {
            if (index < 0 || index >= layers.Count)
                return null;

            return layers[index];
        }

        /// <summary>
        /// Get all the layers.
        /// </summary>
        /// <returns>The list of layers.</returns>
        public List<BodyLayer> GetAllLayers()
        {
            return layers;
        }

        /// <summary>
        /// Add a layer.
        /// </summary>
        /// <param name="layer">The layer to add.</param>
        /// <returns>True, if layer could be added; False, otherwhise.</returns>
        public bool AddLayer(BodyLayer layer)
        {
            if (layer == null)
                return false;

            if (layer.id.Value.IsEmpty() || layer.id.Equals((SerializableGUID)Guid.Empty))
                return false;

            if (layers.Exists((e) => e.id.Equals(layer.id)))
                return false;

            layers.Add(layer);
            return true;
        }

        /// <summary>
        /// Remove a layer by ID.
        /// </summary>
        /// <param name="layerID">The Layer ID to remove</param>
        /// <returns>True, if layer could be removed; False, otherwhise.</returns>
        public bool RemoveLayer(SerializableGUID layerID)
        {
            return RemoveLayer(GetLayer(layerID));
        }

        /// <summary>
        /// Remove a layer.
        /// </summary>
        /// <param name="layer">The Layer to remove</param>
        /// <returns>True, if layer could be removed; False, otherwhise.</returns>
        public bool RemoveLayer(BodyLayer layer)
        {
            if (layer == null)
                return false;

            return layers.Remove(layer);
        }

        /// <summary>
        /// Get Side from provided GUID.
        /// </summary>
        /// <param name="guid">The Side ID.</param>
        /// <returns>The Body Side, if found; Null, otherwhise.</returns>
        public BodySide GetSide(SerializableGUID guid)
        {
            if (guid.Value.IsEmpty() || guid.Equals(Guid.Empty))
                return null;

            BodySide side = sides.Where((e) => e.id.Equals(guid)).FirstOrDefault();

            return side;
        }

        /// <summary>
        /// Add a side.
        /// </summary>
        /// <param name="side">The side to add.</param>
        /// <returns>True, if side could be added; False, otherwhise.</returns>
        public bool AddSide(BodySide side)
        {
            if (side == null)
                return false;

            if (side.id.Value.IsEmpty() || side.id.Equals(Guid.Empty))
                return false;

            if (sides.Exists((e) => e.id.Equals(side.id)))
                return false;

            sides.Add(side);
            return true;
        }

        /// <summary>
        /// Remove a side by ID.
        /// </summary>
        /// <param name="sideID">The Side ID to remove</param>
        /// <returns>True, if side could be removed; False, otherwhise.</returns>
        public bool RemoveSide(SerializableGUID sideID)
        {
            return RemoveSide(GetSide(sideID));
        }

        /// <summary>
        /// Remove a side.
        /// </summary>
        /// <param name="sideID">The Side to remove</param>
        /// <returns>True, if side could be removed; False, otherwhise.</returns>
        public bool RemoveSide(BodySide sideID)
        {
            if (sideID == null)
                return false;

            return sides.Remove(sideID);
        }

        #endregion

        private Dictionary<SerializableGUID, BodyPartFlag> Init_LazyLoad_BodyPartsByID()
        {
            Dictionary<SerializableGUID, BodyPartFlag> parts = new Dictionary<SerializableGUID, BodyPartFlag>();

            foreach (var item in GetAllBodyParts())
            {
                parts.Add(item.id, item);
            }

            return parts;
        }


        private static List<BodyPartFlag> GetAllBodyParts(List<BodyPartFlag> parts)
        {
            List<BodyPartFlag> bodyParts = new List<BodyPartFlag>();

            if (parts == null)
                return new List<BodyPartFlag>();

            foreach (var item in parts)
            {
                bodyParts.Add(item);
                bodyParts.AddRange(GetAllBodyParts(item.Children));
            }
            return bodyParts;
        }
    }
}
