using Finegamedesign.Utils;
using UnityEngine;

namespace Finegamedesign.LudumDare41
{
    public sealed class MatchBlockGridView : ASingletonView<MatchBlockGridSystem>
    {
        [SerializeField]
        private BoxCollider2D m_Collider = null;

        private void OnEnable()
        {
            controller.ParseGrid(m_Collider, transform.position.z);
        }

        private void Update()
        {
            controller.Update(Time.deltaTime);
        }
    }
}
