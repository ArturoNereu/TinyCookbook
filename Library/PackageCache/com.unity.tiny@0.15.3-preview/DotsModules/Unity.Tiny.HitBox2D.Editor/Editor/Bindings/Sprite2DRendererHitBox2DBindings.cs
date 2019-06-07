using JetBrains.Annotations;
using Unity.Entities;
using Unity.Editor.Bindings;
using Unity.Tiny.Core2D;
using Unity.Tiny.HitBox2D;

namespace Unity.Tiny.HitBox2D.Editor
{
    [UsedImplicitly]
    internal class Sprite2DRendererHitBox2DBindings : IEntityBinding,
        IComponentBinding<Sprite2DRenderer, Sprite2DRendererHitBox2D>,
        IBindingDependency<Core2D.Editor.Sprite2DRendererBindings>
    {
        public void LoadBinding(Entity entity, IBindingContext context)
        {
            context.AddMissingUnityComponent<Unity.Tiny.Sprite2DRendererHitBox2D>(entity);
        }

        public void UnloadBinding(Entity entity, IBindingContext context)
        {
            context.RemoveUnityComponent<Unity.Tiny.Sprite2DRendererHitBox2D>(entity);
        }

        public void TransferToUnityComponents(Entity entity, IBindingContext context)
        {
            var spriteRenderer = context.GetComponentData<Sprite2DRenderer>(entity);
            var behaviour = context.GetUnityComponent<Unity.Tiny.Sprite2DRendererHitBox2D>(entity);
            behaviour.Sprite = context.GetUnityObject<UnityEngine.Sprite>(spriteRenderer.sprite);
        }

        public void TransferFromUnityComponents(Entity entity, IBindingContext context)
        {
        }
    }
}
