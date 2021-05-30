using System;
using Unity.Collections;
using UnityEngine;

namespace PathfinderSystem
{
    [Serializable]
    public struct PathNode {

        #region Fields
        
        [SerializeField] private int _x;
        [SerializeField] private int _y;

        [SerializeField] private int _index;

        // Walking Cost from the start Node
        [SerializeField] private int _gCost;
        
        // Heuristic Cost to reach End Node
        [SerializeField] private int _hCost;
        
        // f = G + H
        [SerializeField] private int _fCost;

        // penalty to move on this node
        [SerializeField] private int _movementPenalty;

        // is this tile walkable
        [SerializeField] private bool _isWalkable;
        
        [SerializeField] private int _cameFromNodeIndex;

        [SerializeField] private NativeHeapIndex _heapIndex;

        #endregion

        #region Properties

        public int X
        {
            get => _x;
            set => _x = value;
        }

        public int Y
        {
            get => _y;
            set => _y = value;
        }

        public int Index
        {
            get => _index;
            set => _index = value;
        }

        public int GCost
        {
            get => _gCost;
            set => _gCost = value;
        }

        public int HCost
        {
            get => _hCost;
            set => _hCost = value;
        }

        public int FCost => _fCost;

        public int MovementPenalty
        {
            get => _movementPenalty;
            set => _movementPenalty = value;
        }

        public bool IsWalkable
        {
            get => _isWalkable;
            set => _isWalkable = value;
        }

        public int CameFromNodeIndex
        {
            get => _cameFromNodeIndex;
            set => _cameFromNodeIndex = value;
        }

        public NativeHeapIndex HeapIndex
        {
            get => _heapIndex;
            set => _heapIndex = value;
        }

        #endregion

        public PathNode(int x, int y, int index, bool isWalkable, int movementPenalty = 0) : this()
        {
            _x = x;
            _y = y;
            _index = index;
            _movementPenalty = movementPenalty;
            _isWalkable = isWalkable;
        }

        public void CalculateFCost() {
            _fCost = _gCost + _hCost;
        }
    }
}