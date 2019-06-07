using System;

namespace Unity.Tiny.Core2D.Editor
{
    public static class UnityCore2DConversionExtensions
    {
        public static UnityEngine.Rect Convert(this Rect rect)
        {
            return new UnityEngine.Rect(rect.x, rect.y, rect.width, rect.height);
        }

        public static Rect Convert(this UnityEngine.Rect rect)
        {
            return new Rect(rect.x, rect.y, rect.width, rect.height);
        }

        public static UnityEngine.CameraClearFlags Convert(this CameraClearFlags flags)
        {
            switch (flags)
            {
                case CameraClearFlags.Nothing:
                    return UnityEngine.CameraClearFlags.Depth;
                case CameraClearFlags.SolidColor:
                    return UnityEngine.CameraClearFlags.SolidColor;
                default:
                    throw new ArgumentOutOfRangeException(nameof(flags), flags, null);
            }
        }

        public static CameraClearFlags Convert(this UnityEngine.CameraClearFlags flags)
        {
            switch (flags)
            {
                case UnityEngine.CameraClearFlags.Skybox:
                case UnityEngine.CameraClearFlags.Color:
                    return CameraClearFlags.SolidColor;
                case UnityEngine.CameraClearFlags.Depth:
                case UnityEngine.CameraClearFlags.Nothing:
                    return CameraClearFlags.Nothing;
                default:
                    throw new ArgumentOutOfRangeException(nameof(flags), flags, null);
            }
        }
    }
}
