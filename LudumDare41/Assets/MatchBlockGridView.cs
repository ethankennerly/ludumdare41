using Finegamedesign.Utils;
using UnityEngine;

namespace Finegamedesign.LudumDare41
{
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class MatchBlockGridView : ASingletonView<MatchBlockGrid>
    {
        private BoxCollider2D m_Collider;

        private void OnEnable()
        {
            m_Collider = (BoxCollider2D)GetComponent(typeof(BoxCollider2D));
            ParseGrid(m_Collider, controller, transform.position.z);
        }

        private static void ParseGrid(BoxCollider2D collider, MatchBlockGrid blockGrid, float snapZ)
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
            MatchBlock[] blocks = FindObjectsOfType<MatchBlock>();
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
                    continue;
                }
                grid[cellIndex] = block;
            }
        }
    }
}
