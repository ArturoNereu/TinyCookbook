using System;
using System.Reflection;
using Unity.Authoring;
using Unity.Collections;
using Unity.Entities;

namespace Unity.Editor
{
    internal class ComponentDataElement<TComponentData> : DataElement<TComponentData>
        where TComponentData : struct, IComponentData
    {
        protected override bool HasData(Entity entity)
        {
            return EntityManager.HasComponent<TComponentData>(entity);
        }

        protected override TComponentData GetData(Entity entity)
            => EntityManager.GetComponentData<TComponentData>(entity);

        protected override void SetData(Entity entity, TComponentData data)
            => EntityManager.SetComponentData(entity, data);

        protected override void RemoveComponent(Entity entity)
        {
            EntityManager.RemoveComponentRaw(entity, TypeManager.GetTypeIndex(typeof(TComponentData)));
        }

        protected override Assembly Assembly => typeof(TComponentData).Assembly;

        protected override bool IsTagComponent => TypeManager.IsZeroSized(TypeManager.GetTypeIndex<TComponentData>());
        public override Type ComponentType { get; } = typeof(TComponentData);

        public ComponentDataElement(Session session, NativeArray<Entity> targets, IInspector<TComponentData> inspector)
            :base(session, targets, inspector)
        {
            AddToClassList("unity-ecs-component");
        }
    }
}
