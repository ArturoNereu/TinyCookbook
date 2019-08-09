using System;

namespace Unity.Tiny
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class EntityWithComponentsAttribute : Attribute
    {
        public Type[] Types { get; }

        public EntityWithComponentsAttribute(params Type[] types)
        {
            Types = types;
        }
    }
}
