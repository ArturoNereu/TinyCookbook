using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny;
using Unity.Tiny.Core;

[assembly: ModuleDescription("Unity.Tiny.Sprite2D", "2D sprites and surrounding components")]
namespace Unity.Tiny.Core2D
{
    /// <summary>
    /// A component describing a sprite as a sub-region of an image.  Specifies the source <see cref="Image2D"/>
    /// atlas and the region to use.
    /// </summary>
    public struct Sprite2D : IComponentData
    {
        /// <summary>
        ///  The Entity on which to look for a <see cref="Image2D"/> component to use as the source image.
        ///  If null, the <see cref="Image2D"/> is looked for on the same entity as the <see cref="Sprite2D"/>.
        /// </summary>
        [EntityWithComponents(typeof(Image2D))]
        public Entity image;

        /// <summary>
        ///  The region of the source image to use as the sprite.
        /// </summary>
        /// <remarks>
        ///  The image is treated as a unit
        ///  rectangle; thus this rectangle should use values in the range of 0..1.  For example,
        ///  to use the bottom left portion of the image, the rectangle should go from (0, 0) to
        ///  (0.5, 0.5)
        /// </remarks>
        public Rect imageRegion;

        /// <summary>
        ///  The point in the sprite that is the sprite's center.  Relative to the bottom-left corner
        ///  of the sprite, in unit rectangle coordinates.
        /// </summary>
        public float2 pivot;

        /// <summary>
        /// Conversion ratio of sprite image pixels to world units, e.g.:
        /// The default value is 1.
        /// </summary>
        /// <remarks>
        /// This should not be used for scaling sprites.
        /// <list>
        ///     <item> 1 : 100 pixels = 100 world units </item>
        ///     <item> 1/4: 100 pixels = 25 world units </item>
        /// </list>
        /// </remarks>
        public float pixelsToWorldUnits;
    }

    internal struct Sprite2DPrivate : ISystemStateComponentData {
        public bool valid;
        public Rect rect;
    };

    internal class MakeEntrySprite : IExternalDisplayListEntryMaker
    {
        public int GetRendererComponent()
        {
            return TypeManager.GetTypeIndex<Sprite2DRenderer>();
        }

        // Do not clip this type of entry, used by helper entries like sorting group heads
        public bool DoNotClip()
        {
            return false;
        }

        // Extra filter applied during iteration, optional
        public void Filter(ref EntityQueryBuilder query)
        {
            query.WithAll<Sprite2DRenderer>();
        }

        // Callback to create entry
        // DisplayListEntry de is input/output
        //    e = input, the entity being added
        //    finalMatrix = undefined at this point, do not change
        //    inBounds = output, object space bounding rectangle
        //    type = output, type to be used by rendering
        //    inSortingGroup = undefined at this point, do not change
        // return false to discard the entry
        public bool MakeEntry(EntityManager m, ref DisplayListEntry de)
        {
            var spriteRenderer = cachedGetSprite2DRenderer[de.e];
            if (spriteRenderer.color.a <= 0.0f)
                return false;
            var spritePrivate = cachedGetSprite2DPrivate[spriteRenderer.sprite];
            if ( !spritePrivate.valid )
                return false;

            if (cachedGetSprite2DRendererOptions.Exists(de.e)) {
                // slow path: with options
                var sprite = cachedGetSprite2D[spriteRenderer.sprite];
                var tiling = cachedGetSprite2DRendererOptions[de.e];
                float2 size = tiling.size;
                Entity esprite = spriteRenderer.sprite;
                bool hasBorder = false;
                // check for border
                if (cachedGetSprite2DBorder.Exists(esprite)) {
                    var border = cachedGetSprite2DBorder[esprite];
                    hasBorder = border.bottomLeft.x > 0.0f || border.bottomLeft.y > 0.0f || border.topRight.x < 1.0f ||
                                border.topRight.y < 1.0f;
                    //Assert(border->bottomLeft.x < border->topRight.x);
                    //Assert(border->bottomLeft.y < border->topRight.y);
                    //Assert(border->bottomLeft.x >= 0.0f && border->topRight.x <= 1.0f);
                    //Assert(border->bottomLeft.y >= 0.0f && border->topRight.y <= 1.0f);
                }
                if (tiling.drawMode == DrawMode.Stretch && !hasBorder)
                    de.type = DisplayListEntryType.Sprite;
                else
                    de.type = hasBorder ? DisplayListEntryType.SlicedSprite : DisplayListEntryType.TiledSprite;
                // compute bounds
                de.inBounds = new Rect{ x=size.x * -sprite.pivot.x, y=size.y * -sprite.pivot.y, width=size.x, height=size.y};
            } else {
                de.inBounds = spritePrivate.rect;
                de.type = DisplayListEntryType.Sprite;
            }
            return true;
        }

        public ComponentDataFromEntity<Sprite2DRenderer> cachedGetSprite2DRenderer;
        public ComponentDataFromEntity<Sprite2DPrivate> cachedGetSprite2DPrivate;
        public ComponentDataFromEntity<Sprite2DRendererOptions> cachedGetSprite2DRendererOptions;
        public ComponentDataFromEntity<Sprite2D> cachedGetSprite2D;
        public ComponentDataFromEntity<Sprite2DBorder> cachedGetSprite2DBorder;

