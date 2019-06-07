using NUnit.Framework;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;

namespace Unity.Authoring.Tests
{
    internal class UndoManagerTests
    {
        private struct Unit
        {
            public EntityGuid ImageGuid;
            public float2 ImageSize;
            public bool ImageHasAlpha;
            public bool ImageDisableSmoothing;
            public string ImageFileName;
            public EntityGuid SpriteGuid;
            public Rect SpriteImageRegion;
            public float2 SpritePivot;
            public Color SpriteColor;
        }

        private Unit m_Knight = new Unit()
        {
            ImageGuid = new EntityGuid { a = 1, b = 1 },
            ImageSize = new float2(32f, 64f),
            ImageHasAlpha = true,
            ImageDisableSmoothing = false,
            ImageFileName = "Knight.png",
            SpriteGuid = new EntityGuid { a = 2, b = 2 },
            SpriteImageRegion = new Rect(0f, 0f, 1f, 1f),
            SpritePivot = new float2(0.5f, 1f),
            SpriteColor = Color.Default
        };

        private Unit m_Enemy = new Unit()
        {
            ImageGuid = new EntityGuid { a = 3, b = 3 },
            ImageSize = new float2(512f, 512f),
            ImageHasAlpha = true,
            ImageDisableSmoothing = false,
            ImageFileName = "Flying Monster.jpg",
            SpriteGuid = new EntityGuid { a = 4, b = 4 },
            SpriteImageRegion = new Rect(0f, 0f, 1f, 1f),
            SpritePivot = new float2(0.5f, 0.5f),
            SpriteColor = new Color(1f, 0f, 0f)
        };

        private Entity CreateSprite(EntityManager manager, EntityGuid guid, Entity imageEntity, Rect imageRegion, float2 pivot, Color color)
        {
            var entity = manager.CreateEntity(typeof(EntityGuid), typeof(Sprite2D), typeof(Sprite2DRenderer));
            manager.SetComponentData<EntityGuid>(entity, guid);
            manager.SetComponentData(entity, new Sprite2D()
            {
                image = imageEntity,
                imageRegion = imageRegion,
                pivot = pivot,
                pixelsToWorldUnits = 1f
            });
            manager.SetComponentData(entity, new Sprite2DRenderer()
            {
                sprite = entity,
                color = color
            });
            return entity;
        }

        private Entity CreateImage(EntityManager manager, EntityGuid guid, bool disableSmoothing, float2 size, bool alpha, string fileName)
        {
            var entity = manager.CreateEntity(typeof(EntityGuid), typeof(Image2D), typeof(Image2DLoadFromFile), typeof(Image2DLoadFromFileImageFile));
            manager.SetComponentData<EntityGuid>(entity, guid);
            manager.SetComponentData(entity, new Image2D()
            {
                disableSmoothing = disableSmoothing,
                imagePixelSize = size,
                hasAlpha = alpha
            });
            manager.SetBufferFromString<Image2DLoadFromFileImageFile>(entity, fileName);
            return entity;
        }

        private void CreateUnit(EntityManager manager, Unit unit)
        {
            var imageEntity = CreateImage(manager, unit.ImageGuid, unit.ImageDisableSmoothing, unit.ImageSize, unit.ImageHasAlpha, unit.ImageFileName);
            var spriteEntity = CreateSprite(manager, unit.SpriteGuid, imageEntity, unit.SpriteImageRegion, unit.SpritePivot, unit.SpriteColor);
        }

        private void DestroyUnit(EntityManager manager, Unit unit)
        {
            var env = manager.World.GetExistingSystem<TinyEnvironment>();
            manager.DestroyEntity(env.GetEntityByGuid(unit.SpriteGuid));
            manager.DestroyEntity(env.GetEntityByGuid(unit.ImageGuid));
        }

        private void ValidateUnit(EntityManager manager, Unit unit, bool expectedToExist)
        {
            var env = manager.World.GetExistingSystem<TinyEnvironment>();
            var imageEntity = env.GetEntityByGuid(unit.ImageGuid);
            var spriteEntity = env.GetEntityByGuid(unit.SpriteGuid);
            if (expectedToExist)
            {
                Assert.AreNotEqual(Entity.Null, imageEntity);
                Assert.IsTrue(manager.Exists(imageEntity));
                Assert.IsTrue(manager.HasComponent<Image2D>(imageEntity));
                var image2D = manager.GetComponentData<Image2D>(imageEntity);
                Assert.AreEqual(unit.ImageSize, image2D.imagePixelSize);
                Assert.AreEqual(unit.ImageHasAlpha, image2D.hasAlpha);
                Assert.AreEqual(unit.ImageDisableSmoothing, image2D.disableSmoothing);
                Assert.IsTrue(manager.HasComponent<Image2DLoadFromFile>(imageEntity));
                Assert.IsTrue(manager.HasComponent<Image2DLoadFromFileImageFile>(imageEntity));
                Assert.AreEqual(unit.ImageFileName, manager.GetBufferAsString<Image2DLoadFromFileImageFile>(imageEntity));

                Assert.AreNotEqual(Entity.Null, spriteEntity);
                Assert.IsTrue(manager.Exists(spriteEntity));
                Assert.IsTrue(manager.HasComponent<Sprite2D>(spriteEntity));
                var sprite2D = manager.GetComponentData<Sprite2D>(spriteEntity);
                Assert.IsTrue(manager.Exists(sprite2D.image));
                Assert.AreEqual(unit.ImageGuid, manager.GetComponentData<EntityGuid>(sprite2D.image));
                Assert.AreEqual(unit.SpriteImageRegion, sprite2D.imageRegion);
                Assert.AreEqual(unit.SpritePivot, sprite2D.pivot);
                Assert.IsTrue(manager.HasComponent<Sprite2DRenderer>(spriteEntity));
                var sprite2DRenderer = manager.GetComponentData<Sprite2DRenderer>(spriteEntity);
                Assert.IsTrue(manager.Exists(sprite2DRenderer.sprite));
                Assert.AreEqual(unit.SpriteGuid, manager.GetComponentData<EntityGuid>(sprite2DRenderer.sprite));
                Assert.AreEqual(unit.SpriteColor, sprite2DRenderer.color);
            }
            else
            {
                Assert.AreEqual(Entity.Null, imageEntity);
                Assert.IsFalse(manager.Exists(imageEntity));

                Assert.AreEqual(Entity.Null, spriteEntity);
                Assert.IsFalse(manager.Exists(spriteEntity));
            }
        }

