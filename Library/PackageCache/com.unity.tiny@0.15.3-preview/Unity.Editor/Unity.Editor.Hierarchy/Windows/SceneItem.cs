using Unity.Authoring;
using Unity.Entities;
using Unity.Tiny.Scenes;

namespace Unity.Editor.Hierarchy
{
    internal class SceneItem : HierarchyItem
    {
        private readonly Session m_Session;
        private readonly Scene m_Scene;
        private readonly SceneGraph m_Graph;
        private readonly EntityManager m_EntityManager;
        private readonly IEditorSceneManagerInternal m_SceneManager;

        public SceneItem(Session session, SceneGraph graph)
        {
            m_Session = session;
            m_Graph = graph;
            m_Scene = graph.Scene;
            m_EntityManager = session.GetManager<IWorldManager>().EntityManager;
            m_SceneManager = session.GetManager<IEditorSceneManagerInternal>();
        }

        public SceneGraph Graph => m_Graph;
        public Scene Scene => m_Scene;

        public override string displayName
        {
            get
            {
                var name = m_SceneManager.GetSceneName(m_Scene);
                
                if (IsChanged)
                {
                    name += " *";
                }
                
                return name;
            }
        }

        public override int id => m_Scene.SceneGuid.GetHashCode();

        public override int depth => parent?.depth + 1 ?? 0;

        public bool IsChanged => m_SceneManager.IsSceneChanged(m_Scene);
    }
}
