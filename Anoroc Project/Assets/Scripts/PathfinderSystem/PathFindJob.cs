/*
 * SOURCE: https://www.youtube.com/watch?v=ubUPVu_DeVk&list=RDCMUCFK6NCbuCIVzA6Yj1G_ZqCg&start_radio=1&t=13s&ab_channel=CodeMonkey
 * Modified for this projects purposes 
 */


using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace PathfinderSystem
{
    [BurstCompile]
    internal struct FindPathJob : IJob
    {
        private const int MOVE_STRAIGHT_COST = 10;
        private const int MOVE_DIAGONAL_COST = 14;
        
        private int2 _startPosition;
        private int2 _endPosition;

        private int2 _gridSize;
        private NativeArray<PathNode> _pathNodes;

        private NativeList<int> _result;

        private int _closestNode;

        public int2 StartPosition
        {
            get => _startPosition;
            set => _startPosition = value;
        }
        
        public int2 EndPosition
        {
            get => _endPosition;
            set => _endPosition = value;
        }
        
        public int2 GridSize
        {
            get => _gridSize;
            set => _gridSize = value;
        }
        
        public NativeArray<PathNode> PathNodes
        {
            get => _pathNodes;
            set => _pathNodes = value;
        }

        public NativeList<int> Result
        {
            get => _result;
            set => _result = value;
        }

        public void Execute()
        {
            if (!IsPositionInsideGrid(_startPosition, _gridSize) || !IsPositionInsideGrid(_endPosition, _gridSize))
                return;
            
            NativeArray<int2> neighbourOffsetArray = new NativeArray<int2>(8, Allocator.Temp);
            neighbourOffsetArray[0] = new int2(-1, 0); // Left
            neighbourOffsetArray[1] = new int2(+1, 0); // Right
            neighbourOffsetArray[2] = new int2(0, +1); // Up
            neighbourOffsetArray[3] = new int2(0, -1); // Down
            neighbourOffsetArray[4] = new int2(-1, -1); // Left Down
            neighbourOffsetArray[5] = new int2(-1, +1); // Left Up
            neighbourOffsetArray[6] = new int2(+1, -1); // Right Down
            neighbourOffsetArray[7] = new int2(+1, +1); // Right Up
            
            PathNode startNode = _pathNodes[CalculateIndex(_startPosition.x, _startPosition.y, _gridSize.x)];
            startNode.GCost = 0;
            startNode.CalculateFCost();
            _pathNodes[startNode.Index] = startNode;

            _closestNode = -1;


            int bestHCost = CalculateDistanceCost(new int2(startNode.X, startNode.Y), _endPosition);
            for (int i = 0; i < _pathNodes.Length; i++) {
                PathNode pathNode = _pathNodes[i];
                pathNode.HCost = CalculateDistanceCost(new int2(pathNode.X, pathNode.Y), _endPosition);
                pathNode.CameFromNodeIndex = -1;

                if (pathNode.IsWalkable && pathNode.HCost != 0 && bestHCost > pathNode.HCost)
                {
                    bestHCost = pathNode.HCost;
                    _closestNode = pathNode.Index;
                }

                _pathNodes[i] = pathNode;
            }


            int endNodeIndex = CalculateIndex(_endPosition.x, _endPosition.y, _gridSize.x);
            

            NativeList<int> openList = new NativeList<int>(Allocator.Temp);
            //NativeHeap<PathNode, PathNodeCompare> openList = new NativeHeap<PathNode, PathNodeCompare>(Allocator.Temp);
            NativeList<int> closedList = new NativeList<int>(Allocator.Temp);

            //openList.Insert(startNode);
            openList.Add(startNode.Index);

            //openList.Insert(startNode);
            
            while (openList.Length > 0) {
                int currentNodeIndex = GetLowestCostFNodeIndex(openList, _pathNodes);
                PathNode currentNode = _pathNodes[currentNodeIndex];
                //PathNode currentNode = GetLowestCostFNodeIndex(openList);
                //int currentNodeIndex = currentNode.Index;
                
                if (currentNodeIndex == endNodeIndex) {
                    // Reached our destination!
                    break;
                }

                // Remove current node from Open List
                for (int i = 0; i < openList.Length; i++) {
                    if (openList[i] == currentNodeIndex) {
                        openList.RemoveAtSwapBack(i);
                        break;
                    }
                }
                

                closedList.Add(currentNodeIndex);

                for (int i = 0; i < neighbourOffsetArray.Length; i++) {
                    int2 neighbourOffset = neighbourOffsetArray[i];
                    int2 neighbourPosition = new int2(currentNode.X + neighbourOffset.x, currentNode.Y + neighbourOffset.y);

                    if (!IsPositionInsideGrid(neighbourPosition, _gridSize)) {
                        // Neighbour not valid position
                        continue;
                    }

                    int neighbourNodeIndex = CalculateIndex(neighbourPosition.x, neighbourPosition.y, _gridSize.x);

                    if (closedList.Contains(neighbourNodeIndex)) {
                        // Already searched this node
                        continue;
                    }

                    PathNode neighbourNode = _pathNodes[neighbourNodeIndex];
                    if (!neighbourNode.IsWalkable) {
                        // Not walkable
                        continue;
                    }

                    int2 currentNodePosition = new int2(currentNode.X, currentNode.Y);

	                int tentativeGCost = currentNode.GCost + CalculateDistanceCost(currentNodePosition, neighbourPosition) + neighbourNode.MovementPenalty;
	                if (tentativeGCost < neighbourNode.GCost) {
		                neighbourNode.CameFromNodeIndex = currentNodeIndex;
		                neighbourNode.GCost = tentativeGCost;
		                neighbourNode.CalculateFCost();
		                _pathNodes[neighbourNodeIndex] = neighbourNode;

                        //openList.Insert(neighbourNode);
                        if (!openList.Contains(neighbourNode.Index)) {
                            openList.Add(neighbourNode.Index);
                        }
                    }

                }
            }

            if(_pathNodes[endNodeIndex].CameFromNodeIndex != -1)
                CalculatePath(_pathNodes, _pathNodes[endNodeIndex]);
            else if(_closestNode > 0)
                CalculatePath(_pathNodes, _pathNodes[_closestNode]);

            neighbourOffsetArray.Dispose();
            openList.Dispose();
            closedList.Dispose();
        }

        private void CalculatePath(NativeArray<PathNode> pathNodeArray, PathNode endNode) {
            if (endNode.CameFromNodeIndex == -1) {
                // Couldn't find a path!
            } else {
                // Found a path
                _result.Add(endNode.Index);

                PathNode currentNode = endNode;
                while (currentNode.CameFromNodeIndex != -1) {
                    PathNode cameFromNode = pathNodeArray[currentNode.CameFromNodeIndex];
                    _result.Add(cameFromNode.Index);
                    currentNode = cameFromNode;
                }
            }
        }

        private static bool IsPositionInsideGrid(int2 gridPosition, int2 gridSize) {
            return
                gridPosition.x >= 0 && 
                gridPosition.y >= 0 &&
                gridPosition.x < gridSize.x &&
                gridPosition.y < gridSize.y;
        }

        private static int CalculateIndex(int x, int y, int gridWidth) {
            return x + y * gridWidth;
        }

        private static int CalculateDistanceCost(int2 aPosition, int2 bPosition) {
            int xDistance = math.abs(aPosition.x - bPosition.x);
            int yDistance = math.abs(aPosition.y - bPosition.y);
            int remaining = math.abs(xDistance - yDistance);
            return MOVE_DIAGONAL_COST * math.min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
        }

        
        private static PathNode GetLowestCostFNodeIndex(NativeHeap<PathNode, PathNodeCompare> openList) {
            return openList.Pop();
        }
        
        private static int GetLowestCostFNodeIndex(NativeList<int> openList, NativeArray<PathNode> pathNodeArray) {
            PathNode lowestCostPathNode = pathNodeArray[openList[0]];
            for (int i = 1; i < openList.Length; i++) {
                PathNode testPathNode = pathNodeArray[openList[i]];
                if (testPathNode.FCost < lowestCostPathNode.FCost) {
                    lowestCostPathNode = testPathNode;
                }
            }
            return lowestCostPathNode.Index;
        }


        private struct PathNodeCompare : IComparer<PathNode> {
            public int Compare(PathNode a, PathNode b) {
                float distForA = a.FCost;
                float distForB = b.FCost;
                return distForA.CompareTo(distForB);
            }
        }
        
    }
}