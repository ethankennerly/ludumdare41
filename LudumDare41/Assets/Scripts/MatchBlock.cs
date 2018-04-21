using Spine.Unity;
using UnityEngine;

namespace Finegamedesign.LudumDare41
{
    public sealed class MatchBlock : MonoBehaviour
    {
        [Header("When enough of same index match, plays match animation.")]
        [SerializeField]
        private int m_MatchIndex = -1;
        public int matchIndex { get { return m_MatchIndex; } }

        [SerializeField]
        private SkeletonAnimation m_Skeleton = null;

        [SerializeField]
		private string m_MatchAnimationName = null;

        public void Match()
        {
            if (m_Skeleton == null)
            {
                return;
            }
            m_Skeleton.AnimationName = m_MatchAnimationName;
        }
    }
}
