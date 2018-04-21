using System;

namespace Finegamedesign.Utils
{
    public sealed class VerticalInputSystem : ASingleton<VerticalInputSystem>
    {
        public static event Action onSelectDown;
        public static event Action onSelectUp;

        private Action<float, float> OnKeyDownXY_SelectDirection;

        public VerticalInputSystem()
        {
            OnKeyDownXY_SelectDirection = SelectDirection;
            KeyView.onKeyDownXY += OnKeyDownXY_SelectDirection;
        }

        ~VerticalInputSystem()
        {
            KeyView.onKeyDownXY -= OnKeyDownXY_SelectDirection;
        }

        public void Update()
        {
            KeyView.Update();
        }

        private void SelectDirection(float x, float y)
        {
            if (y < 0f)
            {
                if (onSelectDown != null)
                {
                    onSelectDown();
                }
            }
            else if (y > 0f)
            {
                if (onSelectUp != null)
                {
                    onSelectUp();
                }
            }
        }
    }
}
