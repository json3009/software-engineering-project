using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utilities.Attributes;
using Debug = UnityEngine.Debug;

namespace PathfinderSystem
{
    public class PathFinder : MonoBehaviour
    {

        [Serializable]
        private struct TileCost
        {
            [SerializeField] private TileBase _tile;
            [SerializeField] private int _cost;
            [SerializeField] private bool _isWalkable;

            public TileBase Tile => _tile;
            public int Cost => _cost;

            public bool IsWalkable => _isWalkable;
        }
        
        [Serializable]
        private class PathNodeDefinition
        {
            public int x;
            public int y;
            public int index;
            
            public int movementPenalty;

            public bool isWalkable;

        }

        [SerializeField] private bool _showPathCost = false;
        [SerializeField] private bool _showPathIndicies = false;
        
        [SerializeField] private Tilemap _tilemap;
        [SerializeField] private List<TileCost> _tileCosts = new List<TileCost>();
        [SerializeField] private GameObject _mapObjectsParent;


        [SerializeField] private List<PathNodeDefinition> _nodes;
        [SerializeField] private Gradient _gradiant;

        [SerializeField, HideInInspector] private int _minCost = 0;
        [SerializeField, HideInInspector] private int _maxCost = 0;

        [InspectorButton("CalculateNodes")] public bool _recalculateNodes;

        [SerializeField] private int _from = 0;
        [SerializeField] private int _to = 1;
        [InspectorButton("test")] public bool _test;

        [SerializeField] private Texture2D _overlayTexture;
        
        
        private int CalculateIndex(int x, int y)
        {
            return x + y * _tilemap.size.x;
        }

        #if UNITY_EDITOR
        public void test()
        {
            var from = _nodes[_from];
            var to = _nodes[_to];
            var r = FindPathGrid(new Vector2Int(from.x, from.y), new Vector2Int(to.x, to.y)).ToArray();

            for (int i = 0; i < r.Length-1; i++)
            {
                Debug.DrawLine(r[i], r[i+1], Color.red);   
            }
            
            SceneView.RepaintAll();
        }
        #endif


        public Stack<Vector2> FindPath(Vector2 start, Vector2 end)
        {
            return FindPath(
                new Vector2Int( (int)Math.Floor(start.x), (int)Math.Floor(start.y) ),
                new Vector2Int( (int)Math.Floor(end.x), (int)Math.Floor(end.y) )
            );
        }

        public Stack<Vector2> FindPath(Vector2Int start, Vector2Int end)
        {
            var origin = _tilemap.origin;
            
            start.x -= origin.x;
            start.y -= origin.y;
            
            end.x -= origin.x;
            end.y -= origin.y;
            
            return FindPathGrid(start, end);
        }
        public Stack<Vector2> FindPathGrid(Vector2Int start, Vector2Int end)
        {
            NativeArray<PathNode> nodes = new NativeArray<PathNode>(_nodes.Count, Allocator.TempJob);
            
            var origin = _tilemap.origin;
            var cellSize = _tilemap.cellSize;
            var size = _tilemap.size;

            foreach (PathNodeDefinition pathNode in _nodes)
            {
                PathNode node = new PathNode
                {
                    X = pathNode.x,
                    Y = pathNode.y,
                    Index = pathNode.index,
                    
                    IsWalkable = pathNode.isWalkable,
                    MovementPenalty = pathNode.movementPenalty,
                    
                    GCost = int.MaxValue,
                    CameFromNodeIndex = -1,
                    
                };
                nodes[pathNode.index] = node;
            }

            if (!nodes[CalculateIndex(end.x, end.y)].IsWalkable)
            {
                
            }

            var result = new NativeList<int>(Allocator.TempJob);

            //Stopwatch watch = Stopwatch.StartNew();
            FindPathJob job = new FindPathJob
            {
               StartPosition = new int2(start.x, start.y),
               EndPosition = new int2(end.x, end.y),
               GridSize = new int2(size.x, size.y),
               PathNodes = nodes,
               Result = result
            };

            var jobHandle = job.Schedule();
            jobHandle.Complete();
            //Debug.Log(watch.ElapsedMilliseconds);
            //watch.Stop();

            Stack<Vector2> resultList = new Stack<Vector2>(job.Result.Length);
            foreach (var pathNode in job.Result)
            {
                resultList.Push(new Vector2(
                    _nodes[pathNode].x + origin.x + (cellSize.x / 2),
                    _nodes[pathNode].y + origin.y + (cellSize.y / 2))
                );
            }

            job.Result.Dispose();
            
            nodes.Dispose();
            return resultList;
        }
        
