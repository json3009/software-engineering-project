using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;
using Utilities.Collections;

namespace Scripts.BodySystem
{
    
    /// <summary>
    /// <para>The BodyRenderOptions is a data class holding settings and information on each bodyPart / layer.</para>
    /// <para>Holds data on:
    /// <list type="bullet">
    /// <item>
    /// <description>Render Order per Body Type</description>
    /// </item>
    /// <item>
    /// <description>Relative Position / Scale per Body Type</description>
    /// </item>
    /// </list>
    /// </para>
    /// </summary>
    [Serializable]
    public class BodyRenderOptions
    {
        [Serializable]
        private class LayersPerSide
        {
            // sort order - 
            //  SerializableGUID == GUID of Layer
            [SerializeField] private SerializableDictionary<SerializableGUID, RenderOptionsPerLayer> layers = new SerializableDictionary<SerializableGUID, RenderOptionsPerLayer>();

            // transforms
            //  SerializableGUID == GUID of Body Part
            [SerializeField] private SerializableDictionary<SerializableGUID, RenderTrasform> transforms = new SerializableDictionary<SerializableGUID, RenderTrasform>();

            /// <summary>
            /// Try to get Render options by <see cref="BodyLayer">layer</see> ID.
            /// </summary>
            /// <param name="id">The <see cref="BodyLayer">layer</see> ID.</param>
            /// <param name="render">The RenderOptionsPerLayer object.</param>
            /// <returns><b><c>TRUE</c></b>, if found; <b><c>FALSE</c></b> otherwise.</returns>
            public bool TryGetRenderOrder(SerializableGUID id, out RenderOptionsPerLayer render)
            {
                render = null;

                if (layers.ContainsKey(id))
                    render = layers[id];

                return render != null;
            }

            /// <summary>
            /// Get Render options by <see cref="BodyLayer">layer</see> ID.
            /// </summary>
            /// <param name="id">The <see cref="BodyLayer">layer</see> ID.</param>
            /// <returns>The RenderOptionsPerLayer object.</returns>
            public RenderOptionsPerLayer CreateOrGetRenderOrder(SerializableGUID id)
            {
                if (TryGetRenderOrder(id, out RenderOptionsPerLayer render))
                    return render;

                RenderOptionsPerLayer res = new RenderOptionsPerLayer();
                layers.Add(id, res);
                return res;
            }

            /// <summary>
            /// Try to get Render transform by <see cref="BodyLayer">layer</see> ID.
            /// </summary>
            /// <param name="id">The <see cref="BodyLayer">layer</see> ID.</param>
            /// <param name="transform">The transform object.</param>
            /// <returns><b><c>TRUE</c></b>, if found; <b><c>FALSE</c></b> otherwise.</returns>
            public bool TryGetRenderTransform(SerializableGUID id, out RenderTrasform transform)
            {
                transform = null;

                if (transforms.ContainsKey(id))
                    transform = transforms[id];

                return transform != null;
            }

            /// <summary>
            /// Get or Create a render transform object by <see cref="BodyLayer">layer</see> ID.
            /// </summary>
            /// <param name="id">The <see cref="BodyLayer">layer</see> ID.</param>
            /// <returns>The transform object.</returns>
            public RenderTrasform CreateOrGetRenderTransform(SerializableGUID id)
            {
                if (TryGetRenderTransform(id, out RenderTrasform transform))
                    return transform;

                RenderTrasform res = new RenderTrasform();
                transforms.Add(id, res);
                return res;
            }

            /// <summary>
            /// Set the render transform by <see cref="BodyLayer">layer</see> ID.
            /// </summary>
            /// <param name="id">The <see cref="BodyLayer">layer</see> ID.</param>
            /// <param name="transform">The Render transform</param>
            public void SetRenderTransform(SerializableGUID id, RenderTrasform transform)
            {
                if (transforms.ContainsKey(id))
                    transforms[id] = transform;
                else
                    transforms.Add(id, transform);
            }
        }

        
        [Serializable]
        public class RenderTrasform
        {
            [SerializeField] private Vector3 _position;
            [SerializeField] private Vector3 _rotation;
            [SerializeField] private Vector3 _scale;

            public Vector3 Position { get => _position; set => _position = value; }
            public Vector3 Rotation { get => _rotation; set => _rotation = value; }
            public Vector3 Scale { get => _scale; set => _scale = value; }
        }

        [Serializable]
        private class RenderOptionsPerLayer
        {
            // sort order - 
            //  SerializableGUID == GUID of Body Part
            //  int              == Sort Order (-1 = inherit sort order)
            [SerializeField] public SerializableDictionary<SerializableGUID, int> RenderOrder = new SerializableDictionary<SerializableGUID, int>();

            public int GetSortOrder(SerializableGUID id)
            {
                if (RenderOrder.ContainsKey(id))
                    return RenderOrder[id];
                
                return -1;
            }

            public void SetSortOrder(SerializableGUID id, int order)
            {
                if (RenderOrder.ContainsKey(id))
                {
                    if (order < 0)
                        RenderOrder.Remove(id);
                    else
                        RenderOrder[id] = order;
                }
                else if (order >= 0)
                    RenderOrder.Add(id, order);
            }
        }


