using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Unity.Authoring;
using Unity.Authoring.ChangeTracking;
using Unity.Editor.Modes;
using Unity.Editor.Undo;
using Unity.Entities;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Editor
{
    internal class EntityInspector : UnityEditor.Editor
    {
        private static readonly List<EntityInspector> s_ActiveInspectors;
        [SerializeField] private InspectorMode m_Mode = InspectorMode.Normal;
        [SerializeField] private InspectorBackendType m_BackendType = InspectorBackendType.UIElements;
        private IInspectorBackend<Entity> m_Backend;
        private Session m_Session;
        private bool m_IsUndoing;

        private VisualElement RootElement { get; set; }

        static EntityInspector()
        {
            s_ActiveInspectors = new List<EntityInspector>();
        }

        public static void ForceRebuildAll()
        {
            foreach (var inspector in s_ActiveInspectors)
            {
                inspector.Refresh();
            }
        }

        public static void RepaintAll()
        {
            foreach (var inspector in s_ActiveInspectors)
            {
                inspector.Repaint();
            }
        }

        [RootEditor, UsedImplicitly]
        private static Type ShouldOverrideInspection(UnityEngine.Object[] objects)
        {
            if (!EditorModes.IsDotsModeActive)
            {
                return null;
            }
            foreach (var obj in objects)
            {
                if (obj is GameObject go && go.GetComponent<EntityReference>() is var goReference && goReference && null != goReference)
                {
                    return typeof(EntityInspector);
                }

                if (obj is Component component && component.GetComponent<EntityReference>() is var cReference && cReference && null != cReference)
                {
                    return typeof(EntityInspector);
                }
            }

            return null;
        }

        protected override void OnHeaderGUI()
        {
            // Disable normal header
        }

        public override bool UseDefaultMargins() => false;

        public override VisualElement CreateInspectorGUI()
        {
            return RootElement;
        }

        private void OnEnable()
        {
            s_ActiveInspectors.Add(this);
            RootElement = new VisualElement { name = "Entity Inspector" };
            RootElement.style.marginBottom = RootElement.style.marginTop = RootElement.style.marginLeft = RootElement.style.marginRight = 0;
            RootElement.style.paddingBottom = RootElement.style.paddingTop = RootElement.style.paddingLeft= RootElement.style.paddingRight= 0;
            RootElement.AddStyleSheetSkinVariant(StyleSheets.Inspector.StyleSheet);

            Application.BeginAuthoringProject += EnterAuthoringContext;
            Application.EndAuthoringProject += ExitAuthoringContext;
            EnterAuthoringContext(Application.AuthoringProject);
        }

        private void EnterAuthoringContext(Project project)
        {
            if (null == project)
            {
                return;
            }

            m_Session = project.Session;

            var changeManager = m_Session.GetManager<IChangeManager>();
            changeManager.RegisterChangeCallback(HandleChanges);
            m_Session.GetManager<IEditorUndoManager>().UndoRedoBatchStarted += HandleUndoStarted;
            m_Session.GetManager<IEditorUndoManager>().UndoRedoBatchEnded += HandleUndoEnded;
            SwitchToBackend(m_BackendType, true);
        }

        private void ExitAuthoringContext(Project project)
        {
            var changeManager = m_Session.GetManager<IChangeManager>();
            changeManager.UnregisterChangeCallback(HandleChanges);
            m_Session.GetManager<IEditorUndoManager>().UndoRedoBatchStarted -= HandleUndoStarted;
            m_Session.GetManager<IEditorUndoManager>().UndoRedoBatchEnded -= HandleUndoEnded;
            m_Session = null;
            Refresh();
        }

        private void HandleChanges(Changes changes)
        {
            if (m_IsUndoing)
            {
                return;
            }
            if (changes.ComponentsWereAdded || changes.ComponentsWereRemoved)
            {
                // TODO: Add or Remove specific data element instead of recreating.
                ForceRebuildAll();
            }
        }
        
        private void HandleUndoStarted()
        {
            m_IsUndoing = true;
        }
        
        private void HandleUndoEnded()
        {
            m_IsUndoing = false;
            ForceRebuildAll();
        }

        private void OnDisable()
        {
            s_ActiveInspectors.Remove(this);
            m_Backend?.OnDestroyed();
            Application.BeginAuthoringProject -= EnterAuthoringContext;
            Application.EndAuthoringProject -= ExitAuthoringContext;
        }

        private IInspectorBackend<Entity> Backend
        {
            get
            {
                if (null == m_Backend)
                {
                    SwitchToBackend(m_BackendType, true);
                }
                return m_Backend;
            }
        }

        private void SwitchToBackend(InspectorBackendType type, bool force = false)
        {
            if (type == m_BackendType && !force)
            {
                return;
            }
            m_BackendType = type;
            m_Backend = GetBackend(m_BackendType);
            m_Backend.OnCreated();
            Refresh();
        }

        private IInspectorBackend<Entity> GetBackend(InspectorBackendType type)
        {
            switch (type)
            {
                case InspectorBackendType.UIElements:
                {
                    return new UIElementsBackend(m_Session, RootElement)
                    {
                        Mode = m_Mode
                    };
                }
                default:
                    throw new ArgumentException("Unknown InspectorBackendType", nameof(type));
            }
        }

        private void Refresh()
        {
            if (null == m_Session)
            {
                Backend.Reset();
                return;
            }

            try
            {
                Backend.Targets.Clear();
                if (null == targets)
                {
                    return;
                }

                using (var pooled = ListPool<Entity>.GetDisposable())
                {
                    var list = pooled.List;
                    list.AddRange(targets.OfType<GameObject>().Where(go => null != go && go).Select(GetEntity));
                    list.AddRange(targets.OfType<Component>().Where(c => null != c && c).Select(GetEntity));
                    Backend.Targets.AddRange(list);
                }
            }
            finally
            {
                Backend.Build();
            }
        }

        private Entity GetEntity(GameObject go) => GetEntity(go.transform);

        private Entity GetEntity(Component component)
        {
            var reference = component.GetComponent<EntityReference>();
            if (reference && null != reference)
            {
                return m_Session.GetManager<IWorldManager>().GetEntityFromGuid(reference.Guid);
            }

            return Entity.Null;
        }
    }


}
