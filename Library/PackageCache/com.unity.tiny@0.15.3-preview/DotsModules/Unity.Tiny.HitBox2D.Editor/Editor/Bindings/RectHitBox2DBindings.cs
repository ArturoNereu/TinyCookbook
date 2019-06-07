using JetBrains.Annotations;
using Unity.Entities;
using Unity.Editor.Bindings;
using Unity.Tiny.Core2D;
using Unity.Tiny.Core2D.Editor;

namespace Unity.Tiny.HitBox2D.Editor
{
    [UsedImplicitly]
    internal class RectHitBox2DBindings : IEntityBinding,
        IComponentBinding<RectHitBox2D, Parent>
    {
        public void LoadBinding(Entity entity, IBindingContext context)
        {
            context.AddMissingUnityComponent<Unity.Tiny.RectHitBox2D>(entity);
        }

        public void UnloadBinding(Entity entity, IBindingContext context)
        {
            context.RemoveUnityComponent<Unity.Tiny.RectHitBox2D>(entity);
        }

        public void TransferToUnityComponents(Entity entity, IBindingContext context)
        {
            var hitbox = context.GetComponentData<RectHitBox2D>(entity);
            context.GetUnityComponent<Unity.Tiny.RectHitBox2D>(entity).Box = hitbox.box.Convert();
        }

        public void TransferFromUnityComponents(Entity entity, IBindingContext context)
        {
            context.SetComponentData(entity, new RectHitBox2D
            {
                box = context.GetUnityComponent<Unity.Tiny.RectHitBox2D>(entity).Box.Convert()
            });
        }
    }
}
