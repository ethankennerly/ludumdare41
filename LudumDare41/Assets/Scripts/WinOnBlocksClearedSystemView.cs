using Finegamedesign.Utils;
using System;
using UnityEngine;

namespace Finegamedesign.LudumDare41
{
    public sealed class WinOnBlocksClearedSystemView : ASingletonView<WinOnBlocksClearedSystem>
    {
        [SerializeField]
        private Animator m_Animator = null;

        [SerializeField]
        private string m_WinState = null;

        private Action m_OnWin_PlayAnimation;

        private void OnEnable()
        {
            m_OnWin_PlayAnimation = PlayAnimation;
            WinOnBlocksClearedSystem.onWin += m_OnWin_PlayAnimation;
        }

        private void OnDisable()
        {
            WinOnBlocksClearedSystem.onWin -= m_OnWin_PlayAnimation;
        }

        private void PlayAnimation()
        {
            LoadNextSceneOnInput.nextIncrement = 1;

            if (m_Animator == null)
            {
                return;
            }
            m_Animator.Play(m_WinState);
        }
    }
}
