using System;
using Unity.Authoring;
using Unity.Collections;
using Unity.Entities;

namespace Unity.Editor
{
    public interface IDataProvider<T>
    {
        Session Session { get; }
        T Data { get; set; }
        Entity MainTarget { get; }
        NativeArray<Entity> Targets { get; }

        TAttribute GetAttribute<TAttribute>() where TAttribute : Attribute;
    }
}
