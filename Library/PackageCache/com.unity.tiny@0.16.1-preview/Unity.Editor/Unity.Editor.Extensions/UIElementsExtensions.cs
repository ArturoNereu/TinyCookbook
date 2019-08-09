using UnityEngine.UIElements;

namespace Unity.Editor.Extensions
{
    internal static class UIElementsExtensions
    {
        public static VisualElement GetInput(this VisualElement searchField)
        {
            return searchField.Query<VisualElement>("unity-text-input");
        }
    }
}
