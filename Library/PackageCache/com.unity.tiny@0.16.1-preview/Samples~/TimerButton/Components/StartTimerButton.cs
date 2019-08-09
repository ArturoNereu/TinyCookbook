using Unity.Entities;

namespace TimerButton
{
    public struct StartTimerButton : IComponentData
    {
        public Entity TimerEntity;
    }
}
