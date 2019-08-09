using System.Runtime.CompilerServices;
using Unity.Entities;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Unity.Editor
{
    internal class StructItemDataElement<T> : StructDataElement<DynamicBuffer<T>, T>
        where T : struct, IBufferElementData
    {
        private VisualElement m_ItemContent;

        public override VisualElement contentContainer => m_ItemContent;

        public StructItemDataElement(IInspector<T> inspector, IComponentDataElement<DynamicBuffer<T>> root, int index, int offset, string name)
            : base(inspector, root, index, offset, Unsafe.SizeOf<T>(), name)
        {
        }

        public override T Data
        {
            get => Root.Data[Index];
            set
            {
                var buffer =Root.Data;
                buffer[Index] = value;
            }
        }

        public override void BuildFromVisitor<TProperty, TContainer, TValue>(IPropertyVisitor visitor, TProperty property,
            ref TContainer container, ref TValue value, InspectorContext context)
        {
            var template = StyleSheets.ListItem;
            template.Template.CloneTree(this);
            this.AddStyleSheetSkinVariant(template.StyleSheet);
            AddToClassList("unity-ecs-inspector--list-item");
            m_ItemContent = this.Q<VisualElement>("Content");
            base.BuildFromVisitor(visitor, property, ref container, ref value, context);
            var button = this.Q<Button>("RemoveButton");
            button.RegisterCallback<MouseUpEvent>(Removed);
        }

        private void Removed(MouseUpEvent evt)
        {
            (Root as BufferDataElement<T>)?.RemoveDataAtIndex(Index);
        }
    }
}
