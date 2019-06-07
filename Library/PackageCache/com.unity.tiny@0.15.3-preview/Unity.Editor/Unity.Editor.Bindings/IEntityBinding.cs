using Unity.Entities;

namespace Unity.Editor.Bindings
{
    /// <summary>
    /// Entry point to create a binding between a <see cref="Entity"/> and a <see cref="UnityEngine.GameObject"/>
    /// </summary>
    internal interface IEntityBinding
    {
        /// <summary>
        /// Allows to add <see cref="UnityEngine.Component"/> to the associated <see cref="UnityEngine.GameObject"/> of
        /// the <see cref="entity"/> when the partial binding is loaded.
        /// </summary>
        /// <param name="entity">The entity instance</param>
        /// <param name="context">Context to transfer entity data</param>
        void LoadBinding(Entity entity, IBindingContext context);

        /// <summary>
        /// Allows to remove <see cref="UnityEngine.Component"/> to the associated <see cref="UnityEngine.GameObject"/>
        /// of the <see cref="entity"/> when the partial binding is unloaded.
        /// </summary>
        /// <param name="entity">The entity instance</param>
        /// <param name="context">Context to transfer entity data</param>
        void UnloadBinding(Entity entity, IBindingContext context);

        /// <summary>
        /// Allows to transfer the <see cref="entity"/> data to the associated <see cref="UnityEngine.GameObject"/>
        /// </summary>
        /// <param name="entity">The entity instance</param>
        /// <param name="context">Context to transfer entity data</param>
        void TransferToUnityComponents(Entity entity, IBindingContext context);

        /// <summary>
        /// Allows to transfer the associated <see cref="UnityEngine.GameObject"/> data back to the <see cref="entity"/>
        /// </summary>
        /// <param name="entity">The entity instance</param>
        /// <param name="context">Context to transfer entity data</param>
        void TransferFromUnityComponents(Entity entity, IBindingContext context);
    }
}
