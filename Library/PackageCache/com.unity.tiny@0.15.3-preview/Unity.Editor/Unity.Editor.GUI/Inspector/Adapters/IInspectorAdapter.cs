using Unity.Authoring;
using Unity.Collections;
using Unity.Properties;

namespace Unity.Editor
{
    internal abstract class InspectorAdapter<T> : IPropertyVisitorAdapter
        where T : struct
    {
        protected readonly InspectorVisitor<T> Visitor;
        protected readonly InspectorContext Context;
        protected readonly NativeArray<T> Targets;
        protected readonly Session Session;

        protected InspectorAdapter(InspectorVisitor<T> visitor)
        {
            Visitor = visitor;
            Context = visitor.Context;
            Targets = visitor.Targets;
            Session = visitor.Session;
        }
    }
}
