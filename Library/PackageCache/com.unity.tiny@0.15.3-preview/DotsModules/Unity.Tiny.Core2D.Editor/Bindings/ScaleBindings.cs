using JetBrains.Annotations;
using Unity.Entities;
using Unity.Editor.Bindings;

namespace Unity.Tiny.Core2D.Editor
{
    [UsedImplicitly]
    internal class ScaleBindings : IEntityBinding,
        IComponentBinding<Scale>,
        IExcludeComponentBinding<NonUniformScale>,
        IBindingDependency<ParentBindings>
    {
        public void LoadBinding(Entity entity, IBindingContext context)
        {
            // Nothing to do.
        }

        public void UnloadBinding(Entity entity, IBindingContext context)
        {
            var transform = context.GetUnityComponent<UnityEngine.Transform>(entity);
            // If this binding is being unloaded, but the entity has a NonUniformScale (because it
            // was swapped at some point), set the right value.
            if (context.HasComponent<NonUniformScale>(entity))
                transform.localScale = context.GetComponentData<NonUniformScale>(entity).Value;
            else
                transform.localScale = UnityEngine.Vector3.one;
        }

        public void TransferToUnityComponents(Entity entity, IBindingContext context)
        {
            var transform = context.GetUnityComponent<UnityEngine.Transform>(entity);
            var v = context.GetComponentData<Scale>(entity).Value;
            transform.localScale = new UnityEngine.Vector3(v, v, v);
        }

        public void TransferFromUnityComponents(Entity entity, IBindingContext context)
        {
            var transform = context.GetUnityComponent<UnityEngine.Transform>(entity);
            // If the Unity scale isn't uniform, then we upgrade this to a NonUniformScale, and let
            // that binding take over.  We do an explicit == check even on the floating point values,
            // because the only case we want to handle is when the same bit value is really assigned
            // to all of x/y/z.
            var scale = transform.localScale;
            if (scale.x == scale.y &&
                scale.x == scale.z)
            {
                context.SetComponentData(entity, new Scale() { Value = scale.x });
            }
            else
            {
                context.RemoveComponent<Scale>(entity);
                // depending on the order that the bindings transfer/unload is called,
                // this might have been added already (even if this particular binding is Exclude<NonUniformScale>
                if (context.HasComponent<NonUniformScale>(entity))
                    context.SetComponentData(entity, new NonUniformScale() { Value = scale });
                else
                    context.AddComponentData(entity, new NonUniformScale() { Value = scale });
            }
        }
    }

    [UsedImplicitly]
    internal class BothScaleBindings : IEntityBinding,
        IComponentBinding<Scale, NonUniformScale>,
        IBindingDependency<ParentBindings>
    {
        public void LoadBinding(Entity entity, IBindingContext context)
        {
            // Nothing to do.
        }

        public void UnloadBinding(Entity entity, IBindingContext context)
        {
            var transform = context.GetUnityComponent<UnityEngine.Transform>(entity);
            transform.localScale = UnityEngine.Vector3.one;
        }

        public void TransferToUnityComponents(Entity entity, IBindingContext context)
        {
            // There's a conflict because both are present.  Transforms treats this as 1, so we do too.
            var transform = context.GetUnityComponent<UnityEngine.Transform>(entity);
            transform.localScale = UnityEngine.Vector3.one;
        }

        public void TransferFromUnityComponents(Entity entity, IBindingContext context)
        {
            var transform = context.GetUnityComponent<UnityEngine.Transform>(entity);

            // There's a conflcit because both are present, but we're going to "fix" this and leave
            // just a NonUniformScale on here if the user modifies it using the editor.
            context.RemoveComponent<Scale>(entity);
            context.SetComponentData(entity, new NonUniformScale() { Value = transform.localScale });
        }
    }
}
