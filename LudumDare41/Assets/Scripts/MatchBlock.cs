using UnityEngine;

namespace Finegamedesign.LudumDare41
{
    public sealed class MatchBlock : MonoBehaviour
    {
        [Header("When enough match, they are destroyed.")]
        [SerializeField]
        private int m_MatchIndex = -1;
        public int matchIndex { get { return m_MatchIndex; } }
    }
}
