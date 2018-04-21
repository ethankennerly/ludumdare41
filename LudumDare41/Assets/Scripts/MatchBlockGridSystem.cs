using Finegamedesign.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Finegamedesign.LudumDare41
{
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

        public readonly HashSet<MatchBlock> blocksOutOfBounds = new HashSet<MatchBlock>();
        public readonly HashSet<MatchBlock> nextBlockSet = new HashSet<MatchBlock>();
    }

    public sealed class MatchBlockGridSystem : ASingleton<MatchBlockGridSystem>
    {
        public static event Action<MatchBlockGrid> onBlocksPacked;

        public readonly MatchBlockGrid blockGrid = new MatchBlockGrid();

        private Action m_OnSelectDown_AcceptBlockSet;
        private Action<MatchBlockGrid> m_OnBlocksDestroyed_PackBlocksDown;

        public MatchBlockGridSystem()
        {
            m_OnSelectDown_AcceptBlockSet = AcceptBlockSet;
            VerticalInputSystem.onSelectDown += m_OnSelectDown_AcceptBlockSet;
            m_OnBlocksDestroyed_PackBlocksDown = PackBlocksDown;
            MatchDestroySystem.onBlocksDestroyed += m_OnBlocksDestroyed_PackBlocksDown;
        }

        ~MatchBlockGridSystem()
        {
            VerticalInputSystem.onSelectDown -= m_OnSelectDown_AcceptBlockSet;
            MatchDestroySystem.onBlocksDestroyed -= m_OnBlocksDestroyed_PackBlocksDown;
        }

        public void ParseGrid(BoxCollider2D collider, float snapZ)
        {
            blockGrid.snapZ = snapZ;
            ParseGrid(collider, blockGrid);
        }

        public void ParseGrid(BoxCollider2D collider, MatchBlockGrid blockGrid)
        {
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
            Vector2 min = blockGrid.min;
            float cellSize = blockGrid.cellSize;
            foreach (MatchBlock block in blocks)
            {
                Vector2 blockPoint = (Vector2)block.transform.position;
                int rowIndex = (int)((blockPoint.y - min.y) / cellSize);
                int columnIndex = (int)((blockPoint.x - min.x) / cellSize);
                int cellIndex = rowIndex * blockGrid.numColumns + columnIndex;
                SnapBlock(blockGrid, cellIndex, block);

                bool contains = cellIndex >= 0 && cellIndex < blockGrid.numCells;
                if (!contains)
                {
                    blockGrid.blocksOutOfBounds.Add(block);
                    continue;
                }
                blockGrid.blocksOutOfBounds.Remove(block);
                blockGrid.grid[cellIndex] = block;
            }

            PackBlocksDown(blockGrid);
        }

        private void PackBlocksDown(MatchBlockGrid blockGrid)
        {
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
                    SwapBlocks(blockGrid, previousAboveIndex, belowIndex);
                }
                while (belowIndex >= numColumns);
            }

            if (onBlocksPacked != null)
            {
                onBlocksPacked(blockGrid);
            }
        }

        private static void SwapBlocks(MatchBlockGrid blockGrid, int indexA, int indexB)
        {
            Swap(blockGrid.grid, indexA, indexB);
            SnapBlock(blockGrid, indexA);
            SnapBlock(blockGrid, indexB);
        }

        private static void Swap<T>(T[] elements, int indexA, int indexB)
        {
            T swap = elements[indexA];
            elements[indexA] = elements[indexB];
            elements[indexB] = swap;
        }

        private static void SnapBlock(MatchBlockGrid blockGrid, int cellIndex, MatchBlock block = null)
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
            Vector3 snappedPosition = new Vector3(
                blockGrid.cellSize * (columnIndex + blockGrid.min.x) + blockGrid.cellCenter,
                blockGrid.cellSize * (rowIndex + blockGrid.min.y) + blockGrid.cellCenter,
                blockGrid.snapZ);
            block.transform.position = snappedPosition;
        }

        private void AcceptBlockSet()
        {
            if (blockGrid.nextBlockSet.Count == 0)
            {
                // TODO: Group remaining blocks into continguous sets.
                blockGrid.nextBlockSet.UnionWith(blockGrid.blocksOutOfBounds);
            }
            foreach (MatchBlock block in blockGrid.nextBlockSet)
            {
                // TODO: Tolerate varying height.
                Move(block, 0, -2);
            }
            IncludeBlocks(blockGrid, blockGrid.nextBlockSet);
            blockGrid.nextBlockSet.Clear();
        }

        private void Move(MatchBlock block, float x, float y)
        {
            Vector3 position = block.transform.position;
            block.transform.position = new Vector3(position.x + x, position.y + y, position.z);
        }
    }
}
