using Unity.Authoring;
using Unity.Collections;
using Unity.Entities;

namespace Unity.Tiny.Core2D.Editor
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(SceneConversionSystemGroup))]
    public class DisabledHierarchyOnExportConversionSystem : ComponentSystem
    {
        private EntityQuery m_DisabledEntityQuery;
        protected override void OnCreate()
        {
            m_DisabledEntityQuery = EntityManager.CreateEntityQuery(typeof(Disabled));
            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            var children = new NativeList<Entity>(Allocator.TempJob);
            var allDisabled = m_DisabledEntityQuery.ToEntityArray(Allocator.TempJob);
            try
            {
                if (allDisabled.Length > 0)
                {
                    foreach (var disabled in allDisabled)
                    {
                        GatherChildren(disabled, children);
                    }

                    for (var i = 0; i < children.Length; ++i)
                    {
                        var child = children[i];
                        if (!EntityManager.HasComponent<Disabled>(child))
                        {
                            EntityManager.AddComponent(child, typeof(Disabled));
                        }
                    }
                }
            }
            finally
            {
                allDisabled.Dispose();
                children.Dispose();
            }
        }

        private void GatherChildren(Entity entity, NativeList<Entity> gatherer)
        {
            var children = new NativeList<Entity>(Allocator.TempJob);
            try
            {
                TransformHelpers.GetChildren(this, entity, ref children, true);
                for (var i = 0; i < children.Length; ++i)
                {
                    var child = children[i];
                    gatherer.Add(child);
                    GatherChildren(child, gatherer);
                }
            }
            finally
            {
                children.Dispose();
            }
        }
    }

}
