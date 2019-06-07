using System;
using Unity.Authoring;
using Unity.Editor.Persistence;
using Unity.Tiny.Scenes;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Unity.Editor
{
    internal class SceneReferenceInspector : IComponentInspector<SceneReference>
    {
        private InspectorDataProxy<SceneReference> m_Proxy;
        private ObjectField m_ObjectField;
        private IWorldManager m_WorldManager;
        private IPersistenceManager m_PersistenceManager;

        public VisualElement Build(InspectorDataProxy<SceneReference> proxy)
        {
            m_Proxy = proxy;
            m_ObjectField = new ObjectField { label = m_Proxy.Name };
            m_ObjectField.RegisterValueChangedCallback(evt => ValueChanged(evt));
            m_ObjectField.objectType = typeof(SceneAsset);
            m_WorldManager = m_Proxy.Session.GetManager<IWorldManager>();
            m_PersistenceManager = m_Proxy.Session.GetManager<IPersistenceManager>();
            return m_ObjectField;
        }

        private void ValueChanged(ChangeEvent<Object> evt)
        {
            if (evt.newValue && null != evt.newValue)
            {
                var sceneAsset = evt.newValue as SceneAsset;
                m_Proxy.Data = new SceneReference { SceneGuid = new Guid(sceneAsset.Guid) };
            }
            else
            {
                m_Proxy.Data = SceneReference.Null;
            }
        }

        public void Update(InspectorDataProxy<SceneReference> proxy)
        {
            var sceneReference = proxy.Data;
            var assetPath = m_PersistenceManager.GetSceneAssetPath(sceneReference.SceneGuid);
            var sceneAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<SceneAsset>(assetPath);
            m_ObjectField.SetValueWithoutNotify(sceneAsset);
        }
    }
}
