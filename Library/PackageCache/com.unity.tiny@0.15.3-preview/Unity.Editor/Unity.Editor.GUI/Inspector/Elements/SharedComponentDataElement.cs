using System;
using System.Reflection;
using Unity.Authoring;
using Unity.Collections;
using Unity.Entities;

namespace Unity.Editor
{
    internal class SharedComponentDataElement<TComponentData> : DataElement<TComponentData>
        where TComponentData : struct, ISharedComponentData
    {
        protected override bool HasData(Entity entity)
        {
            return EntityManager.HasComponent<TComponentData>(entity);
        }

        protected override TComponentData GetData(Entity entity)
            => EntityManager.GetSharedComponentData<TComponentData>(entity);

        protected override void SetData(Entity entity, TComponentData data)
            => EntityManager.SetSharedComponentData(entity, data);

        protected override void RemoveComponent(Entity entity)
        {
            EntityManager.RemoveComponentRaw(entity, TypeManager.GetTypeIndex(typeof(TComponentData)));
        }

        protected override Assembly Assembly => typeof(TComponentData).Assembly;

        protected override bool IsTagComponent => false;
        public override Type ComponentType { get; } = typeof(TComponentData);

        public SharedComponentDataElement(Session session, NativeArray<Entity> targets, IInspector<TComponentData> inspector)
            :base(session, targets, inspector)
        {
            AddToClassList("unity-ecs-component");
        }
    }
}
