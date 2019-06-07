using System;
using Unity.Authoring.Core;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Tiny.Core;
using Unity.Tiny.Debugging;

#if UNITY_EDITOR
using System.ComponentModel;
#endif

namespace Unity.Tiny.Core2D
{
    /// <summary>
    /// This component is required for an entity to be part of the transform hierarchy.
    /// Entities without this component are not rendered.
    /// A root entity's parent entity can be NONE.
    /// </summary>
    public struct Parent : IComponentData
    {
        public Entity Value;
    }

    /// <summary>
    ///  This is an optional component that specifies a position transform.
    ///  If it is not attached to the entity, no local translation is assumed.
    /// </summary>
    [WriteGroup(typeof(LocalToParent))]
    public struct Translation : IComponentData
    {
        public float3 Value;
    }

    /// <summary>
    ///  This is an optional component that specifies a quaternion rotation transform.
    ///  If it is not added to the entity, the unit quaternion rotation is assumed.
    /// </summary>
    [WriteGroup(typeof(LocalToParent))]
    public struct Rotation : IComponentData
    {
        public static Rotation Default { get; } = new Rotation()
        {
            Value = quaternion.identity
        };

        public quaternion Value;
    }

    /// <summary>
    ///  This is an optional component that specifies a three-axis scale transform.
    ///  It is optional. If it is not added to the entity, the unit scale (1,1,1) is assumed.
    /// </summary>
    [WriteGroup(typeof(LocalToParent))]
    public struct NonUniformScale : IComponentData
    {
        public static NonUniformScale Default { get; } = new NonUniformScale()
        {
            Value = new float3(1f)
        };

        public NonUniformScale(float s = 1.0f)
        {
            Value.x = s;
            Value.y = s;
            Value.z = s;
        }

        public float3 Value;
    }

    /// <summary>
    ///  This is an optional component that specifies a uniform scale transform.
    ///  It is optional. If it is not added to the entity, the unit scale (1,1,1) is assumed.
    /// </summary>
    [WriteGroup(typeof(LocalToParent))]
    public struct Scale : IComponentData
    {
        public static Scale Default { get; } = new Scale()
        {
            Value = 1f
        };

        public Scale(float s = 1.0f)
        {
            Value = s;
        }

        public float Value;
    }

    /// <summary>
    ///  The UpdateLocalTransformSystem system adds and updates this component, which
    ///  provides direct access to the cached object space transform. The object
    ///  transform is computed from the contents of the NonUniformScale,
    ///  Rotation, and Translation components. This component
    ///  is readable and writeable.
    /// </summary>
    [WriteGroup(typeof(LocalToWorld))]
    public struct LocalToParent : IComponentData
    {
        public static LocalToParent Default { get; } = new LocalToParent
        {
            Value = float4x4.identity
        };

        public float4x4 Value;
    }

    /// <summary>
    ///  The UpdateWorldTransformSystem system adds and updates this component, which
    ///  provides direct access to the cached Object to World space transform. This
    ///  component is readable and writeable.
    /// </summary>
    [HideInInspector]
    public struct LocalToWorld : IComponentData
    {
        public static LocalToWorld Default { get; } = new LocalToWorld
        {
            Value = float4x4.identity
        };

        public float4x4 Value;
    }

    /// <summary>
    ///  This is a flag component that marks the start of a sorting group in a hierarchy.
    ///  You only need to add this component to the head node (entity) in a group, but
    ///  that entity must also have a Transform component.
    ///
    ///  Sorting groups allow you to sort the group head's descendants indepenently.
    ///  Whatever sorting you apply to the overall hierarchy also happens locally
    ///  within the group.
    /// </summary>
    public struct SortingGroup : IComponentData
    {
    }

    /// <summary>
    ///  This is a flag component that marks an entity's transform as static. The
    ///  entity you add it to must have a Transform component.
    ///  If an entity transform is marked as static, both its object and world
    ///  transforms are computed once and not updated again.
    ///  You can remove the static transform marker at any time to resume transform
    ///  computations.
    ///  Note that the TransformStatic tag has no effect on uncached compute functions.
    /// </summary>
    public struct TransformStatic : IComponentData
    {
    }

    internal struct PrivateTransformStatic : ISystemStateComponentData
    {
    }

    /// <summary>
    ///  This component is read by the DisplayList system for tracking sorting group membership.
    /// It is automatically added by the UpdateWorldTransformSystem;
    /// </summary>
    public struct PrivateTransformData : ISystemStateComponentData
    {
        public Entity inSortingGroup;
        public uint flags;
    }

    /// <summary>
    ///  This component tags a node with two additional values that allow sorting
    ///  by layer and order. The default value for both is 0. The entity you add
    ///  this component to must have a Transform component.
    ///
    ///  Entities with a higher layer value overlay entities with a lower layer value.
    ///  The order value defines how entities with the same layer value are ordered.
    ///  As with layers, entities with a higher order value overlay those with a lower
    ///  order value.
    ///
    ///  Layer sorting happens before order sorting, and both happen before axis
    ///  sorting. Axis sorting only happens when entities share the same layer and
    ///  order values.
    ///
    ///  LayerSorting values are not propagated through the hierarchy.
    /// </summary>
    public struct LayerSorting : IComponentData
    {
        /// <summary>
        ///  First, sort by layer.
        /// </summary>
        public short layer;

        /// <summary>
        ///  If layer values are equal, sort by order.
        /// </summary>
        public short order;

        /// <summary>
        /// Id that maps to UnityEngine.SortingLayer.id
        /// </summary>
        public int id;
    }

#if UNITY_EDITOR
    // upgrade helpers

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Renamed (UnityUpgradable) -> Parent", true)]
    public struct TransformNode : IComponentData
    {
        [Obsolete("Renamed (UnityUpgradable) -> Value", true)]
        public Entity parent;
    }

    [Obsolete("Renamed (UnityUpgradable) -> Translation", true)]
    public struct TransformLocalPosition : IComponentData
    {
        [Obsolete("Renamed (UnityUpgradable) -> Value", true)]
        public float3 position;
    }

    [Obsolete("Renamed (UnityUpgradable) -> Rotation", true)]
    public struct TransformLocalRotation : IComponentData
    {
        [Obsolete("Renamed (UnityUpgradable) -> Value", true)]
        public quaternion rotation;
    }

    [Obsolete("Renamed (UnityUpgradable) -> NonUniformScale", true)]
    public struct TransformLocalScale : IComponentData
    {
        [Obsolete("Renamed (UnityUpgradable) -> Value", true)]
        public float3 scale;
    }

    [Obsolete("Renamed (UnityUpgradable) -> LocalToWorld", true)]
    public struct TransformObjectToWorld : IComponentData
    {
        [Obsolete("Renamed (UnityUpgradable) -> Value", true)]
        public float4x4 matrix;
    }

    [Obsolete("Renamed (UnityUpgradable) -> LocalToParent", true)]
    public struct TransformLocal : IComponentData
    {
        [Obsolete("Renamed (UnityUpgradable) -> Value", true)]
        public float4x4 matrix;
    }
#endif
}
