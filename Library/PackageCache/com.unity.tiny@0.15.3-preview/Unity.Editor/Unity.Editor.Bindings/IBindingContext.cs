using System;
using Unity.Entities;
using UnityEngine;

namespace Unity.Editor.Bindings
{
    /// <summary>
    /// Allows you to transfer data between an <see cref="Entity"/> and a <see cref="Component"/>.
    /// </summary>
    internal interface IBindingContext
    {
        /// <summary>
        /// Checks whether an Entity has a specific type of Component.
        /// </summary>
        /// <param name="entity">The Entity.</param>
        /// <typeparam name="TComponentData">The data type of the Component.</typeparam>
        /// <returns>True, if the specified entity has the component.</returns>
        bool HasComponent<TComponentData>(Entity entity)
            where TComponentData : struct;

        /// <summary>
        /// Gets the value of a Component for an Entity.
        /// </summary>
        /// <param name="entity">The Entity.</param>
        /// <typeparam name="TComponentData">The type of Component to retrieve.</typeparam>
        /// <returns>A struct of type T containing the Component value.</returns>
        TComponentData GetComponentData<TComponentData>(Entity entity)
            where TComponentData : struct, IComponentData;

        /// <summary>
        /// Adds a Component to an Entity.
        /// </summary>
        /// <param name="entity">The Entity.</param>
        /// <typeparam name="TComponentData">The data type of the Component.</typeparam>
        void AddComponent<TComponentData>(Entity entity)
            where TComponentData : struct, IComponentData;

        /// <summary>
        /// Adds a Component to an Entity and sets the Component's value.
        /// </summary>
        /// <param name="entity">The Entity.</param>
        /// <param name="data">The data to set.</param>
        /// <typeparam name="TComponentData">The type of Component.</typeparam>
        void AddComponentData<TComponentData>(Entity entity, TComponentData data)
            where TComponentData : struct, IComponentData;

        /// <summary>
        /// Sets the value of a Component of an Entity.
        /// </summary>
        /// <param name="entity">The Entity.</param>
        /// <param name="data">The data to set.</param>
        /// <typeparam name="TComponentData">The type of Component.</typeparam>
        void SetComponentData<TComponentData>(Entity entity, TComponentData data)
            where TComponentData : struct, IComponentData;

        /// <summary>
        /// Adds a dynamic buffer Component to an Entity.
        /// </summary>
        /// <param name="entity">The Entity.</param>
        /// <typeparam name="TElementType">The type of the buffer's elements.</typeparam>
        /// <returns>The DynamicBuffer object for accessing the buffer contents.</returns>
        DynamicBuffer<TElementType> AddBuffer<TElementType>(Entity entity)
            where TElementType : struct, IBufferElementData;

        /// <summary>
        /// Gets the dynamic buffer of an Entity.
        /// </summary>
        /// <param name="entity">The Entity.</param>
        /// <typeparam name="TElementType">The type of the buffer's elements.</typeparam>
        /// <returns>The DynamicBuffer object for accessing the buffer contents.</returns>
        DynamicBuffer<TElementType> GetBuffer<TElementType>(Entity entity)
            where TElementType : struct, IBufferElementData;

        /// <summary>
        /// Gets the dynamic buffer of an Entity for ReadOnly.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <typeparam name="TElementType">The type of the buffer's elements.</typeparam>
        /// <returns>The DynamicBuffer object for accessing the buffer contents.</returns>
        DynamicBuffer<TElementType> GetBufferRO<TElementType>(Entity entity)
            where TElementType : struct, IBufferElementData;

        /// <summary>
        /// Removes a Component of an Entity.
        /// </summary>
        /// <param name="entity">The Entity.</param>
        /// <typeparam name="TComponentData">The type of Component.</typeparam>
        void RemoveComponent<TComponentData>(Entity entity)
            where TComponentData : struct;

        /// <summary>
        /// Gets the Unity Component from the GameObject bound to an Entity.
        /// </summary>
        /// <param name="entity">The Entity.</param>
        /// <typeparam name="TComponent">The <exception cref="Component"> type</exception></typeparam>
        /// <returns>The <see cref="TComponent"/> instance</returns>
        TComponent GetUnityComponent<TComponent>(Entity entity)
            where TComponent : Component;

        /// <summary>
        /// Adds a Unity component to the GameObject bound to an Entity, if it is not already present.
        /// </summary>
        /// <param name="entity">The Entity.</param>
        /// <param name="onComponentAdded">Callback called when the Component is added.</param>
        /// <typeparam name="TComponent">The <exception cref="Component"> type.</exception></typeparam>
        /// <returns>The <see cref="TComponent"/> instance.</returns>
        TComponent AddMissingUnityComponent<TComponent>(Entity entity, Action<TComponent> onComponentAdded = null)
            where TComponent : Component;

        /// <summary>
        /// Removes a Unity component from the GameObject bound to an Entity, if it is present.
        /// </summary>
        /// <param name="entity">The Entity.</param>
        /// <typeparam name="TComponent">The <exception cref="Component"> type</exception></typeparam>
        void RemoveUnityComponent<TComponent>(Entity entity)
            where TComponent : Component;

        /// <summary>
        /// Returns the Entity associated with the Unity Component.
        /// </summary>
        /// <param name="component">The Component.</param>
        /// <returns>The Entity.</returns>
        Entity GetEntityFromUnityComponent(Component component);

        /// <summary>
        /// Returns an Entity associated with an asset.
        /// </summary>
        /// <param name="obj">The asset</param>
        /// <typeparam name="TObject">The asset type</typeparam>
        /// <returns>The Entity.</returns>
        Entity GetEntity<TObject>(TObject obj)
            where TObject : UnityEngine.Object;

        /// <summary>
        /// Returns an asset associated with an Entity.
        /// </summary>
        /// <param name="entity">The Entity.</param>
        /// <typeparam name="TObject">The asset type.</typeparam>
        /// <returns>The asset.</returns>
        TObject GetUnityObject<TObject>(Entity entity)
            where TObject : UnityEngine.Object;
    }
}
