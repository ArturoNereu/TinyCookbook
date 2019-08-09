using Unity.Authoring.Core;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny;
using Unity.Tiny.Core2D;
using Unity.Collections;
using Unity.Tiny.UILayout;

namespace Unity.Tiny.Text
{

    [HideInInspector]
    public struct GlyphPrivate
    {
        public CharacterInfo ci;
        public float2 position;
    }

    [HideInInspector]
    public struct GlyphPrivateBuffer : IBufferElementData
    {
        public GlyphPrivate c;
    }

    /// <summary>
    ///  Use CharacterInfo to create glyph metrics for a bitmap font.
    /// </summary>
    public struct CharacterInfo
    {
        /// <summary>
        ///  UTF-32 character value of the glyph.
        /// </summary>
        public uint value;

        /// <summary>
        ///  The horizontal distance, in pixels, from this character's origin to
        ///  the next character's origin.
        /// </summary>
        public float advance;

        /// <summary>
        ///  The horizontal distance in pixels from this glyph's origin to the beginning
        ///  of the glyph image.
        /// </summary>
        public float bearingX;

        /// <summary>
        ///  The vertical distance in pixels from the baseline to the glyph's ymax
        ///  (top of the glyph bounding box).
        /// </summary>
        public float bearingY;

        /// <summary>
        ///  The width of the glyph image.
        /// </summary>
        public float width;

        /// <summary>
        ///  The height of the glyph image.
        /// </summary>
        public float height;

        /// <summary>
        ///  The glyph's uv coordinates in the texture atlas. x, y is bottom left.
        /// </summary>
        public Rect characterRegion;

    }

    /// <summary>
    /// A CharacterInfo for each glyph in the font.
    /// </summary>
    [HideInInspector]
    public struct CharacterInfoBuffer : IBufferElementData
    {
        public CharacterInfo data;
    }

    /// <summary>
    /// Provides metadata about the font.
    /// </summary>
    /// <remarks>
    /// Generally you will want to use <see cref="Unity.Tiny.Text.TextService.CreateBitmapFont"/> to create BitmapFonts.
    /// A BitmapFont is fully specified by an entity that has the following components:
    /// <list>
    ///    <item><see cref="BitmapFont"/></item>
    ///    <item><see cref="CharacterInfoBuffer"/></item>
    /// </list>
    /// </remarks>
    [HideInInspector]
    public struct BitmapFont : IComponentData
    {
        /// <summary>
        ///  The entity on which to look for an <see cref="Image2D"/> component to use.
        /// </summary>
        [EntityWithComponents(typeof(Image2D))]
        public Entity textureAtlas;

        /// <summary>
        ///  The Font size in World Units.
        /// </summary>
        public float size;

        /// <summary>
        ///  The distance from the baseline to the font's ascent line.
        /// </summary>
        public float ascent;

        /// <summary>
        /// Distance from the baseline to the font's descent line.
        /// </summary>
        public float descent;

    }

    /// <summary>
    ///   Lists available generic font families.
    /// </summary>
    public enum FontName
    {
        SansSerif,
        Serif,
        Monospace
    }

    /// <summary>
    /// Initialize a Native font from file (.ttf)
    /// <remarks>
    /// You need to specify the file name with <see cref="NativeFontLoadFromFileName"> next to this component
    /// </remarks>
    /// </summary>
    [HideInInspector]
    public struct NativeFontLoadFromFile : IComponentData
    {
        private int dummy;
    }

    /// <summary>
    /// The path of the font (.ttf) to load at runtime corresponding to the NativeFont.FontName.
    /// The ttf has to contain only one font.
    /// </summary>
    /// <remarks>
    /// This component should be attached to the font entity. Not the text entity
    /// </remarks>
    [HideInInspector]
    public struct NativeFontLoadFromFileName : IBufferElementData
    {
        public char c;
    }

    public enum FontStatus
    {
        Invalid,
        Loaded,
        Loading,
        LoadError
    }

    //This needs to be internal (only used during font loading). Currently it can't or it is less accessible than the GenericAssetLoader methods (public)
    [HideInInspector]
    public struct NativeFontPrivate : ISystemStateComponentData
    {
        public int fontHandle;
    }

