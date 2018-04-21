namespace Finegamedesign.Utils
{
    public sealed class VerticalInputSystemView : ASingletonView<VerticalInputSystem>
    {
        private void Update()
        {
            controller.Update();
        }
    }
}