        [Test]
        [Ignore("World diff is broken")]
        public void UndoRedo()
        {
            /*
            using (var session = new Session())
            {
                var worldManager = session.GetManager<IWorldManager>();
                var undoManager = session.GetManager<IUndoManager>();
                var entityManager = worldManager.EntityManager;

                worldManager.World.AddSystem(worldManager.World.GetOrCreateSystem<TinyEnvironment>());

                // No operations recorded yet
                Assert.AreEqual(0, undoManager.UndoCount);
                Assert.AreEqual(0, undoManager.RedoCount);
                ValidateUnit(entityManager, m_Knight, expectedToExist: false);
                ValidateUnit(entityManager, m_Enemy, expectedToExist: false);

                // Recording after no changes does not record operations
                undoManager.Record();
                Assert.AreEqual(0, undoManager.UndoCount);
                Assert.AreEqual(0, undoManager.RedoCount);

                // User create content
                CreateUnit(entityManager, m_Knight);
                ValidateUnit(entityManager, m_Knight, expectedToExist: true);
                ValidateUnit(entityManager, m_Enemy, expectedToExist: false);

                // Recording changes adds one undo operation
                undoManager.Record();
                Assert.AreEqual(1, undoManager.UndoCount);
                Assert.AreEqual(0, undoManager.RedoCount);

                // User undo changes adds one redo operation
                undoManager.Undo();
                Assert.AreEqual(0, undoManager.UndoCount);
                Assert.AreEqual(1, undoManager.RedoCount);
                ValidateUnit(entityManager, m_Knight, expectedToExist: false);
                ValidateUnit(entityManager, m_Enemy, expectedToExist: false);

                // Recording after no changes does not record operations
                undoManager.Record();
                Assert.AreEqual(0, undoManager.UndoCount);
                Assert.AreEqual(1, undoManager.RedoCount);

                // User redo changes adds one undo operation
                undoManager.Redo();
                Assert.AreEqual(1, undoManager.UndoCount);
                Assert.AreEqual(0, undoManager.RedoCount);
                ValidateUnit(entityManager, m_Knight, expectedToExist: true);
                ValidateUnit(entityManager, m_Enemy, expectedToExist: false);

                // Recording after no changes does not record operations
                undoManager.Record();
                Assert.AreEqual(1, undoManager.UndoCount);
                Assert.AreEqual(0, undoManager.RedoCount);

                // User destroy content
                DestroyUnit(entityManager, m_Knight);
                ValidateUnit(entityManager, m_Knight, expectedToExist: false);
                ValidateUnit(entityManager, m_Enemy, expectedToExist: false);

                // Recording changes adds one undo operation
                undoManager.Record();
                Assert.AreEqual(2, undoManager.UndoCount);
                Assert.AreEqual(0, undoManager.RedoCount);

                // User create content
                CreateUnit(entityManager, m_Knight);
                ValidateUnit(entityManager, m_Knight, expectedToExist: true);
                ValidateUnit(entityManager, m_Enemy, expectedToExist: false);

                // Recoding changes adds one undo operation
                undoManager.Record();
                Assert.AreEqual(3, undoManager.UndoCount);
                Assert.AreEqual(0, undoManager.RedoCount);

                // User create content
                CreateUnit(entityManager, m_Enemy);
                ValidateUnit(entityManager, m_Knight, expectedToExist: true);
                ValidateUnit(entityManager, m_Enemy, expectedToExist: true);

                // Recoding changes adds one undo operation
                undoManager.Record();
                Assert.AreEqual(4, undoManager.UndoCount);
                Assert.AreEqual(0, undoManager.RedoCount);

                // User undo changes adds one redo operation
                undoManager.Undo();
                Assert.AreEqual(4, undoManager.UndoCount);
                Assert.AreEqual(1, undoManager.RedoCount);
                ValidateUnit(entityManager, m_Knight, expectedToExist: true);
                ValidateUnit(entityManager, m_Enemy, expectedToExist: false);

                // Recording after no changes does not record operations
                undoManager.Record();
                Assert.AreEqual(4, undoManager.UndoCount);
                Assert.AreEqual(1, undoManager.RedoCount);

                // User undo changes adds one redo operation
                undoManager.Undo();
                Assert.AreEqual(3, undoManager.UndoCount);
                Assert.AreEqual(2, undoManager.RedoCount);
                ValidateUnit(entityManager, m_Knight, expectedToExist: false);
                ValidateUnit(entityManager, m_Enemy, expectedToExist: false);

                // Recording after no changes does not record operations
                undoManager.Record();
                Assert.AreEqual(3, undoManager.UndoCount);
                Assert.AreEqual(2, undoManager.RedoCount);
            }
            */
        }
    }
}
