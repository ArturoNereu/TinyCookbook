using JetBrains.Annotations;
using System;
using System.Linq;
using Unity.Authoring;
using Unity.Authoring.Core;
using Unity.Editor.Extensions;
using Unity.Editor.Hierarchy;
using Unity.Entities;
using Unity.Tiny.Core2D;
using Unity.Tiny.Scenes;
using Unity.Tiny.UILayout;
using UnityEditor;
using UnityEngine.Assertions;

namespace Unity.Editor.MenuItems
{
    internal static class EntityMenuItems
    {
        private static Entity CreateEntity(EntityArchetype archetype, string name, Guid parentGuid = default)
        {
            var session = Application.AuthoringProject.Session;
            var worldManager = session.GetManager<IWorldManager>();
            var entityManager = worldManager.EntityManager;
            var sceneManager = session.GetManager<IEditorSceneManagerInternal>();

            var parent = worldManager.GetEntityFromGuid(parentGuid);
            var parentExists = parent != Entity.Null && entityManager.Exists(parent);
            var scene = !parentExists ? sceneManager.GetActiveScene() : sceneManager.GetScene(parent);
            Assert.AreNotEqual(scene, Scene.Null);
            
            var graph = sceneManager.GetGraphForScene(scene);
            var uniqueName = EntityNameHelper.GetUniqueEntityName(name, worldManager, parentExists ? graph.FindNode(parent)?.Children : graph.Roots);

            var entity = worldManager.CreateEntity(uniqueName, archetype);
            scene.AddEntityReference(entityManager, entity);

            if (parentExists)
            {
                if (entityManager.HasComponent<Parent>(entity))
                {
                    entityManager.SetComponentData(entity, new Parent { Value = parent });
                }
                else
                {
                    entityManager.AddComponentData(entity, new Parent { Value = parent });
                }
            }

            SetComponentDefaultValue<NonUniformScale>(entity);
            SetComponentDefaultValue<SiblingIndex>(entity);

            EntityHierarchyWindow.SelectOnNextPaint(worldManager.GetEntityGuid(entity).AsEnumerable().ToList());
            return entity;
        }

        private static void SetComponentDefaultValue<T>(Entity entity)
            where T : struct, IComponentData
        {
            var session = Application.AuthoringProject.Session;
            var worldManager = session.GetManager<IWorldManager>();
            var entityManager = worldManager.EntityManager;
            if (entityManager.HasComponent<T>(entity))
            {
                entityManager.SetComponentData(entity, DomainCache.GetDefaultValue<T>());
            }
        }

        [UsedImplicitly, CommandHandler(CommandIds.Entity.EntityCreationValidation, CommandHint.Menu | CommandHint.Validate)]
        public static void ValidateEntityCreation(CommandExecuteContext context)
        {
            context.result = Application.AuthoringProject != null;
        }

        [UsedImplicitly, CommandHandler(CommandIds.Entity.CreateEmpty, CommandHint.Menu)]
        public static void CreateEmpty(CommandExecuteContext context = null)
        {
            CreateEntity(Application.AuthoringProject.Session.GetManager<IArchetypeManager>().Empty, "Entity");
        }

        [UsedImplicitly, CommandHandler(CommandIds.Entity.CreateEmptyChild, CommandHint.Menu)]
        public static void CreateEmptyChild(CommandExecuteContext context = null)
        {
            var session = Application.AuthoringProject.Session;
            var worldManager = session.GetManager<IWorldManager>();
            using (var pooled = ListPool<Entity>.GetDisposable())
            {
                var list = pooled.List;
                foreach (var guid in SelectionUtility.GetEntityGuidSelection())
                {
                    list.Add(CreateEntity(Application.AuthoringProject.Session.GetManager<IArchetypeManager>().Empty, "Entity", guid));
                }

                if (list.Count == 0)
                {
                    list.Add(CreateEntity(Application.AuthoringProject.Session.GetManager<IArchetypeManager>().Empty, "Entity", default));
                }

                EntityHierarchyWindow.SelectOnNextPaint(list.Select(worldManager.GetEntityGuid).ToList());
            }
        }

        [UsedImplicitly, CommandHandler(CommandIds.Entity.CreateAudioSource, CommandHint.Menu)]
        public static void AudioSource(CommandExecuteContext context)
        {
            var entity = CreateEntity(Application.AuthoringProject.Session.GetManager<IArchetypeManager>().AudioSource, "AudioSource");
        }

        [UsedImplicitly, CommandHandler(CommandIds.Entity.CreateCamera, CommandHint.Menu)]
        public static void Camera(CommandExecuteContext context)
        {
            var entity = CreateEntity(Application.AuthoringProject.Session.GetManager<IArchetypeManager>().Camera, "Camera");
            SetComponentDefaultValue<Camera2D>(entity);
        }

        [UsedImplicitly, CommandHandler(CommandIds.Entity.CreateSprite, CommandHint.Menu)]
        public static void Sprite(CommandExecuteContext context)
        {
            var entity = CreateEntity(Application.AuthoringProject.Session.GetManager<IArchetypeManager>().Sprite, "Sprite");
            SetComponentDefaultValue<Sprite2DRenderer>(entity);
        }

        [UsedImplicitly, CommandHandler(CommandIds.Entity.CreateCanvas, CommandHint.Menu)]
        public static void UICanvas(CommandExecuteContext context)
        {
            var entity = CreateEntity(Application.AuthoringProject.Session.GetManager<IArchetypeManager>().UICanvas, "UICanvas");
            SetComponentDefaultValue<UICanvas>(entity);
        }
    }
}
