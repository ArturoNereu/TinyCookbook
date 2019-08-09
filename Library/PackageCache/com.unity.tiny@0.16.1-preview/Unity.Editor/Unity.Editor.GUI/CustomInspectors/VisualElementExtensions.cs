using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.Editor
{
    public static class VisualElementExtensions
    {
        private const string ProSuffix = "_pro";
        private const string PersonalSuffix = "_personal";
        private static string SkinSuffix => EditorGUIUtility.isProSkin ? ProSuffix : PersonalSuffix;

        /// <summary>
        /// Adds the specified style sheet and attempts to load the "_pro" and "_personal" variants as well.
        /// </summary>
        /// <param name="element">The <see cref="VisualElement"/> to add the StyleSheet to.</param>
        /// <param name="mainStyleSheet">The main <see cref="StyleSheet"/>.</param>
        public static void AddStyleSheetSkinVariant(this VisualElement element, StyleSheet mainStyleSheet)
        {
            if (null == mainStyleSheet)
            {
                return;
            }

            element.styleSheets.Add(mainStyleSheet);
            var assetPath = AssetDatabase.GetAssetPath(mainStyleSheet);
            assetPath = assetPath.Insert(assetPath.LastIndexOf('.'), SkinSuffix);
            if (string.IsNullOrEmpty(assetPath))
            {
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<StyleSheet>(assetPath) is var skin && null != skin)
            {
                element.styleSheets.Add(skin);
            }
        }
    }
}
