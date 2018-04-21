using Finegamedesign.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Finegamedesign.LudumDare41
{
    public sealed class MatchBlockGrid
    {
        public float cellSize = 1f;
        public int numColumns = 0;
        public int numRows = 0;
        public int numCells = 0;
        public MatchBlock[] grid;

        public readonly List<MatchBlock> blocksOutOfBounds = new List<MatchBlock>();
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
            ParseGrid(collider, snapZ, blockGrid);
        }

        public void ParseGrid(BoxCollider2D collider, float snapZ, MatchBlockGrid blockGrid)
        {
            Bounds bounds = collider.bounds;
            Vector2 min = (Vector2)bounds.min;
            Vector2 max = (Vector2)bounds.max;
            float cellSize = blockGrid.cellSize;
            float cellCenter = 0.5f * cellSize;
            blockGrid.numRows = (int)((max.y - min.y) / cellSize);
            blockGrid.numColumns = (int)((max.x - min.x) / cellSize);
            int numCells = blockGrid.numRows * blockGrid.numColumns;
            blockGrid.numCells = numCells;

            MatchBlock[] grid = new MatchBlock[numCells];
            blockGrid.grid = grid;
            blockGrid.blocksOutOfBounds.Clear();
            MatchBlock[] blocks = GameObject.FindObjectsOfType<MatchBlock>();
            foreach (MatchBlock block in blocks)
            {
                Vector2 blockPoint = (Vector2)block.transform.position;
                int rowIndex = (int)((blockPoint.y - min.y) / cellSize);
                int columnIndex = (int)((blockPoint.x - min.x) / cellSize);
                int cellIndex = rowIndex * blockGrid.numColumns + columnIndex;
                Vector3 snappedPosition = new Vector3(
                    cellSize * (columnIndex + min.x) + cellCenter,
                    cellSize * (rowIndex + min.y) + cellCenter,
                    snapZ);
                block.transform.position = snappedPosition;

                bool contains = collider.OverlapPoint(blockPoint);
                if (!contains)
                {
                    blockGrid.blocksOutOfBounds.Add(block);
                    continue;
                }
                grid[cellIndex] = block;
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
        }

        private void Move(MatchBlock block, float x, float y)
        {
            Vector3 position = block.transform.position;
            block.transform.position = new Vector3(position.x + x, position.y + y, position.z);
        }
    }
}
