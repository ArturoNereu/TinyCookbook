using JetBrains.Annotations;
using Unity.Entities;
using Unity.Editor.Bindings;
using UnityEngine;

namespace Unity.Tiny.Core2D.Editor
{
    [UsedImplicitly]
    internal class RotationBindings : IEntityBinding,
        IComponentBinding<Rotation>,
        IBindingDependency<ParentBindings>
    {
        public void LoadBinding(Entity entity, IBindingContext context)
        {
            // Nothing to do.
        }

        public void UnloadBinding(Entity entity, IBindingContext context)
        {
            var transform = context.GetUnityComponent<UnityEngine.Transform>(entity);
            transform.localRotation = Quaternion.identity;
        }

        public void TransferToUnityComponents(Entity entity, IBindingContext context)
        {
            var transform = context.GetUnityComponent<UnityEngine.Transform>(entity);
            transform.localRotation = context.GetComponentData<Rotation>(entity).Value;
        }

        public void TransferFromUnityComponents(Entity entity, IBindingContext context)
        {
            var transform = context.GetUnityComponent<UnityEngine.Transform>(entity);
            context.SetComponentData(entity, new Rotation() {Value = transform.localRotation});
        }
    }
}
