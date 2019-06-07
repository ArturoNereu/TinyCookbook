using System;
using System.Linq;
using Unity.Authoring;
using Unity.Authoring.Hashing;
using Unity.Editor.Hierarchy;
using Unity.Tiny.Core2D;
using Unity.Tiny.Video;
using Unity.Tiny.Audio;
using UnityEditor;

namespace Unity.Editor
{
    internal static class HierarchyDragAndDrop<T, TKey>
    {
        public static Type KeyType { get; } = typeof(TKey);
        
        public delegate ISceneGraphNode SingleDropHandler(Session session, T t, SceneGraph graph, EntityNode parent, int index);
        public static SingleDropHandler SingleObjectDrop;
    }

    [InitializeOnLoad]
    internal static class BuiltinHierarchyDragAndDrop
    {
        static BuiltinHierarchyDragAndDrop()
        {
            HierarchyDragAndDrop<UnityEngine.Sprite, EntityHierarchyTree.Key>.SingleObjectDrop = (session, sprite, graph, parent, index) =>
            {
                var worldManager = session.GetManager<IWorldManager>();
                var entityManager = worldManager.EntityManager;
                var archetypeManager = session.GetManager<IArchetypeManager>();
                var assetManager = session.GetManager<IAssetManager>();

                var entity = entityManager.CreateEntity(archetypeManager.Sprite);
                var renderer = DomainCache.GetDefaultValue<Sprite2DRenderer>();
                renderer.sprite = assetManager.GetEntity(sprite);
                worldManager.SetEntityName(entity, sprite.name);
                entityManager.SetComponentData(entity, renderer);
                entityManager.SetComponentData(entity, DomainCache.GetDefaultValue<Rotation>());
                entityManager.SetComponentData(entity, DomainCache.GetDefaultValue<NonUniformScale>());
                entityManager.SetComponentData(entity, Guid.NewGuid().ToEntityGuid());

                var node = new EntityNode(graph, session, entity);
                if (null != parent)
                {
                    parent.Insert(index, node);
                }
                else
                {
                    graph.Insert(index, node);
                }
                
                return node;
            };
            
            HierarchyDragAndDrop<UnityEngine.Texture2D, EntityHierarchyTree.Key>.SingleObjectDrop = (session, texture, graph, parent, index) =>
            {
                if (null == texture)
                {
                    return null;
                }
                
                var path = AssetDatabase.GetAssetPath(texture);
                var sprites = AssetDatabase.LoadAllAssetsAtPath( path ).OfType<UnityEngine.Sprite>().ToArray();
                if (sprites.Length == 0)
                {
                    return null;
                }
                
                // Create normal Sprite2DRenderer
                if (sprites.Length == 1)
                {
                    return HierarchyDragAndDrop<UnityEngine.Sprite, EntityHierarchyTree.Key>.SingleObjectDrop(session, sprites[0], graph,
                        parent, index);
                }
                
                // Create Sprite2DRendererSequence
                var worldManager = session.GetManager<IWorldManager>();
                var entityManager = worldManager.EntityManager;
                var archetypeManager = session.GetManager<IArchetypeManager>();
                var assetManager = session.GetManager<IAssetManager>();

                var entity = entityManager.CreateEntity(archetypeManager.SpriteSequence);
                var renderer = DomainCache.GetDefaultValue<Sprite2DRenderer>();
                renderer.sprite = assetManager.GetEntity(sprites[0]);
                worldManager.SetEntityName(entity, texture.name);
                entityManager.SetComponentData(entity, renderer);
                entityManager.SetComponentData(entity, DomainCache.GetDefaultValue<Rotation>());
                entityManager.SetComponentData(entity, DomainCache.GetDefaultValue<NonUniformScale>());
                entityManager.SetComponentData(entity, Guid.NewGuid().ToEntityGuid());
                entityManager.SetComponentData(entity, new Sprite2DSequencePlayer { loop = LoopMode.Loop, sequence = entity, paused = false, speed = 1, time = 0});
                entityManager.SetComponentData(entity, new Sprite2DSequenceOptions{ frameRate = 20 });
                
                // [HACK] We need to prime the asset entities, because if they don't already exist, generating them will
                // cause the buffer to be invalid.
                foreach (var sprite in sprites)
                {
                    assetManager.GetEntity(sprite);
                }
                
                var sequence = entityManager.GetBuffer<Sprite2DSequence>(entity);
                foreach (var sprite in sprites)
                {
                    sequence.Add(new Sprite2DSequence{ e = assetManager.GetEntity(sprite)});
                }
                
                var node = new EntityNode(graph, session, entity);
                if (null != parent)
                {
                    parent.Insert(index, node);
                }
                else
                {
                    graph.Insert(index, node);
                }

                return node;
            };

            //Create audio source
            HierarchyDragAndDrop<UnityEngine.AudioClip, EntityHierarchyTree.Key>.SingleObjectDrop = (session, audioClip, graph, parent, index) =>
            {
                var worldManager = session.GetManager<IWorldManager>();
                var entityManager = worldManager.EntityManager;
                var archetypeManager = session.GetManager<IArchetypeManager>();
                var assetManager = session.GetManager<IAssetManager>();

                var entitySource = entityManager.CreateEntity(archetypeManager.AudioSource);
                worldManager.SetEntityName(entitySource, audioClip.name);
                entityManager.SetComponentData(entitySource, new AudioSource() {
                   clip = assetManager.GetEntity(audioClip),
                   volume = 1.0f
                });
                entityManager.SetComponentData(entitySource, Guid.NewGuid().ToEntityGuid());

                var node = new EntityNode(graph, session, entitySource);
                if (null != parent)
                {
                    parent.Insert(index, node);
                }
                else
                {
                    graph.Insert(index, node);
                }

                return node;

            };

            HierarchyDragAndDrop<UnityEngine.Video.VideoClip, EntityHierarchyTree.Key>.SingleObjectDrop = (session, videoClip, graph, parent, index) =>
            {
                var worldManager = session.GetManager<IWorldManager>();
                var entityManager = worldManager.EntityManager;
                var archetypeManager = session.GetManager<IArchetypeManager>();
                var assetManager = session.GetManager<IAssetManager>();

                var entitySource = entityManager.CreateEntity(archetypeManager.VideoSource);
                worldManager.SetEntityName(entitySource, videoClip.name);
                entityManager.SetComponentData(entitySource, new VideoPlayer()
                {
                    clip = assetManager.GetEntity(videoClip),
                    controls = true
                });
                entityManager.SetComponentData(entitySource, Guid.NewGuid().ToEntityGuid());

                var node = new EntityNode(graph, session, entitySource);
                if (null != parent)
                {
                    parent.Insert(index, node);
                }
                else
                {
                    graph.Insert(index, node);
                }

                return node;

            };
        }
    }
}