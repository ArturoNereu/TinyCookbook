using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny;
using Unity.Authoring.Core;

/// <summary>
/// The Main Unity.Tiny namespace
/// @module
/// @name Unity.Tiny
/// </summary>
[assembly: ModuleDescription("Unity.Tiny.Core2DTypes", "Core 2D basic shared types")]
namespace Unity.Tiny.Core2D
{
    /// <summary>
    /// RGBA floating-point color.
    /// </summary>
    [IdAlias("2a90a1633d6dc16cd37cdfaca7b93cea")]
    public struct Color : IEquatable<Color>
    {
        public static Color Default { get; } = new Color(1f, 1f, 1f, 1f);

        /// <summary> Red value, range is [0..1] </summary>
        public float r;
        /// <summary> Green value, range is [0..1] </summary>
        public float g;
        /// <summary> Blue value, range is [0..1] </summary>
        public float b;
        /// <summary> Alpha value, range is [0..1] </summary>
        public float a;

        public Color(float red, float green, float blue, float alpha = 1f)
        {
            r = red;
            g = green;
            b = blue;
            a = alpha;
        }

        public static bool operator ==(Color cl, Color cr)
        {
            return cl.r == cr.r && cl.g == cr.g && cl.b == cr.b && cl.a == cr.a;
        }

        public static Color operator +(Color cl, Color cr)
        {
            return new Color(cl.r + cr.r, cl.g + cr.g, cl.b + cr.b, cl.a + cr.a);
        }

        public static Color operator *(Color cl, Color cr)
        {
            return new Color(cl.r * cr.r, cl.g * cr.g, cl.b * cr.b, cl.a * cr.a);
        }

        public static Color operator *(Color cl, float v)
        {
            return new Color(cl.r * v, cl.g * v, cl.b * v, cl.a * v);
        }

        public static bool operator !=(Color cl, Color cr)
        {
            return !(cl == cr);
        }

        public bool Equals(Color c)
        {
            return this == c;
        }

        public override bool Equals(object obj)
        {
            return Equals((Color)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = r.GetHashCode();
                hashCode = (hashCode * 397) ^ g.GetHashCode();
                hashCode = (hashCode * 397) ^ b.GetHashCode();
                hashCode = (hashCode * 397) ^ a.GetHashCode();
                return hashCode;
            }
        }

        public static Color Lerp(Color c1, Color c2, float time)
        {
            return c1 * (1.0f - time) + c2 * time;
        }
    }

    /// <summary>
    /// Blending operation when drawing
    /// </summary>
    public enum BlendOp
    {
        /// <summary> Default. Normal alpha blending. </summary>
        Alpha,

        /// <summary> Additive blending. Only brightens colors. Black is neutral and has no effect.. </summary>
        Add,

        /// <summary> Multiplicative blending. Only darken colors. White is neutral and has no effect. </summary>
        Multiply,

        /// <summary>
        /// Multiplies the target by the source alpha.
        /// Only the source alpha channel is used.
        /// Drawing using this mode is useful when rendering to a textures to mask borders.
        /// </summary>
        MultiplyAlpha,

        /// <summary> Do not perform any blending. </summary>
        Disabled
    }

    /// <summary>
    /// Add this compoment next to a RectTransform component and a Text2DRenderer (for now)
    /// while adding a text in a rect transform
    /// </summary>
    [IdAlias("00d6bc791188d69186aa19bf8f6652a8")]
    [HideInInspector]
    public struct RectTransformFinalSize : ISystemStateComponentData
    {
        /// <summary>
        /// Rect transform size of an entity.
        /// This value is updated by the SetRectTransformSizeSystem system
        /// </summary>
        public float2 size;
    }
}