        [SerializeField] private SerializableDictionary<SerializableGUID, LayersPerSide> sides = new SerializableDictionary<SerializableGUID, LayersPerSide>();


        /// <summary>
        /// Get Layer by the side GUID
        /// </summary>
        /// <param name="id">The id of the Side</param>
        /// <param name="layer">The found layer; NULL if not found</param>
        /// <returns>True, if found; False, otherwhise</returns>
        private bool TryGetLayer(SerializableGUID id, out LayersPerSide layer)
        {
            layer = null;

            if (sides.ContainsKey(id))
                layer = sides[id];

            return layer != null;
        }


        private LayersPerSide CreateOrGetLayer(SerializableGUID id)
        {
            if (TryGetLayer(id, out LayersPerSide layer))
                return layer;

            LayersPerSide res = new LayersPerSide();
            sides.Add(id, res);
            return res;
        }

        /// <summary>
        /// Get Sort order of a body part
        /// </summary>
        /// <param name="sideID">The GUID of the Side</param>
        /// <param name="layerID">The GUID of the layer</param>
        /// <param name="partID">The GUID of the part</param>
        /// <returns></returns>
        public int GetSortOrder(SerializableGUID sideID, SerializableGUID layerID, SerializableGUID partID)
        {
            if (!TryGetLayer(sideID, out LayersPerSide layer))
                return -1;

            if (!layer.TryGetRenderOrder(layerID, out RenderOptionsPerLayer renderOrders))
                return -1;

            return renderOrders.GetSortOrder(partID);
        }


        /// <summary>
        /// Get all PartID's and their respective sort order
        /// </summary>
        /// <param name="sideID">The GUID of the Side</param>
        /// <param name="layerID">The GUID of the layer</param>
        /// <returns></returns>
        public List<(SerializableGUID, int)> GetSortOrders(SerializableGUID sideID, SerializableGUID layerID)
        {
            if (!TryGetLayer(sideID, out LayersPerSide layer))
                return new List<(SerializableGUID, int)>();

            if (!layer.TryGetRenderOrder(layerID, out RenderOptionsPerLayer renderOrders))
                return new List<(SerializableGUID, int)>();

            return renderOrders.RenderOrder.Select((e) => { return (e.Key, e.Value); }).ToList();
        }

        /// <summary>
        /// Set the sort order of a body part
        /// </summary>
        /// <param name="sideID">The GUID of the Side</param>
        /// <param name="layerID">The GUID of the layer</param>
        /// <param name="partID">The GUID of the part</param>
        /// <param name="order">The sort order</param>
        public void SetSortOrder(SerializableGUID sideID, SerializableGUID layerID, SerializableGUID partID, int order)
        {
            CreateOrGetLayer(sideID).CreateOrGetRenderOrder(layerID).SetSortOrder(partID, order);
        }


        /// <summary>
        /// Get Render Transform of a body part
        /// </summary>
        /// <param name="sideID">The GUID of the Side</param>
        /// <param name="partID">The GUID of the part</param>
        /// <returns>The transform for the given part & side</returns>
        public RenderTrasform GetRenderTrasform(SerializableGUID sideID, SerializableGUID partID)
        {
            if (!TryGetLayer(sideID, out LayersPerSide layer))
                return null;

            if (!layer.TryGetRenderTransform(partID, out RenderTrasform renderTransform))
                return null;

            return renderTransform;
        }


        /// <summary>
        /// Set Render Transform for part per Side
        /// </summary>
        /// <param name="sideID">The GUID of the Side</param>
        /// <param name="partID">The GUID of the part</param>
        /// <param name="transform">The transform to set</param>
        public void SetRenderTrasform(SerializableGUID sideID, SerializableGUID partID, RenderTrasform transform)
        {
            CreateOrGetLayer(sideID).SetRenderTransform(partID, transform);
        }

        /// <summary>
        /// Set Render Transform position for part per Side
        /// </summary>
        /// <param name="sideID">The GUID of the Side</param>
        /// <param name="partID">The GUID of the part</param>
        /// <param name="pos">The position to set</param>
        public void SetRenderTrasformPosition(SerializableGUID sideID, SerializableGUID partID, Vector3 pos)
        {
            CreateOrGetLayer(sideID).CreateOrGetRenderTransform(partID).Position = pos;
        }

        /// <summary>
        /// Set Render Transform position for part per Side
        /// </summary>
        /// <param name="sideID">The GUID of the Side</param>
        /// <param name="partID">The GUID of the part</param>
        /// <param name="rot">The rotation to set</param>
        public void SetRenderTrasformRotation(SerializableGUID sideID, SerializableGUID partID, Vector3 rot)
        {
            CreateOrGetLayer(sideID).CreateOrGetRenderTransform(partID).Rotation = rot;
        }

        /// <summary>
        /// Set Render Transform position for part per Side
        /// </summary>
        /// <param name="sideID">The GUID of the Side</param>
        /// <param name="partID">The GUID of the part</param>
        /// <param name="scl">The scale to set</param>
        public void SetRenderTrasformScale(SerializableGUID sideID, SerializableGUID partID, Vector3 scl)
        {
            CreateOrGetLayer(sideID).CreateOrGetRenderTransform(partID).Scale = scl;
        }
    }
}
