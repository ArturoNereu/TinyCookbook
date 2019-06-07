using JetBrains.Annotations;
using Unity.Entities;
using Unity.Editor.Bindings;

namespace Unity.Tiny.Core2D.Editor
{
    [UsedImplicitly]
    internal class TranslationBindings : IEntityBinding,
        IComponentBinding<Translation>,
        IBindingDependency<ParentBindings>
    {
        public void LoadBinding(Entity entity, IBindingContext context)
        {
            // Nothing to do.
        }

        public void UnloadBinding(Entity entity, IBindingContext context)
        {
            var transform = context.GetUnityComponent<UnityEngine.Transform>(entity);
            transform.localPosition = UnityEngine.Vector3.zero;
        }

        public void TransferToUnityComponents(Entity entity, IBindingContext context)
        {
            var transform = context.GetUnityComponent<UnityEngine.Transform>(entity);
            transform.localPosition = context.GetComponentData<Translation>(entity).Value;
        }

        public void TransferFromUnityComponents(Entity entity, IBindingContext context)
        {
            var transform = context.GetUnityComponent<UnityEngine.Transform>(entity);
            context.SetComponentData(entity, new Translation(){ Value = transform.localPosition });
        }
    }
}
