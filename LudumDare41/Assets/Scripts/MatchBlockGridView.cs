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

        private void OnEnable()
        {
            controller.ParseGrid(m_Collider, transform.position.z);
            MatchBlockGridSystem.onAcceptBlockSet += PlayRandomAcceptClip;
            MatchBlockGridSystem.onRejectBlockSet += PlayRandomRejectClip;
        }

        private void OnDisable()
        {
            MatchBlockGridSystem.onAcceptBlockSet -= PlayRandomAcceptClip;
            MatchBlockGridSystem.onRejectBlockSet -= PlayRandomRejectClip;
        }

        private void PlayRandomAcceptClip()
        {
            AudioUtils.PlayRandom(m_AudioSource, m_AcceptClips);
        }

        private void PlayRandomRejectClip()
        {
            AudioUtils.PlayRandom(m_AudioSource, m_RejectClips);
        }

        private void Update()
        {
            controller.Update(Time.deltaTime);
        }
    }
}
