using UnityEngine;

namespace Finegamedesign.LudumDare41
{
    public sealed class MatchBlock : MonoBehaviour
    {
        [Header("When they match, they are destroyed.")]
        [SerializeField]
        private int m_MatchIndex = -1;
        public int MatchIndex { get { return m_MatchIndex; } }
    }
}
