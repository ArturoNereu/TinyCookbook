namespace Unity.Tiny.Core2DTypes.Editor
{
    public static class UnityCore2DTypesConversionExtensions
    {
        public static UnityEngine.Color Convert(this Core2D.Color color)
        {
            return new UnityEngine.Color(color.r, color.g, color.b, color.a);
        }

        public static Core2D.Color Convert(this UnityEngine.Color color)
        {
            return new Core2D.Color(color.r, color.g, color.b, color.a);
        }
    }
}