    //This needs to be internal (only used during font loading). Currently it can't or it is less accessible than the GenericAssetLoader methods (public)
    [HideInInspector]
    public struct NativeFontLoading : ISystemStateComponentData
    {
        public long internalId;
    }

    /// <summary>
    ///  Add this component to an entity to specify a native font.
    /// </summary>
    /// <remarks>
    /// Generally you will want to use <see cref="Unity.Tiny.Text.TextService.CreateNativeFont"/> to create a NativeFont.
    /// </remarks>
    public struct NativeFont : IComponentData
    {
        public static NativeFont Default { get; } = new NativeFont()
        {
            name = FontName.SansSerif,
            worldUnitsToPt = 1.0f
        };
        /// <summary>
        ///  Name of the font.
        /// </summary>
        public FontName name;

        /// <summary>
        ///  Multiplier for converting World units to Points. Fonts are rendered
        ///  in points (pt).
        /// </summary>
        [HideInInspector]
        public float worldUnitsToPt;

        /// <summary>
        /// Font loading status
        /// </summary>
        [HideInInspector]
        public FontStatus status;
    }

    /// <summary>
    ///  Add this component to an entity with a Text2DRenderer component to specify
    ///  a native font.
    /// </summary>
    public struct Text2DStyleNativeFont : IComponentData
    {
        public static Text2DStyleNativeFont Default { get; } = new Text2DStyleNativeFont()
        {
            font = Entity.Null,
            italic = false,
            weight = 400 
        };
        /// <summary>
        ///  The Font entity on which to look for a {@link NativeFont} component to use.
        /// </summary>
        [EntityWithComponents(typeof(NativeFont))]
        public Entity font;

        /// <summary>
        ///  If true, adds the italic attribute to the text.
        /// </summary>
        public bool italic;

        /// <summary>
        ///  Sets the font weight. A value of 400 is normal weight. A value of 700 is bold. Higher values
        ///  make characters thicker. Lower values make them thinner.
        /// </summary>
        public int weight;

    }

    /// <summary>
    ///  Add this component to an entity with a Text2DRenderer component to specify
    ///  a bitmap font.
    /// </summary>
    public struct Text2DStyleBitmapFont : IComponentData
    {
        /// <summary>
        ///  The Font entity on which to look for a {@link BitmapFont} component to use.
        /// </summary>
        [EntityWithComponents(typeof(BitmapFont))]
        public Entity font;

    }

    /// <summary>
    ///  Add this component to an entity with a Text2DRenderer component to specify
    ///  the font style.
    /// </summary>
    public struct Text2DStyle : IComponentData
    {
        public static Text2DStyle Default { get; } = new Text2DStyle()
        {
            color = new Color(0, 0, 0),
            size = 16
        };

        /// <summary>
        /// The text {@link Color}
        /// </summary>
        public Color color;

        /// <summary>
        /// The Font size in World Units;
        /// </summary>
        public float size;

    }

    /// <summary>
    /// Defines text to display. Add this to an entity with a fully defined font
    /// to display text.
    /// </summary>
    public struct TextString : IBufferElementData
    {
        public char c;
    }

    /// <summary>
    /// Enables rendering of 2D text.
    /// </summary>
    /// <remarks>
    /// <para>
    ///  The Text module allows you to define Text and Font components on entities,
    ///  and render them in Canvas or WebGL.
    /// </para><para>
    ///  Two types of font are available: Native fonts (from 3 generic fonts widely
    ///  supported by most browsers) and Bitmap fonts.
    ///  Add this component to a text entity to specify the style, pivot, and blending.
    /// </para><para>
    ///  If you want to set the text inside of a RectTransform component, you must
    ///  add a <see cref="RectTransformFinalSize "/>
    ///  component to the entity with the <see cref="Text2DRenderer"/>.
    /// </para><para>
    ///  If you want to auto-fit the text inside the RectTransform component, you
    ///  must also add a <see cref="Text2DAutoFit"/> component.
    /// </para>
    /// </remarks>
    public struct Text2DRenderer : IComponentData
    {
        public static Text2DRenderer Default { get; } = new Text2DRenderer()
        {
            pivot = new float2(0.5f, 0.5f)
        };

        /// <summary>
        ///  The entity on which to look for a <see cref="Text2DStyle"/> component to
        ///  use as the Text style.
        /// </summary>
        [EntityWithComponents(typeof(Text2DStyle))]
        public Entity style;

