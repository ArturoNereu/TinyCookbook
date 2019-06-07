using UnityEngine.UIElements;

namespace Unity.Editor
{
    public interface IInspectorTemplateProvider
    {
        VisualTreeAsset UxmlAsset { get; }
        StyleSheet UssAsset { get; }
        bool AutoRegisterBindings { get; }
    }

    public static class IInspectorTemplateProviderExtensions
    {
        public static VisualElement BuildFromTemplate(this IInspectorTemplateProvider provider)
        {
            if (null == provider)
            {
                return null;
            }

            var template = provider.UxmlAsset;
            var container = template?.CloneTree() ?? new VisualElement();

            container.AddStyleSheetSkinVariant(provider.UssAsset);

            return container;
        }
    }
}
