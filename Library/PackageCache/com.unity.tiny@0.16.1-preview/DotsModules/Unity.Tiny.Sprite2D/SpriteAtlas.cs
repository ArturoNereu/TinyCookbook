using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Tiny.Core;

namespace Unity.Tiny.Core2D
{
    /// <summary>
    ///  A component describing a list of <see cref="Sprite2D"/> packed in an image atlas.
    /// </summary>
    public struct SpriteAtlas : IBufferElementData
    {
        /// <summary>
        ///  List of <see cref="Sprite2D"/> found in the <see cref="Image2D"/> atlas.
        /// </summary>
        [EntityWithComponents(typeof(Sprite2D))]
        public Entity sprite;
    }

    internal class SpriteAtlasSystem : ComponentSystem
    {
        public int GetSpriteCount(Entity atlas)
        {
            if (!EntityManager.Exists(atlas) || !EntityManager.HasComponent<SpriteAtlas>(atlas))
            {
                return 0;
            }

            return EntityManager.GetBuffer<SpriteAtlas>(atlas).Length;
        }

        public Entity GetSprite(Entity atlas, string name)
        {
            if (!EntityManager.Exists(atlas) || !EntityManager.HasComponent<SpriteAtlas>(atlas))
            {
                return Entity.Null;
            }

            var env = World.TinyEnvironment();
            var buffer = EntityManager.GetBuffer<SpriteAtlas>(atlas).Reinterpret<Entity>();
            for (var i = 0; i < buffer.Length; ++i)
            {
                var entity = buffer[i];
                if (env.GetEntityName(entity) == name)
                {
                    return entity;
                }
            }

            return Entity.Null;
        }

        public Entity GetSprite(Entity atlas, int index)
        {
            if (!EntityManager.Exists(atlas) || !EntityManager.HasComponent<SpriteAtlas>(atlas))
            {
                return Entity.Null;
            }

            var buffer = EntityManager.GetBuffer<SpriteAtlas>(atlas).Reinterpret<Entity>();
            if (index < 0 || index >= buffer.Length)
            {
                throw new IndexOutOfRangeException(nameof(index));
            }
            return buffer[index];
        }

        public NativeArray<Entity> GetSprites(Entity atlas, Allocator allocator = Allocator.Temp)
        {
            if (!EntityManager.Exists(atlas) || !EntityManager.HasComponent<SpriteAtlas>(atlas))
            {
                return new NativeArray<Entity>();
            }

            var buffer = EntityManager.GetBuffer<SpriteAtlas>(atlas).Reinterpret<Entity>();
            return new NativeArray<Entity>(buffer.AsNativeArray(), allocator);
        }

        protected override void OnUpdate()
        {
        }
    }
}
