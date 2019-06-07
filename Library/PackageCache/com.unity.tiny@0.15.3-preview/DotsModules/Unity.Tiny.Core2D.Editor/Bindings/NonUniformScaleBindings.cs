using JetBrains.Annotations;
using Unity.Entities;
using Unity.Editor.Bindings;

namespace Unity.Tiny.Core2D.Editor
{
    [UsedImplicitly]
    internal class NonUniformScaleBindings : IEntityBinding,
        IComponentBinding<NonUniformScale>,
        IExcludeComponentBinding<Scale>,
        IBindingDependency<ParentBindings>
    {
        public void LoadBinding(Entity entity, IBindingContext context)
        {
            // Nothing to do.
        }

        public void UnloadBinding(Entity entity, IBindingContext context)
        {
            var transform = context.GetUnityComponent<UnityEngine.Transform>(entity);
            // If this binding is being unloaded, but the entity has a Scale (because it
            // was swapped at some point), set the right value.
            if (context.HasComponent<Scale>(entity))
            {
                var v = context.GetComponentData<Scale>(entity).Value;
                transform.localScale = new UnityEngine.Vector3(v, v, v);
            }
            else
            {
                transform.localScale = UnityEngine.Vector3.one;
            }
        }

        public void TransferToUnityComponents(Entity entity, IBindingContext context)
        {
            var transform = context.GetUnityComponent<UnityEngine.Transform>(entity);
            transform.localScale = context.GetComponentData<NonUniformScale>(entity).Value;
        }

        public void TransferFromUnityComponents(Entity entity, IBindingContext context)
        {
            var transform = context.GetUnityComponent<UnityEngine.Transform>(entity);
            context.SetComponentData(entity, new NonUniformScale() { Value = transform.localScale });
        }
    }
}