        public void Update(ComponentSystem cs)
        {
            cachedGetSprite2DRenderer = cs.GetComponentDataFromEntity<Sprite2DRenderer>();
            cachedGetSprite2DPrivate = cs.GetComponentDataFromEntity<Sprite2DPrivate>();
            cachedGetSprite2DRendererOptions = cs.GetComponentDataFromEntity<Sprite2DRendererOptions>();
            cachedGetSprite2D = cs.GetComponentDataFromEntity<Sprite2D>();
            cachedGetSprite2DBorder = cs.GetComponentDataFromEntity<Sprite2DBorder>();
        }
    }

    // An initialization only System required to initialize sprite rendering
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateBefore(typeof(DisplayListSystem))]
    internal class Sprite2DSystem : ComponentSystem
    {
        void InitWhiteSprite()
        {
            var env = World.TinyEnvironment();
            var mgr = EntityManager;
            if (env.whiteSpriteEntity == Entity.Null)
                env.whiteSpriteEntity = mgr.CreateEntity();
            var e = env.whiteSpriteEntity;

            if (mgr.HasComponent<Sprite2D>(e))
                return;

            Image2D im = new Image2D();
            mgr.AddComponentData(e, im);
            Image2DAlphaMask mask = new Image2DAlphaMask();
            mask.threshold = .5f;
            mgr.AddComponentData(e, mask);

            Image2DLoadFromFile ftrigger = new Image2DLoadFromFile();
            mgr.AddComponentData(e, ftrigger);

            mgr.AddBufferFromString<Image2DLoadFromFileImageFile>(e, "::white1x1");

            Sprite2D sp = new Sprite2D();
            sp.image = e;
            sp.pivot = new float2(.5f, .5f);
            sp.imageRegion = new Rect { x=0.0f, y=0.0f, width=1.0f, height=1.0f};
            sp.pixelsToWorldUnits = 1f;
            mgr.AddComponentData(e, sp);
        }

        bool VerifyImageForSprite(Entity e, ref Sprite2D sprite, out Image2D image)
        {
            var mgr = EntityManager;
            // verify image
            if (sprite.image == Entity.Null) // fixup invalid ref to point to self
                sprite.image = e;
            if (!mgr.Exists(sprite.image) || !cachedGetImage2D.Exists(sprite.image))
            {
                var env = World.TinyEnvironment();
                sprite.image = env.whiteSpriteEntity;
            }
            image = cachedGetImage2D[sprite.image];
            if (image.status != ImageStatus.Loaded)
                return false;
            // TODO: add back in all the debug only checks

            return true;
        }

        struct WrapImage2D
        {
            public bool valid;
            public Image2D image;
        }

        NativeHashMap<Entity, WrapImage2D> imageCheck;
        private ComponentDataFromEntity<Image2D> cachedGetImage2D;
        private MakeEntrySprite dlm;

        protected override void OnCreate()
        {
            InitWhiteSprite();
            dlm = new MakeEntrySprite();
            DisplayListSystem.RegisterDisplayListEntryMaker(dlm);
            imageCheck = new NativeHashMap<Entity, WrapImage2D>(256, Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            imageCheck.Dispose();
            DisplayListSystem.DeRegisterExternalDisplayListEntryMaker(TypeManager.GetTypeIndex<Sprite2DRenderer>());
        }

        protected override void OnUpdate()
        {
            // do all the things we used to do in MakeEntrySprite::PreUpdate

            var mgr = EntityManager;
            var env = World.TinyEnvironment();

            // Sprite2DRenderer: fix up any invalid ref to point to self or white sprite
            Entities.ForEach((Entity e, ref Sprite2DRenderer spriteRenderer) =>
            {
                if (spriteRenderer.sprite == Entity.Null)
                    spriteRenderer.sprite = e;
                if (!mgr.Exists(spriteRenderer.sprite) || !mgr.HasComponent<Sprite2D>(spriteRenderer.sprite))
                    spriteRenderer.sprite = env.whiteSpriteEntity;
            });

            // Sprite2D: Add Sprite2DPrivate if it does not exist
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
            Entities
                .WithAll<Sprite2D>().WithNone<Sprite2DPrivate>()
                .ForEach(e => ecb.AddComponent<Sprite2DPrivate>(e, default(Sprite2DPrivate)));

            // Sprite2DPrivate: clean up if there is no Sprite2D anymore
            Entities
                .WithNone<Sprite2D>().WithAll<Sprite2DPrivate>()
                .ForEach(e => ecb.RemoveComponent<Sprite2DPrivate>(e));

            ecb.Playback(mgr);
            ecb.Dispose();

            cachedGetImage2D = GetComponentDataFromEntity<Image2D>();
            dlm.Update(this);
            // update Sprite2DPrivate for every Sprite2D: this helps with sprites that are used by a lot of Sprite2DRenderers
            // (which is the case mostly in benchmarks..) as we do not verify image a lot
            Entities.ForEach( ( Entity e, ref Sprite2D sprite, ref Sprite2DPrivate spritePrivate) => {
                WrapImage2D wrapImage;
                if (!imageCheck.TryGetValue(e, out wrapImage))
                {
                    wrapImage.valid = VerifyImageForSprite(e, ref sprite, out wrapImage.image);
                    imageCheck.TryAdd(e, wrapImage);
                }

                if (!wrapImage.valid)
                {
                    spritePrivate.valid = false;
                }
                else
                {
                    float2 size = wrapImage.image.imagePixelSize * sprite.pixelsToWorldUnits;
                    size.x *= sprite.imageRegion.width;
                    size.y *= sprite.imageRegion.height;
                    spritePrivate.rect = new Rect
                    {
                        x = size.x * -sprite.pivot.x, y = size.y * -sprite.pivot.y, width = size.x, height = size.y
                    };
                    spritePrivate.valid = true;
                }
            });
            imageCheck.Clear();

        }
    }
}
