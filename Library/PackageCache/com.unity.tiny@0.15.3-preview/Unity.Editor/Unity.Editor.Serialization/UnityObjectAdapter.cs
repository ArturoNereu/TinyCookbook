using Unity.Editor.Extensions;
using Unity.Properties;
using Unity.Serialization.Json;
using UnityEngine;

namespace Unity.Editor.Serialization
{
    internal class UnityObjectAdapter : JsonVisitorAdapter,
        IVisitAdapter<Object>
    {
        public UnityObjectAdapter(JsonVisitor visitor) : base(visitor)
        {
        }

        public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container, ref Object value, ref ChangeTracker changeTracker)
            where TProperty : IProperty<TContainer, Object>
        {
            if (!value || null == value)
            {
                return VisitStatus.Override;
            }

            var reference = value.ToAssetReference();
            var str = $"{{ \"Guid\": \"{reference.Guid:N}\", \"FileId\": {reference.FileId}, \"Type\": {reference.Type}}}";
            Append(property, str, (b, s) => b.Append(s));
            return VisitStatus.Override;
        }
    }
}
