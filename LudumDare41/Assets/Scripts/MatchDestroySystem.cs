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

        public static event Action<MatchBlockGrid> onBlocksDestroyed;

        private Action<MatchBlockGrid> m_OnBlocksPacked_DestroyMatches;

        private readonly Stack<int> m_CellIndexesToVisit = new Stack<int>();
        
        private readonly HashSet<int> m_VisitedBlockIndexes = new HashSet<int>();

        public MatchDestroySystem()
        {
            m_OnBlocksPacked_DestroyMatches = DestroyMatches;
            MatchBlockGridSystem.onBlocksPacked += m_OnBlocksPacked_DestroyMatches;
        }

        ~MatchDestroySystem()
        {
            MatchBlockGridSystem.onBlocksPacked -= m_OnBlocksPacked_DestroyMatches;
        }

        private void DestroyMatches(MatchBlockGrid blockGrid)
        {
            bool isDestroying = TryDestroyMatches(blockGrid, minBlocksToMatch,
                m_CellIndexesToVisit, m_VisitedBlockIndexes);

            if (!isDestroying)
            {
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
                    Destroy(blockGrid.grid, matchedBlockIndex);
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
            HashSet<int> matchedBlockIndexes = new HashSet<int>();
            while (cellIndexesToVisit.Count > 0)
            {
                int blockIndex = cellIndexesToVisit.Pop();
                if (!visitedBlockIndexes.Add(blockIndex))
                {
                    continue;
                }
                matchedBlockIndexes.Clear();
                matchedBlockIndexes.Add(blockIndex);
                groupIndexesToVisit.Clear();
                groupIndexesToVisit.Push(blockIndex);
                while (groupIndexesToVisit.Count > 0)
                {
                    int cellIndex = groupIndexesToVisit.Pop();
                    neighborIndexes[0] = cellIndex - 1;
                    neighborIndexes[1] = cellIndex + 1;
                    neighborIndexes[2] = cellIndex - blockGrid.numColumns;
                    neighborIndexes[3] = cellIndex + blockGrid.numColumns;
                    MatchBlock block = blockGrid.grid[cellIndex];
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
                        if (!visitedBlockIndexes.Add(neighborIndex))
                        {
                            continue;
                        }
                        if (neighbor.matchIndex != block.matchIndex)
                        {
                            continue;
                        }
                        groupIndexesToVisit.Push(neighborIndex);
                        matchedBlockIndexes.Add(neighborIndex);
                    }
                }
                if (matchedBlockIndexes.Count > 0)
                {
                    matchedGroups.Add(matchedBlockIndexes);
                }
            }
            return matchedGroups;
        }

        private static void Destroy(MatchBlock[] grid, int cellIndex)
        {
            UnityEngine.Object.Destroy(grid[cellIndex].gameObject);
            grid[cellIndex] = null;
        }
    }
}
