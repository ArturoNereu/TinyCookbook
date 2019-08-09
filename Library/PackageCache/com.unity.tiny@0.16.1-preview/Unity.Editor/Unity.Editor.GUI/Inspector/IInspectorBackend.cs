using System.Collections.Generic;
using UnityEditor;

namespace Unity.Editor
{
    internal interface IInspectorBackend<T>
    {
        InspectorMode Mode { get; set; }
        List<T> Targets { get; }

        void OnCreated();
        void Build();
        void OnDestroyed();
        void Reset();
    }
}
