using JetBrains.Annotations;
using Unity.Editor.Bindings;
using Unity.Entities;

namespace Unity.Tiny.Core2D.Editor
{
    [UsedImplicitly]
    internal class ParentBindings : IEntityBinding,
        IComponentBinding<Parent>
    {
        public void LoadBinding(Entity entity, IBindingContext context)
        {
            // Nothing to do.
        }

        public void UnloadBinding(Entity entity, IBindingContext context)
        {
            // Nothing to do.
        }

        public void TransferToUnityComponents(Entity entity, IBindingContext context)
        {
            // Nothing to do.
        }

        public void TransferFromUnityComponents(Entity entity, IBindingContext context)
        {
            // Nothing to do.
        }
    }
}
