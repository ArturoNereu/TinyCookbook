using JetBrains.Annotations;
using Unity.Editor.Bindings;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Unity.Tiny.Core2D.Editor
{
    internal abstract class AutoAddTransformBindings : IEntityBinding
    {
        public void LoadBinding(Entity entity, IBindingContext context)
        {
        }

        public void UnloadBinding(Entity entity, IBindingContext context)
        {
        }

        public void TransferToUnityComponents(Entity entity, IBindingContext context)
        {
            AddComponentIfDataIsNotDefault(entity, context);
        }

        public void TransferFromUnityComponents(Entity entity, IBindingContext context)
        {
            AddComponentIfDataIsNotDefault(entity, context);
        }

        protected abstract void AddComponentIfDataIsNotDefault(Entity entity, IBindingContext context);
    }

    [UsedImplicitly]
    internal class AutoAddTranslationBindings : AutoAddTransformBindings,
        IExcludeComponentBinding<Translation>
    {
        protected override void AddComponentIfDataIsNotDefault(Entity entity, IBindingContext context)
        {
            var transform = context.GetUnityComponent<UnityEngine.Transform>(entity);
            var position = transform.localPosition;
            if (!Mathf.Approximately(position.x, 0.0f) ||
                !Mathf.Approximately(position.y, 0.0f) ||
                !Mathf.Approximately(position.z, 0.0f))
            {
                context.AddComponentData(entity, new Translation { Value = position });
            }
        }
    }

    [UsedImplicitly]
    internal class AutoAddRotationBindings : AutoAddTransformBindings,
        IExcludeComponentBinding<Rotation>
    {
        private static readonly quaternion Identity = quaternion.identity;
        protected override void AddComponentIfDataIsNotDefault(Entity entity, IBindingContext context)
        {
            var transform = context.GetUnityComponent<UnityEngine.Transform>(entity);
            var rotation = transform.localRotation;
            if (!Mathf.Approximately(rotation.x, Identity.value.x) ||
                !Mathf.Approximately(rotation.y, Identity.value.y) ||
                !Mathf.Approximately(rotation.z, Identity.value.z) ||
                !Mathf.Approximately(rotation.w, Identity.value.w))
            {
                context.AddComponentData(entity, new Rotation { Value = rotation });
            }
        }
    }

    [UsedImplicitly]
    internal class AutoAddNonUniformScaleBindings : AutoAddTransformBindings,
        IExcludeComponentBinding<NonUniformScale, Scale>
    {
        protected override void AddComponentIfDataIsNotDefault(Entity entity, IBindingContext context)
        {
            var transform = context.GetUnityComponent<UnityEngine.Transform>(entity);
            var scale = transform.localScale;

            if (!Mathf.Approximately(scale.x, 1.0f) ||
                !Mathf.Approximately(scale.y, 1.0f) ||
                !Mathf.Approximately(scale.z, 1.0f))
            {
                context.AddComponentData(entity, new NonUniformScale { Value = scale });
            }
        }
    }
}
