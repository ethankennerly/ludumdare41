using Finegamedesign.Utils;
using UnityEngine;

namespace Finegamedesign.LudumDare41
{
    public sealed class MatchDestroySystemView : ASingletonView<MatchDestroySystem>
    {
        private void Update()
        {
            controller.Update(Time.deltaTime);
        }
    }
}
