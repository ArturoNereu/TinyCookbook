using System;
using Unity.Authoring;
using Unity.Authoring.ChangeTracking;
using Unity.Authoring.Undo;
using UnityEditor;
using UnityEngine;

namespace Unity.Editor.Undo
{
    internal interface IEditorUndoManager : ISessionManager
    {
        event Action UndoRedoBatchStarted;
        event Action UndoRedoBatchEnded;
        
        bool IsUndoing { get; }
        bool IsRedoing { get; }
        bool IsUndoRedoing { get; }
    }

    internal class EditorUndoManager : SessionManager, IEditorUndoManager
    {
        internal class UndoObject : ScriptableObject
        {
            private const string k_UndoName = "DOTS Operation";
            private int m_Current = 0;

            [SerializeField]
            private int m_Version = 0;

            public Action Undo;
            public Action Redo;

            public int Version => m_Version;

            public void IncrementVersion()
            {
                UnityEditor.Undo.RecordObject(this, k_UndoName);
                m_Version++;
                m_Current = m_Version;
                EditorUtility.SetDirty(this);
            }

            private void OnEnable()
            {
                UnityEditor.Undo.undoRedoPerformed += HandleUndoRedoPerformed;
            }

            private void OnDisable()
            {
                UnityEditor.Undo.undoRedoPerformed -= HandleUndoRedoPerformed;
            }

            public void Flush()
            {
                UnityEditor.Undo.FlushUndoRecordObjects();
            }

            private void HandleUndoRedoPerformed()
            {
                if (m_Current != m_Version)
                {
                    if (m_Current > m_Version)
                    {
                        Undo?.Invoke();
                    }
                    else
                    {
                        Redo?.Invoke();
                    }

                    m_Current = m_Version;
                }
            }
        }

        private UndoObject m_Undo;
        private IUndoManager m_UndoManager;
        private int m_Version;
        private bool m_IsUndoing;
        private bool m_IsRedoing;

        public event Action UndoRedoBatchStarted = delegate { };
        public event Action UndoRedoBatchEnded = delegate { };
        public bool IsUndoing => m_IsUndoing;
        public bool IsRedoing => m_IsRedoing;
        public bool IsUndoRedoing => m_IsUndoing || m_IsRedoing;
        
        public EditorUndoManager(Session session) : base(session)
        {
        }

        public override void Load()
        {
            m_UndoManager = Session.GetManager<IUndoManager>();

            m_Undo = ScriptableObject.CreateInstance<UndoObject>();
            m_Undo.hideFlags |= HideFlags.HideAndDontSave;
            m_Undo.Undo += HandleUndo;
            m_Undo.Redo += HandleRedo;

            m_UndoManager.ChangeRecorded += HandleChangeRecorded;
        }

        public override void Unload()
        {
            m_Undo.Undo -= HandleUndo;
            m_Undo.Redo -= HandleRedo;

            m_UndoManager.ChangeRecorded -= HandleChangeRecorded;
        }

        private void HandleChangeRecorded()
        {
            m_Undo.IncrementVersion();
            m_Version = m_Undo.Version;
        }

        private void HandleUndo()
        {
            m_IsUndoing = true;
            UndoRedoBatchStarted();

            for (; m_Version > m_Undo.Version; m_Version--)
            {
                m_UndoManager.Undo();
            }

            UndoRedoBatchEnded();
            m_IsUndoing = false;
        }

        private void HandleRedo()
        {
            m_IsRedoing = true;
            UndoRedoBatchStarted();

            for (; m_Version < m_Undo.Version; m_Version++)
            {
                m_UndoManager.Redo();
            }

            UndoRedoBatchEnded();
            m_IsRedoing = false;
        }
    }
}
