using Finegamedesign.Utils;
using UnityEngine;

namespace Finegamedesign.LudumDare41
{
    public sealed class MatchBlockGridView : ASingletonView<MatchBlockGridSystem>
    {
        [SerializeField]
        private BoxCollider2D m_Collider = null;

        [SerializeField]
        private AudioSource m_AudioSource = null;

        [SerializeField]
        private AudioClip[] m_AcceptClips = null;

        [SerializeField]
        private AudioClip[] m_RejectClips = null;

        [SerializeField]
        private AudioClip[] m_MatchedClips = null;

        private void OnEnable()
        {
            controller.ParseGrid(m_Collider, transform.position.z);

            MatchBlockGridSystem.onAcceptBlockSet += PlayRandomAcceptClip;
            MatchBlockGridSystem.onRejectBlockSet += PlayRandomRejectClip;
            MatchDestroySystem.onBlocksDestroyed += PlayMatchedClips;
        }

        private void OnDisable()
        {
            MatchBlockGridSystem.onAcceptBlockSet -= PlayRandomAcceptClip;
            MatchBlockGridSystem.onRejectBlockSet -= PlayRandomRejectClip;
            MatchDestroySystem.onBlocksDestroyed -= PlayMatchedClips;
        }

        private void PlayRandomAcceptClip()
        {
            AudioUtils.PlayRandom(m_AudioSource, m_AcceptClips);
        }

        private void PlayRandomRejectClip()
        {
            AudioUtils.PlayRandom(m_AudioSource, m_RejectClips);
        }

        private void PlayMatchedClips(MatchBlockGrid blockGrid)
        {
            if (m_MatchedClips == null || m_MatchedClips.Length == 0)
            {
                return;
            }
            foreach (int index in blockGrid.destroyedMatchIndexes)
            {
                m_AudioSource.clip = m_MatchedClips[index];
                m_AudioSource.Play();
            }
            blockGrid.destroyedMatchIndexes.Clear();
        }

        private void Update()
        {
            controller.Update(Time.deltaTime);
        }
    }
}