        private void CalculateNodes()
        {
            _tilemap.CompressBounds();
            
            var size = _tilemap.size;
            var origin = _tilemap.origin;

            _nodes = new List<PathNodeDefinition>(size.x * size.y);

            Dictionary<TileBase, TileCost> tileCostsDict = new Dictionary<TileBase, TileCost>(_tileCosts.Count);
            foreach (var costs in _tileCosts)
            {
                _maxCost = Math.Max(_maxCost, costs.Cost);
                _minCost = Math.Min(_minCost, costs.Cost);
                
                tileCostsDict.Add(costs.Tile, costs);
            }
            
            for (var y = 0; y < size.y; y++)
            {
                for (var x = 0; x < size.x; x++)
                {
                    TileBase t = _tilemap.GetTile(new Vector3Int(x + origin.x, y + origin.y,0));
                    
                    PathNodeDefinition node = new PathNodeDefinition()
                    {
                        x = x,
                        y = y,
                        index = CalculateIndex(x, y),
                        isWalkable = true,
                        movementPenalty = 0
                    };

                    
                    if (t != null && tileCostsDict.TryGetValue(t, out TileCost cost))
                    {
                        node.isWalkable = cost.IsWalkable;
                        node.movementPenalty = cost.Cost;
                    }
                    
                    _nodes.Add(node);
                }
            }

            if (_mapObjectsParent && _mapObjectsParent.activeInHierarchy)
            {
                var origin1 = _tilemap.origin;
                var componentsInChildren = _mapObjectsParent.GetComponentsInChildren<Collider2D>(false);
                foreach (var collider1 in componentsInChildren)
                {
                    Bounds bounds = collider1.bounds;

                    for (int cellY = (int)Math.Floor(bounds.min.y); cellY < (int)Math.Ceiling(bounds.max.y); cellY++)
                    {
                        for (int cellX = (int)Math.Floor(bounds.min.x); cellX < (int)Math.Ceiling(bounds.max.x); cellX++)
                        {
                            int x = cellX - origin1.x;
                            int y = cellY - origin1.y;
                            
                            if(!IsPositionInsideGrid(x, y))
                                continue;

                            var index = CalculateIndex(x, y);
                            if(index > _nodes.Count)
                                continue;

                            var pathNode = _nodes[index];
                            pathNode.isWalkable = false;
                        }   
                    }
                }
            }

            foreach (var node in _nodes)
            {
                if (node.isWalkable)
                    continue;

                foreach (var neighbourIndex in GetAllNeighbours(node))
                {
                    var pathNodeDefinition = _nodes[neighbourIndex];
                    
                    if (pathNodeDefinition.isWalkable && pathNodeDefinition.movementPenalty < _maxCost)
                        pathNodeDefinition.movementPenalty += 10;
                }
            }

            if (_overlayTexture != null)
            {
                // remove texture from asset
            }

            Texture2D pathCostOverlay = new Texture2D(_tilemap.size.x, _tilemap.size.y, TextureFormat.RGBA32, false);
            pathCostOverlay.filterMode = FilterMode.Point;
            
            foreach (var node in _nodes)
            {
                var color = _gradiant.Evaluate(node.isWalkable
                    ? NormalizeValue(_minCost, _maxCost, (int) (node.movementPenalty * 0.8f))
                    : 1);
                
                pathCostOverlay.SetPixel(node.x, _tilemap.size.y - node.y -1, new Color(color.r, color.g, color.b,node.isWalkable ? 0.4f : 0.85f));
            }
            
            pathCostOverlay.Apply();

            _overlayTexture = pathCostOverlay;

            
            
        }

        private List<int> GetAllNeighbours(PathNodeDefinition node)
        {
            List<int> allNeighbours = new List<int>(9);
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if(!IsPositionInsideGrid(node.x + x, node.y + y)) continue;
                    
                    allNeighbours.Add(CalculateIndex(node.x + x, node.y + y));
                }
            }

            return allNeighbours;
        }

        public bool IsPositionWalkable(Vector2 pos)
        {
            var origin = _tilemap.origin;
            var t = new Vector2Int(((int) Math.Floor(pos.x)) - origin.x, ((int) Math.Floor(pos.y)) - origin.y);
            return _nodes[CalculateIndex(t.x, t.y)].isWalkable; 
        }
        
        private bool IsPositionInsideGrid(int x, int y)
        {
            var size = _tilemap.size;
            return
                x >= 0 && 
                y >= 0 &&
                x < size.x &&
                y < size.y;
        }
        
        
        #if UNITY_EDITOR
        private void DrawGrid()
        {
            var origin = _tilemap.origin;
            
            if(!_showPathCost && !_showPathIndicies)
                return;

            if (_showPathCost && _overlayTexture)
            {
                Gizmos.DrawGUITexture(new Rect(origin.x, origin.y, _tilemap.size.x, _tilemap.size.y),
                    _overlayTexture);
            }

            var cellSize = _tilemap.cellSize;
            

            foreach (var node in _nodes)
            {

                if (_showPathIndicies)
                {
                    Handles.Label(new Vector3(
                            node.x + (cellSize.x) + origin.x,
                            node.y + (cellSize.y) + origin.y),
                        node.index.ToString()
                    );
                }
            }

        }
        
        void OnDrawGizmos() 
        {
            if(_nodes != null)
                DrawGrid();
        }
        #endif

        private static float NormalizeValue(int min, int max, int value)
        {
            if (value > max) return 1;
            if (value < min) return 0;
            
            return (float)(value - min) / (float)(max - min);
        }
        

    }
}