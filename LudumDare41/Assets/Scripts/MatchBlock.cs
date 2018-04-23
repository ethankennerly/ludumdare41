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
        private int m_FirstSkinVariant = 1;

        [SerializeField]
        private int m_NumSkinVariants = 4;

        [SerializeField]
        private string m_SkinPrefix = "character";

        [SerializeField]
        private SkeletonAnimation m_Skeleton = null;

        [SerializeField]
        private Animator m_Animator = null;

        [SerializeField]
        private string m_MatchAnimationName = null;

        private void Start()
        {
            name = "MatchBlock_" + m_MatchIndex.ToString();
            SetRandomSkin();
        }

        private void SetRandomSkin()
        {
            if (m_Skeleton == null)
            {
                return;
            }
            int skinNumber = m_MatchIndex * m_NumSkinVariants
                + Random.Range(0, m_NumSkinVariants) + m_FirstSkinVariant;
            string skinName = m_SkinPrefix + skinNumber;
            m_Skeleton.skeleton.SetSkin(skinName);
        }

        public void Match()
        {
            if (m_Skeleton != null)
            {
                m_Skeleton.loop = false;
                m_Skeleton.AnimationName = m_MatchAnimationName;
            }
            if (m_Animator != null)
            {
                m_Animator.Play(m_MatchAnimationName);
            }
        }

        public void Reject()
        {
        }
    }
}
