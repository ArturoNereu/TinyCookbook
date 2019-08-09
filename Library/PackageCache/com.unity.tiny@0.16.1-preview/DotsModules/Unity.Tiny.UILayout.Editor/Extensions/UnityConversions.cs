using System;

namespace Unity.Tiny.UILayout.Editor
{
    public static class UnityUILayoutConversionExtensions
    {
        public static UnityEngine.UI.CanvasScaler.ScaleMode Convert(this UIScaleMode mode)
        {
            switch (mode)
            {
                case UIScaleMode.ConstantPixelSize:
                    return UnityEngine.UI.CanvasScaler.ScaleMode.ConstantPixelSize;
                case UIScaleMode.ScaleWithScreenSize:
                    return UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        public static UIScaleMode Convert(this UnityEngine.UI.CanvasScaler.ScaleMode mode)
        {
            switch (mode)
            {
                case UnityEngine.UI.CanvasScaler.ScaleMode.ConstantPixelSize:
                case UnityEngine.UI.CanvasScaler.ScaleMode.ConstantPhysicalSize:
                    return UIScaleMode.ConstantPixelSize;
                case UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize:
                    return UIScaleMode.ScaleWithScreenSize;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }
    }
}
