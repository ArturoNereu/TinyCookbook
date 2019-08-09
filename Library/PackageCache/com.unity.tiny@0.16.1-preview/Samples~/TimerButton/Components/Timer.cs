using Unity.Entities;
using Unity.Authoring.Core;

namespace TimerButton
{
    public struct Timer : IComponentData
    {
        [HideInInspector]
        public float RemainingTime;
    }
}
