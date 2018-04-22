using DG.Tweening;
using Finegamedesign.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Finegamedesign.LudumDare41
{
    [Serializable]
    public sealed class MatchBlockGrid
    {
        public float cellSize = 1f;
        public float cellCenter = 0.5f;
        public float snapZ = 1f;
        public int numColumns = 0;
        public int numRows = 0;
        public int numCells = 0;
        public Vector2 min;
        public Vector2 max;
        public MatchBlock[] grid;
        public bool selectEnabled = false;
        public bool simulationEnabled = true;
        public int numRowsInSet = 2;

        public readonly HashSet<MatchBlock> blocksOutOfBounds = new HashSet<MatchBlock>();
        public readonly HashSet<MatchBlock> nextBlockSet = new HashSet<MatchBlock>();

        public static bool IsEmpty(MatchBlock[] grid)
        {
            foreach (MatchBlock block in grid)
            {
                if (block != null)
                {
                    return false;
                }
            }
            return true;
        }
    }

    [Serializable]
    public sealed class MatchBlockGridSystem : ASingleton<MatchBlockGridSystem>
    {
        public static event Action onAcceptBlockSet;
        public static event Action onRejectBlockSet;

        public static event Action onColumnOverflow;
        public static event Action<MatchBlockGrid> onBlocksPacked;

        public MatchBlockGrid blockGrid = new MatchBlockGrid();

        private Action m_OnWin_DisableSelect;
        private Action m_OnSelectDown_AcceptBlockSet;
        private Action m_OnSelectUp_RejectBlockSet;
        private Action<MatchBlockGrid> m_OnBlocksDestroyed_PackBlocksDown;

        public float fallPerCellDuration = 0.25f;
        private float m_FallPerCellTimeRemaining = -1f;

        public MatchBlockGridSystem()
        {
            m_OnSelectDown_AcceptBlockSet = AcceptBlockSet;
            VerticalInputSystem.onSelectDown += m_OnSelectDown_AcceptBlockSet;

            m_OnSelectUp_RejectBlockSet = RejectBlockSet;
            VerticalInputSystem.onSelectUp += m_OnSelectUp_RejectBlockSet;

            m_OnBlocksDestroyed_PackBlocksDown = PackBlocksDown;
            MatchDestroySystem.onBlocksDestroyed += m_OnBlocksDestroyed_PackBlocksDown;

            m_OnWin_DisableSelect = DisableSelect;
            WinOnBlocksClearedSystem.onWin += m_OnWin_DisableSelect;
        }

        ~MatchBlockGridSystem()
        {
            VerticalInputSystem.onSelectDown -= m_OnSelectDown_AcceptBlockSet;
            MatchDestroySystem.onBlocksDestroyed -= m_OnBlocksDestroyed_PackBlocksDown;
            WinOnBlocksClearedSystem.onWin += m_OnWin_DisableSelect;
        }

        private void EnableSelect()
        {
            blockGrid.selectEnabled = true;
        }

        private void DisableSelect()
        {
            blockGrid.selectEnabled = false;
        }

        private bool InputEnabled()
        {
            return blockGrid.simulationEnabled && blockGrid.selectEnabled;
        }

        public void ParseGrid(BoxCollider2D collider, float snapZ)
        {
            blockGrid.snapZ = snapZ;
            ParseGrid(collider, blockGrid);
        }

        public void ParseGrid(BoxCollider2D collider, MatchBlockGrid blockGrid)
        {
            blockGrid.simulationEnabled = true;
            Bounds bounds = collider.bounds;
            Vector2 min = (Vector2)bounds.min;
            Vector2 max = (Vector2)bounds.max;
            blockGrid.min = min;
            blockGrid.max = max;
            float cellSize = blockGrid.cellSize;
            blockGrid.cellCenter = 0.5f * cellSize;
            blockGrid.cellSize = cellSize;
            blockGrid.numRows = (int)((max.y - min.y) / cellSize);
            blockGrid.numColumns = (int)((max.x - min.x) / cellSize);
            int numCells = blockGrid.numRows * blockGrid.numColumns;
            blockGrid.numCells = numCells;

            MatchBlock[] grid = new MatchBlock[numCells];
            blockGrid.grid = grid;
            blockGrid.blocksOutOfBounds.Clear();
            MatchBlock[] blocks = GameObject.FindObjectsOfType<MatchBlock>();
            IncludeBlocks(blockGrid, blocks);
        }

        private void IncludeBlocks(MatchBlockGrid blockGrid, IEnumerable<MatchBlock> blocks)
        {
            foreach (MatchBlock block in blocks)
            {
                int cellIndex = GetCellIndex(block.transform.position);
                bool contains = cellIndex >= 0 && cellIndex < blockGrid.numCells;
                if (!contains)
                {
                    blockGrid.blocksOutOfBounds.Add(block);
                    SnapBlock(blockGrid, cellIndex, block);
                    continue;
                }
                blockGrid.blocksOutOfBounds.Remove(block);
                if (blockGrid.grid[cellIndex] != null)
                {
                    ColumnOverflow();
                    SnapBlock(blockGrid, cellIndex, block);
                    continue;
                }
                blockGrid.grid[cellIndex] = block;
            }

            PackBlocksDown(blockGrid);
        }

        private void ColumnOverflow()
        {
            DisableSelect();
            blockGrid.simulationEnabled = false;
            if (onColumnOverflow != null)
            {
                onColumnOverflow();
            }
        }

        public void Update(float deltaTime)
        {
            UpdateFallTimeRemaining(deltaTime);
        }

        private void UpdateFallTimeRemaining(float deltaTime)
        {
            if (m_FallPerCellTimeRemaining < 0f)
            {
                return;
            }
            m_FallPerCellTimeRemaining -= deltaTime;
            if (m_FallPerCellTimeRemaining > 0f)
            {
                return;
            }
            m_FallPerCellTimeRemaining = -1f;
            PackBlocksDown(blockGrid);
        }

        private void PackBlocksDown(MatchBlockGrid blockGrid)
        {
            if (!blockGrid.simulationEnabled)
            {
                return;
            }
            bool isPacked = true;
            int numColumns = blockGrid.numColumns;
            for (int aboveIndex = numColumns, numCells = blockGrid.numCells; aboveIndex < numCells; ++aboveIndex)
            {
                MatchBlock above = blockGrid.grid[aboveIndex];
                if (above == null)
                {
                    continue;
                }
                int belowIndex = aboveIndex;
                do
                {
                    int previousAboveIndex = belowIndex;
                    belowIndex -= numColumns;
                    MatchBlock below = blockGrid.grid[belowIndex];
                    if (below != null)
                    {
                        break;
                    }
                    isPacked = false;
                    SwapBlocks(blockGrid, previousAboveIndex, belowIndex, fallPerCellDuration);
                }
                while (belowIndex >= numColumns && fallPerCellDuration <= 0f);
            }

            if (fallPerCellDuration > 0f)
            {
                m_FallPerCellTimeRemaining = fallPerCellDuration;
            }
            if (!isPacked)
            {
                DisableSelect();
                return;
            }
            EnableSelect();
            if (onBlocksPacked != null)
            {
                onBlocksPacked(blockGrid);
            }
        }

        private static void SwapBlocks(MatchBlockGrid blockGrid, int indexA, int indexB, float duration)
        {
            Swap(blockGrid.grid, indexA, indexB);
            SnapBlock(blockGrid, indexA, null, duration);
            SnapBlock(blockGrid, indexB, null, duration);
        }

        private static void Swap<T>(T[] elements, int indexA, int indexB)
        {
            T swap = elements[indexA];
            elements[indexA] = elements[indexB];
            elements[indexB] = swap;
        }

        private static void SnapBlock(MatchBlockGrid blockGrid, int cellIndex,
            MatchBlock block = null, float duration = 0f)
        {
            if (block == null)
            {
                block = blockGrid.grid[cellIndex];
                if (block == null)
                {
                    return;
                }
            }
            int rowIndex = cellIndex / blockGrid.numColumns;
            int columnIndex = cellIndex % blockGrid.numColumns;
            SnapBlock(block,
                new Vector3(blockGrid.cellSize * (columnIndex + blockGrid.min.x) + blockGrid.cellCenter,
                    blockGrid.cellSize * (rowIndex + blockGrid.min.y) + blockGrid.cellCenter,
                    blockGrid.snapZ
                ),
                duration
            );
        }

        private static void SnapBlock(MatchBlock block, Vector3 snappedPosition, float duration = 0f)
        {
            if (duration <= 0f)
            {
                block.transform.position = new Vector3(snappedPosition.x, snappedPosition.y, snappedPosition.z);
                return;
            }
            block.transform.DOMove(snappedPosition, duration).SetEase(Ease.Linear);
        }

        private void AcceptBlockSet()
        {
            if (!InputEnabled())
            {
                return;
            }
            ShiftNextBlockSets();
            if (onAcceptBlockSet != null)
            {
                onAcceptBlockSet();
            }
        }

        private void ShiftNextBlockSets()
        {
            if (blockGrid.nextBlockSet.Count == 0)
            {
                blockGrid.nextBlockSet.UnionWith(blockGrid.blocksOutOfBounds);
            }
            foreach (MatchBlock block in blockGrid.nextBlockSet)
            {
                float distanceY = GetClearDistanceY(block.transform.position, -blockGrid.numRowsInSet);
                if (distanceY == 0f)
                {
                    ColumnOverflow();
                    return;
                }
                Move(block, 0, distanceY);
            }
            IncludeBlocks(blockGrid, blockGrid.nextBlockSet);
            blockGrid.nextBlockSet.Clear();

            EnableSelect();
        }

        private void RejectBlockSet()
        {
            if (!InputEnabled())
            {
                return;
            }
            if (blockGrid.nextBlockSet.Count == 0)
            {
                blockGrid.nextBlockSet.UnionWith(blockGrid.blocksOutOfBounds);
                if (blockGrid.nextBlockSet.Count == 0)
                {
                    return;
                }
            }
            float topY = MaxY(blockGrid.blocksOutOfBounds) + blockGrid.cellSize;
            float nextRowSet = (topY - blockGrid.max.y - blockGrid.cellSize) % blockGrid.numRowsInSet;
            topY += blockGrid.numRowsInSet - nextRowSet;
            bool isAny = false;
            foreach (MatchBlock block in blockGrid.nextBlockSet)
            {
                if (IsInNextRowSet(block, blockGrid))
                {
                    isAny = true;
                    Vector3 position = block.transform.position;
                    float offsetAboveGrid = position.y - blockGrid.max.y - blockGrid.cellSize;
                    position.y = topY + offsetAboveGrid;
                    SnapBlock(block, position, 0f);

                    block.Reject();
                }
            }
            if (!isAny)
            {
                return;
            }
            DisableSelect();
            ShiftNextBlockSets();
            EnableSelect();
            if (onRejectBlockSet != null)
            {
                onRejectBlockSet();
            }
        }

        private static float MaxY(HashSet<MatchBlock> blockSets)
        {
            float maxY = float.NegativeInfinity;
            foreach (MatchBlock block in blockSets)
            {
                float y = block.transform.position.y;
                if (y > maxY)
                {
                    maxY = y;
                }
            }
            return maxY;
        }

        private static bool IsInNextRowSet(MatchBlock block, MatchBlockGrid blockGrid)
        {
            Vector2 min = blockGrid.min;
            float cellSize = blockGrid.cellSize;
            Vector2 blockPoint = (Vector2)block.transform.position;
            int rowIndex = (int)((blockPoint.y - min.y) / cellSize);
            return rowIndex >= blockGrid.numRows
                && rowIndex < blockGrid.numRows + blockGrid.numRowsInSet;
        }

        private void Move(MatchBlock block, float x, float y)
        {
            Vector3 position = block.transform.position;
            block.transform.position = new Vector3(position.x + x, position.y + y, position.z);
        }

        private int GetCellIndex(Vector3 position)
        {
            Vector2 blockPoint = (Vector2)position;
            int rowIndex = (int)((blockPoint.y - blockGrid.min.y) / blockGrid.cellSize);
            int columnIndex = (int)((blockPoint.x - blockGrid.min.x) / blockGrid.cellSize);
            int cellIndex = rowIndex * blockGrid.numColumns + columnIndex;
            return cellIndex;
        }

        private float GetClearDistanceY(Vector3 position, float distance)
        {
            Vector3 nextPosition = new Vector3();
            nextPosition.x = position.x;
            nextPosition.z = position.z;
            while (distance < 0f)
            {
                nextPosition.y = position.y + distance;
                int cellIndex = GetCellIndex(nextPosition);
                bool contains = cellIndex >= 0 && cellIndex < blockGrid.numCells;
                if (!contains)
                {
                    return distance;
                }
                if (blockGrid.grid[cellIndex] == null)
                {
                    return distance;
                }
                distance += 1f;
            }
            return 0f;
        }
    }
}
