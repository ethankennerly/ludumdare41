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
        public readonly MatchBlockGrid blockGrid = new MatchBlockGrid();

        private Action m_OnSelectDown_AcceptBlockSet;

        public MatchBlockGridSystem()
        {
            m_OnSelectDown_AcceptBlockSet = AcceptBlockSet;
            VerticalInputSystem.onSelectDown += m_OnSelectDown_AcceptBlockSet;
        }

        ~MatchBlockGridSystem()
        {
            VerticalInputSystem.onSelectDown -= m_OnSelectDown_AcceptBlockSet;
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
            UpdateBlocks(blockGrid, blocks);
        }

        private void UpdateBlocks(MatchBlockGrid blockGrid, IEnumerable<MatchBlock> blocks)
        {
            Vector2 min = blockGrid.min;
            float cellSize = blockGrid.cellSize;
            float cellCenter = blockGrid.cellCenter;
            foreach (MatchBlock block in blocks)
            {
                Vector2 blockPoint = (Vector2)block.transform.position;
                int rowIndex = (int)((blockPoint.y - min.y) / cellSize);
                int columnIndex = (int)((blockPoint.x - min.x) / cellSize);
                int cellIndex = rowIndex * blockGrid.numColumns + columnIndex;
                Vector3 snappedPosition = new Vector3(
                    cellSize * (columnIndex + min.x) + cellCenter,
                    cellSize * (rowIndex + min.y) + cellCenter,
                    blockGrid.snapZ);
                block.transform.position = snappedPosition;

                bool contains = cellIndex >= 0 && cellIndex < blockGrid.numCells;
                if (!contains)
                {
                    blockGrid.blocksOutOfBounds.Add(block);
                    continue;
                }
                blockGrid.blocksOutOfBounds.Remove(block);
                blockGrid.grid[cellIndex] = block;
            }
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
                Move(block, 0, -2);
            }
            UpdateBlocks(blockGrid, blockGrid.nextBlockSet);
            blockGrid.nextBlockSet.Clear();
        }

        private void Move(MatchBlock block, float x, float y)
        {
            Vector3 position = block.transform.position;
            block.transform.position = new Vector3(position.x + x, position.y + y, position.z);
        }
    }
}
