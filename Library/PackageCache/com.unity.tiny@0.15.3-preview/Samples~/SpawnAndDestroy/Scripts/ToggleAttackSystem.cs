using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.UIControls;

namespace SpawnAndDestroy
{
    /// <summary>
    /// When the user press the UI Toggle with the ButtonToggleAttack component,
    /// toggle the IsAttacking value in the AttackConfiguration.
    /// </summary>
    public class ToggleAttackSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var env = World.TinyEnvironment();
            Entities.WithAll<ButtonToggleAttack>().ForEach((Entity entity, ref PointerInteraction pointerInteraction, ref Toggle toggle) =>
            {
                if (pointerInteraction.clicked)
                {
                    var attackConfig = env.GetConfigData<AttackConfiguration>();
                    attackConfig.IsAttacking = toggle.isOn;
                    env.SetConfigData(attackConfig);
                }
            });
        }
    }
}
