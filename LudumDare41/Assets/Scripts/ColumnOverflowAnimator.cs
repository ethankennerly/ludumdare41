using System;
using UnityEngine;

namespace Finegamedesign.LudumDare41
{
    public sealed class ColumnOverflowAnimator : MonoBehaviour
    {
        [SerializeField]
        private Animator m_Animator = null;

        [SerializeField]
        private string m_ColumnOverflowState = null;

        private Action m_OnColumnOverflow_PlayAnimation;

        private void OnEnable()
        {
            m_OnColumnOverflow_PlayAnimation = PlayAnimation;
            MatchBlockGridSystem.onColumnOverflow += m_OnColumnOverflow_PlayAnimation;
        }

        private void OnDisable()
        {
            MatchBlockGridSystem.onColumnOverflow -= m_OnColumnOverflow_PlayAnimation;
        }

        private void PlayAnimation()
        {
            if (m_Animator == null)
            {
                return;
            }
            m_Animator.Play(m_ColumnOverflowState);
        }
    }
}