        /// <summary>
        ///  The center point in the text, relative to the text's bottom-left corner,
        ///  in unit rectangle coordinates
        /// </summary>
        public float2 pivot;

        /// <summary>
        ///  {@link BlendOp} for rendering text. The default and regular mode is Alpha.
        /// </summary>
        public BlendOp blending;

    }

    /// <summary>
    ///  When added to an entity with a Text2DRenderer component, auto-fits the text.
    ///  For Text2DAutofit to work, you must also add a RectTransformFinalSize
    ///  component to the entity.
    /// </summary>
    public struct Text2DAutoFit : IComponentData
    {
        /// <summary>
        ///  The minimum font size. If the font size computed at runtime to fit
        ///  in the RectTransform is below this value, the text does not render.
        /// </summary>
        public float minSize;

        /// <summary>
        ///  The maximum font size.
        /// </summary>
        public float maxSize;
    }

    internal class MakeEntryText : IExternalDisplayListEntryMaker
    {
        public int GetRendererComponent()
        {
            return TypeManager.GetTypeIndex<Text2DRenderer>();
        }

        public bool DoNotClip()
        {
            return false;
        }

        public void Filter(ref EntityQueryBuilder query)
        {
            query.WithAll<Text2DRenderer>();
        }

        public bool MakeEntry(EntityManager em, ref DisplayListEntry de)
        {
            var textRenderer = em.GetComponentData<Text2DRenderer>(de.e);
            if (!em.Exists(textRenderer.style))
                return false;

            if (!em.HasComponent<Text2DStyle>(textRenderer.style))
                return false;

            if (em.HasComponent<Text2DStyleNativeFont>(de.e))
            {
                if (!em.HasComponent<Text2DPrivateNative>(de.e))
                    return false;
                var textPrivate = em.GetComponentData<Text2DPrivateNative>(textRenderer.style);
                // System State component added once the texture atlas is loaded
                de.inBounds = textPrivate.bounds;
                de.type = DisplayListEntryType.Text;
                return true;
            }

            if (em.HasComponent<Text2DStyleBitmapFont>(de.e))
            {
                if (!em.HasComponent<Text2DPrivateBitmap>(de.e))
                    return false;

                var textPrivate = em.GetComponentData<Text2DPrivateBitmap>(textRenderer.style);
                // System State component added once the texture atlas is loaded
                de.inBounds = textPrivate.bounds;
                de.type = DisplayListEntryType.Text;
                return true;
            }
            return false;
        }

        public void Update(ComponentSystem cs) { }
    }

    //  An initialization-only system required to initialize text rendering.
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(Unity.Tiny.UILayout.SetRectTransformSizeSystem))]
    internal class Text2DInitSystem : ComponentSystem
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            DisplayListSystem.RegisterDisplayListEntryMaker(new MakeEntryText());
        }

        protected override void OnDestroy()
        {
            DisplayListSystem.DeRegisterExternalDisplayListEntryMaker(TypeManager.GetTypeIndex<Text2DRenderer>());
        }

        protected override void OnUpdate()
        {
            //Add the RectTransformFinalSize component required for text using RectTransform. Bitmap or native fonts
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
            Entities.WithAll<Text2DRenderer>().WithAll<RectTransform>().WithNone<RectTransformFinalSize>().ForEach(e => {
                ecb.AddComponent(e, new RectTransformFinalSize());
            });

            ecb.Playback(EntityManager);
            ecb.Dispose();
            ecb = new EntityCommandBuffer(Allocator.Temp);

            //Remove the RectTransformFinalSize component if not needed
            Entities.WithAll<RectTransformFinalSize>().WithNone<Text2DRenderer>().ForEach(e => {
                ecb.RemoveComponent<RectTransformFinalSize>(e);
            });
            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }

    [HideInInspector]
    public struct TextPrivateString : IBufferElementData
    {
        public char c;
    }

    [HideInInspector]
    public struct TextPrivateFontName : IBufferElementData
    {
        public char c;
    }


    [HideInInspector]
    public struct Text2DPrivateNative : ISystemStateComponentData
    {
        public Rect bounds;
        public float size;
    }

    [HideInInspector]
    public struct Text2DPrivateBitmap : ISystemStateComponentData
    {
        public Rect bounds;
        public float2 fontScale;
        public float size;
    }
}
