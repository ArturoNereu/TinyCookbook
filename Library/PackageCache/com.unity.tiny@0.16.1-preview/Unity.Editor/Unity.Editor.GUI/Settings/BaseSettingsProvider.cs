using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Editor
{
    internal abstract class BaseSettingsProvider : SettingsProvider
    {
        private VisualElement m_RootElement;
        private VisualElement m_SettingsUI;
        private readonly Label m_NoProjectLabel;
        protected Project Project { get; private set; }
        private static GUIStyle s_headerStyle;
        protected static GUIStyle HeaderStyle => s_headerStyle ?? (s_headerStyle = "SettingsHeader");
        protected Rect LocalBound => m_RootElement.localBound;

        protected BaseSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) 
            : base(path, scopes, keywords)
        {
            m_NoProjectLabel = new Label
            {
                text = "No DOTS project is currently opened."
            };
            m_NoProjectLabel.style.marginLeft = 15;
            m_NoProjectLabel.style.marginTop = 5;
        }

        protected abstract VisualElement CreateSettingsGUI(string searchContext);
        protected abstract void OnBeginAuthoring(Project project);
        protected abstract void OnEndAuthoring(Project project);

        public sealed override void OnActivate(string searchContext, VisualElement rootElement)
        {
            //TODO use UIE styles for a label.
            m_RootElement = rootElement;
            m_RootElement.Add(new IMGUIContainer(() =>
            {
                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                GUILayout.Space(10);
                GUILayout.Label(label, HeaderStyle);
                GUILayout.EndHorizontal();
                
                EditorGUILayout.Space();
            }));

            m_RootElement.Add(m_NoProjectLabel);
            m_SettingsUI = CreateSettingsGUI(searchContext);
            m_RootElement.Add(m_SettingsUI);
            
            Application.BeginAuthoringProject += BeginAuthoringProject;
            Application.EndAuthoringProject += EndAuthoringProject;
            if (Application.AuthoringProject != null)
            {
                BeginAuthoringProject(Application.AuthoringProject);
            }
            else
            {
                UpdateSettingsView();
            }
            
            base.OnActivate(searchContext, rootElement);
        }
        
        public sealed override void OnDeactivate()
        {
            Application.BeginAuthoringProject -= BeginAuthoringProject;
            Application.EndAuthoringProject -= EndAuthoringProject;
            if (Application.AuthoringProject != null)
            {
                EndAuthoringProject(Application.AuthoringProject);
            }
           
            base.OnDeactivate();
        }
        
        protected void BeginAuthoringProject(Project project)
        {
            Project = project;
            OnBeginAuthoring(project);
            UpdateSettingsView();
        }
        
        protected void EndAuthoringProject(Project project)
        {
            OnEndAuthoring(project);
            
            Project = null;
            UpdateSettingsView();
        }

        private void UpdateSettingsView()
        {
            if (Project == null)
            {
                if (m_SettingsUI != null)
                {
                    m_SettingsUI.style.display = DisplayStyle.None;
                }
                m_NoProjectLabel.style.display = DisplayStyle.Flex;
            }
            else
            {
               m_SettingsUI.style.display = DisplayStyle.Flex;
               m_NoProjectLabel.style.display = DisplayStyle.None;
            }
        }
    }
}