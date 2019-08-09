using UnityEngine;

namespace Unity.Editor
{
    /// <summary>
    /// This node is backed by the UnityEngine transform hierarchy
    /// </summary>
    internal interface IParent
    {
        Transform Transform { get; }
    }
}
