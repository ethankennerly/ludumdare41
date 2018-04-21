using Finegamedesign.Utils;
using System;
using UnityEngine;

namespace Finegamedesign.LudumDare41
{
    public sealed class WinOnBlocksClearedSystem : ASingleton<WinOnBlocksClearedSystem>
    {
        public static event Action onWin;

        private Action<MatchBlockGrid> m_OnBlocksDestroyed_WinIfAllClear;

        public WinOnBlocksClearedSystem()
        {
            m_OnBlocksDestroyed_WinIfAllClear = WinIfAllClear;
            MatchBlockGridSystem.onBlocksPacked += m_OnBlocksDestroyed_WinIfAllClear;
        }

        ~WinOnBlocksClearedSystem()
        {
            MatchBlockGridSystem.onBlocksPacked -= m_OnBlocksDestroyed_WinIfAllClear;
        }

        private void WinIfAllClear(MatchBlockGrid blockGrid)
        {
            if (!MatchBlockGrid.IsEmpty(blockGrid.grid))
            {
                return;
            }
            if (onWin == null)
            {
                return;
            }
            onWin();
        }
    }
}
