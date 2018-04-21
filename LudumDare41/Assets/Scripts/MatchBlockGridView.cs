using Finegamedesign.Utils;
using UnityEngine;

namespace Finegamedesign.LudumDare41
{
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class MatchBlockGridView : ASingletonView<MatchBlockGridSystem>
    {
        private BoxCollider2D m_Collider;

        private void OnEnable()
        {
            m_Collider = (BoxCollider2D)GetComponent(typeof(BoxCollider2D));
            controller.ParseGrid(m_Collider, transform.position.z);
        }
    }
}
