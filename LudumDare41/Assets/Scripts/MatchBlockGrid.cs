using Finegamedesign.Utils;
using System;

namespace Finegamedesign.LudumDare41
{
    [Serializable]
    public sealed class MatchBlockGrid : ASingleton<MatchBlockGrid>
    {
        public float cellSize = 1f;
        public int numColumns = 0;
        public int numRows = 0;
        public int numCells = 0;
        public MatchBlock[] grid;
    }
}
