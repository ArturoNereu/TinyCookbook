using Unity.Authoring;
using Unity.Authoring.Core;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Tiny.Scenes;
using Unity.Tiny.UILayout;
using UnityEngine;

namespace Unity.Editor
{
    internal interface IArchetypeManager : ISessionManagerInternal
    {
        EntityArchetype Empty { get; }
        EntityArchetype Config { get; }
        EntityArchetype Camera { get; }
        EntityArchetype Sprite { get; }
        EntityArchetype SpriteSequence { get; }
        EntityArchetype UICanvas { get;  }
        EntityArchetype FromGameObject(GameObject go);
        EntityArchetype AudioSource { get; }
        EntityArchetype VideoSource { get;  }
    }

    internal class ArchetypeManager : SessionManager, IArchetypeManager
    {
        public ArchetypeManager(Session session) : base(session)
        {
            var entityManager = session.GetManager<IWorldManager>().EntityManager;
            Empty = entityManager.CreateArchetype(
                typeof(Parent),
                typeof(Translation),
                typeof(Rotation),
                typeof(NonUniformScale),
                typeof(EntityGuid),
                typeof(SceneGuid),
                typeof(SceneInstanceId),
                typeof(EntityName),
                typeof(SiblingIndex));

            Config = entityManager.CreateArchetype(
                typeof(EntityGuid),
                typeof(ConfigurationTag),
                typeof(Scenes),
                typeof(StartupScenes),
                typeof(DisplayInfo));

            Camera = entityManager.CreateArchetype(
                typeof(Parent),
                typeof(Translation),
                typeof(Rotation),
                typeof(NonUniformScale),
                typeof(EntityGuid),
                typeof(SceneGuid),
                typeof(SceneInstanceId),
                typeof(EntityName),
                typeof(Camera2D),
                typeof(SiblingIndex));

            Sprite = entityManager.CreateArchetype(
                typeof(Parent),
                typeof(Translation),
                typeof(Rotation),
                typeof(NonUniformScale),
                typeof(EntityGuid),
                typeof(SceneGuid),
                typeof(SceneInstanceId),
                typeof(EntityName),
                typeof(Sprite2DRenderer),
                typeof(SiblingIndex),
                typeof(LayerSorting));
            
            SpriteSequence = entityManager.CreateArchetype(
                typeof(Parent),
                typeof(Translation),
                typeof(Rotation),
                typeof(NonUniformScale),
                typeof(EntityGuid),
                typeof(SceneGuid),
                typeof(SceneInstanceId),
                typeof(EntityName),
                typeof(Sprite2DRenderer),
                typeof(SiblingIndex),
                typeof(Sprite2DSequence),
                typeof(Sprite2DSequenceOptions),
                typeof(Sprite2DSequencePlayer),
                typeof(LayerSorting));

            AudioSource = entityManager.CreateArchetype(
                typeof(Unity.Tiny.Audio.AudioSource),
                typeof(Unity.Tiny.Audio.AudioSourceStart),
                typeof(EntityName),
                typeof(EntityGuid),
                typeof(SceneGuid),
                typeof(SceneInstanceId),
                typeof(Parent),
                typeof(SiblingIndex));

            VideoSource = entityManager.CreateArchetype(
                typeof(Unity.Tiny.Video.VideoPlayer),
                typeof(EntityName),
                typeof(EntityGuid),
                typeof(SceneGuid),
                typeof(SceneInstanceId),
                typeof(Parent),
                typeof(SiblingIndex));
            
            UICanvas = entityManager.CreateArchetype(
                typeof(Parent),
                typeof(Translation),
                typeof(Rotation),
                typeof(NonUniformScale),
                typeof(EntityGuid),
                typeof(SceneGuid),
                typeof(SceneInstanceId),
                typeof(EntityName),
                typeof(SiblingIndex),
                typeof(Tiny.UILayout.RectTransform),
                typeof(UICanvas));
        }

        public EntityArchetype Empty { get; }
        public EntityArchetype Config { get; }
        public EntityArchetype Camera { get; }
        public EntityArchetype Sprite { get; }
        public EntityArchetype SpriteSequence { get; }
        public EntityArchetype UICanvas { get; }
        public EntityArchetype AudioSource { get; }
        public EntityArchetype VideoSource { get; }

        public unsafe EntityArchetype FromGameObject(GameObject go)
        {
            // TODO: this method should assemble an archetype using binding callbacks
            // For now it's hardcoded for a few core data types
            
            var entityManager = Session.GetManager<IWorldManager>().EntityManager;
            using (var componentTypes = new NativeList<ComponentType>(32, Allocator.Temp))
            {
                componentTypes.Add(typeof(Parent));
                componentTypes.Add(typeof(Translation));
                componentTypes.Add(typeof(Rotation));
                componentTypes.Add(typeof(NonUniformScale));
                componentTypes.Add(typeof(EntityGuid));
                componentTypes.Add(typeof(SceneGuid));
                componentTypes.Add(typeof(SceneInstanceId));
                componentTypes.Add(typeof(EntityName));
                componentTypes.Add(typeof(SiblingIndex));

                if (go.GetComponent<Renderer>())
                {
                    componentTypes.Add(typeof(LayerSorting));
                }

                if (go.GetComponent<UnityEngine.Rendering.SortingGroup>())
                {
                    componentTypes.Add(typeof(SortingGroup));
                }

                if (go.GetComponent<SpriteRenderer>())
                {
                    componentTypes.Add(typeof(Sprite2DRenderer));
                    componentTypes.Add(typeof(Sprite2DRendererOptions));
                }

                if (go.GetComponent<Camera>())
                {
                    componentTypes.Add(typeof(Camera2D));
                    componentTypes.Add(typeof(Camera2DClippingPlanes));
                    componentTypes.Add(typeof(Camera2DAxisSort));
                }

                if (go.GetComponent<UnityEngine.AudioClip>())
                {
                    componentTypes.Add(typeof(Unity.Tiny.Audio.AudioSource));
                }

                if (go.GetComponent<UnityEngine.Video.VideoClip>())
                {
                    componentTypes.Add(typeof(Unity.Tiny.Video.VideoPlayer));
                }

                return entityManager.CreateArchetype((ComponentType*)componentTypes.GetUnsafePtr(), componentTypes.Length);
            }
        }
    }
}