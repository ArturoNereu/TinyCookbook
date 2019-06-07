using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Authoring;
using Unity.Editor.Hierarchy;
using Unity.Tiny.Scenes;
using UnityEditor;

namespace Unity.Editor.MenuItems
{
    internal static class EditMenuItems
    {
        [UsedImplicitly, CommandHandler(CommandIds.Edit.SelectionBasedValidation, CommandHint.Menu | CommandHint.Validate)]
        private static void ValidateDuplicateSelection(CommandExecuteContext context)
        {
            context.result = !SelectionUtility.IsEntitySelectionEmpty();
        }

        [UsedImplicitly, CommandHandler(CommandIds.Edit.DuplicateSelection, CommandHint.Menu)]
        public static void DuplicateSelection(CommandExecuteContext context)
        {
            var session = Application.AuthoringProject?.Session;
            var worldManager = session.GetManager<IWorldManager>();
            var m_EntityManager = worldManager.EntityManager;
            var m_SceneManager = session.GetManager<IEditorSceneManagerInternal>();

            using (var pooled = ListPool<ISceneGraphNode>.GetDisposable())
            {
                var toSelect = pooled.List;
                var selection = SelectionUtility.GetEntityGuidSelection();

                foreach (var group in selection.Select(worldManager.GetEntityFromGuid)
                    .GroupBy(e => m_EntityManager.GetSharedComponentData<SceneGuid>(e)))
                {
                    var graph = m_SceneManager.GetGraphForScene(group.Key);
                    var list = group.Select(graph.FindNode).Cast<ISceneGraphNode>().ToList();
                    toSelect.AddRange(graph.Duplicate(list));
                }

                EntityHierarchyWindow.SelectOnNextPaint(toSelect.OfType<EntityNode>().Select(e => e.Guid).ToList());
            }
        }

        [UsedImplicitly, CommandHandler(CommandIds.Edit.DeleteSelection, CommandHint.Menu)]
        public static void DeleteSelection(CommandExecuteContext context)
        {
            var session = Application.AuthoringProject?.Session;
            var worldManager = session.GetManager<IWorldManager>();
            var m_EntityManager = worldManager.EntityManager;
            var m_SceneManager = session.GetManager<IEditorSceneManagerInternal>();

            var selection = SelectionUtility.GetEntityGuidSelection();
            foreach (var group in selection.Select(worldManager.GetEntityFromGuid).GroupBy(e => m_EntityManager.GetSharedComponentData<SceneGuid>(e)))
            {
                var graph = m_SceneManager.GetGraphForScene(group.Key);
                foreach (var node in group.Select(graph.FindNode))
                {
                    graph.Delete(node);
                }
            }
        }

        //[UsedImplicitly, CommandHandler(CommandIds.Edit.Play)]
        //public static void Play(CommandExecuteContext context)
        //{
        //}

        //[UsedImplicitly, CommandHandler(CommandIds.Edit.Pause)]
        //public static void Pause(CommandExecuteContext context)
        //{
        //}
    }
}
