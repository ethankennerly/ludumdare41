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

        private void OnEnable()
        {
            controller.ParseGrid(m_Collider, transform.position.z);
            MatchBlockGridSystem.onAcceptBlockSet += PlayRandomAcceptClip;
        }

        private void OnDisable()
        {
            MatchBlockGridSystem.onAcceptBlockSet -= PlayRandomAcceptClip;
        }

        private void PlayRandomAcceptClip()
        {
            AudioUtils.PlayRandom(m_AudioSource, m_AcceptClips);
        }

        private void Update()
        {
            controller.Update(Time.deltaTime);
        }
    }
}
