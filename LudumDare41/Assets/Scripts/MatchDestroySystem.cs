using Finegamedesign.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Finegamedesign.LudumDare41
{
    [Serializable]
    public sealed class MatchDestroySystem : ASingleton<MatchDestroySystem>
    {
        public int minBlocksToMatch = 4;

        public float destroyDuration = 1f;

        private float destroyTimeRemaining = -1f;

        public static event Action<MatchBlockGrid> onBlocksDestroyed;

        private Action<MatchBlockGrid> m_OnBlocksPacked_DestroyMatches;

        private readonly Stack<int> m_CellIndexesToVisit = new Stack<int>();
        
        private readonly HashSet<int> m_VisitedBlockIndexes = new HashSet<int>();

        private MatchBlockGrid m_BlockGrid;

        public MatchDestroySystem()
        {
            m_OnBlocksPacked_DestroyMatches = DestroyMatches;
            MatchBlockGridSystem.onBlocksPacked += m_OnBlocksPacked_DestroyMatches;
        }

        ~MatchDestroySystem()
        {
            MatchBlockGridSystem.onBlocksPacked -= m_OnBlocksPacked_DestroyMatches;
        }

        public void Update(float deltaTime)
        {
            UpdateDestroy(deltaTime);
        }

        private void UpdateDestroy(float deltaTime)
        {
            if (destroyTimeRemaining < 0f)
            {
                return;
            }
            destroyTimeRemaining -= deltaTime;
            if (destroyTimeRemaining > 0f)
            {
                return;
            }
            destroyTimeRemaining = -1f;
            if (onBlocksDestroyed != null && m_BlockGrid != null)
            {
                m_BlockGrid.simulationEnabled = true;
                onBlocksDestroyed(m_BlockGrid);
            }
        }

        private void DestroyMatches(MatchBlockGrid blockGrid)
        {
            bool isDestroying = TryDestroyMatches(blockGrid, minBlocksToMatch,
                m_CellIndexesToVisit, m_VisitedBlockIndexes);

            if (!isDestroying)
            {
                return;
            }
            blockGrid.numChains++;
            if (destroyDuration >= 0f)
            {
                destroyTimeRemaining = destroyDuration;
                m_BlockGrid = blockGrid;
                blockGrid.selectEnabled = false;
                blockGrid.simulationEnabled = false;
                return;
            }
            if (onBlocksDestroyed != null)
            {
                onBlocksDestroyed(blockGrid);
            }
        }

        // Could be sped up on large sparse graph by listing occupants rather than traversing total grid.
        private static bool TryDestroyMatches(MatchBlockGrid blockGrid, int minBlocksToMatch,
            Stack<int> cellIndexesToVisit, HashSet<int> visitedBlockIndexes)
        {
            bool isDestroying = false;
            SetCellIndexesToVisit(blockGrid, cellIndexesToVisit);

            List<HashSet<int>> matchedGroups = FindMatchedGroups(blockGrid, cellIndexesToVisit, visitedBlockIndexes);

            foreach (HashSet<int> matchedBlockIndexes in matchedGroups)
            {
                if (matchedBlockIndexes.Count < minBlocksToMatch)
                {
                    continue;
                }
                isDestroying = true;
                foreach (int matchedBlockIndex in matchedBlockIndexes)
                {
                    MatchBlock block = Destroy(blockGrid.grid, matchedBlockIndex);
                    if (block == null)
                    {
                        continue;
                    }
                    blockGrid.destroyedMatchIndexes.Add(block.matchIndex);
                }
            }
            return isDestroying;
        }

        private static void SetCellIndexesToVisit(MatchBlockGrid blockGrid, Stack<int> cellIndexesToVisit)
        {
            cellIndexesToVisit.Clear();
            for (int cellIndex = 0; cellIndex < blockGrid.numCells; ++cellIndex)
            {
                MatchBlock block = blockGrid.grid[cellIndex];
                if (block == null)
                {
                    continue;
                }
                cellIndexesToVisit.Push(cellIndex);
            }
        }

        private static List<HashSet<int>> FindMatchedGroups(MatchBlockGrid blockGrid, Stack<int> cellIndexesToVisit,
            HashSet<int> visitedBlockIndexes)
        {
            visitedBlockIndexes.Clear();
            List<HashSet<int>> matchedGroups = new List<HashSet<int>>();
            int[] neighborIndexes = new int[4];
            Stack<int> groupIndexesToVisit = new Stack<int>();
            while (cellIndexesToVisit.Count > 0)
            {
                int blockIndex = cellIndexesToVisit.Pop();
                if (!visitedBlockIndexes.Add(blockIndex))
                {
                    continue;
                }
                HashSet<int> matchedBlockIndexes = new HashSet<int>();
                groupIndexesToVisit.Clear();
                groupIndexesToVisit.Push(blockIndex);
                while (groupIndexesToVisit.Count > 0)
                {
                    int cellIndex = groupIndexesToVisit.Pop();
                    MatchBlock cell = blockGrid.grid[cellIndex];
                    if (!matchedBlockIndexes.Add(cellIndex))
                    {
                        continue;
                    }
                    int columnIndex = cellIndex % blockGrid.numColumns;
                    int rowIndex = cellIndex / blockGrid.numRows;
                    neighborIndexes[0] = columnIndex <= 0 ? -1 : cellIndex - 1;
                    neighborIndexes[1] = columnIndex >= (blockGrid.numColumns - 1) ? -1 : cellIndex + 1;
                    neighborIndexes[2] = rowIndex <= 0 ? -1 : cellIndex - blockGrid.numColumns;
                    neighborIndexes[3] = rowIndex >= (blockGrid.numRows - 1) ? -1 : cellIndex + blockGrid.numColumns;
                    foreach (int neighborIndex in neighborIndexes)
                    {
                        if (neighborIndex < 0 || neighborIndex >= blockGrid.numCells)
                        {
                            continue;
                        }
                        MatchBlock neighbor = blockGrid.grid[neighborIndex];
                        if (neighbor == null)
                        {
                            continue;
                        }
                        if (neighbor.matchIndex != cell.matchIndex)
                        {
                            continue;
                        }
                        groupIndexesToVisit.Push(neighborIndex);
                    }
                }
                if (matchedBlockIndexes.Count > 0)
                {
                    matchedGroups.Add(matchedBlockIndexes);
                }
            }
            return matchedGroups;
        }

        private static MatchBlock Destroy(MatchBlock[] grid, int cellIndex)
        {
            MatchBlock block = grid[cellIndex];
            if (block == null)
            {
                return null;
            }
            block.Match();
            grid[cellIndex] = null;
            return block;
        }
    }
}
