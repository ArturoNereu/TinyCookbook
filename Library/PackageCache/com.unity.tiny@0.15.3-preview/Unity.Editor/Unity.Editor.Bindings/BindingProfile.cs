using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;

namespace Unity.Editor.Bindings
{
    internal sealed class BindingProfile
    {
        public readonly string Name;
        public readonly IEntityBinding Binding;
        private readonly int[] m_RequiredComponentTypes;
        private readonly int[] m_ExcludeComponentTypes;


        public BindingProfile(IEntityBinding instance, int[] required, int[] exclude)
        {
            Name = instance.GetType().Name;
            Binding = instance;
            m_RequiredComponentTypes = required;
            m_ExcludeComponentTypes = exclude;
        }

        public bool IsValidBinding(EntityManager manager, Entity entity)
        {
            for (var i = 0; i < m_RequiredComponentTypes.Length; ++i)
            {
                var index = m_RequiredComponentTypes[i];
                if (!manager.HasComponent(entity, TypeManager.GetType(index)))
                {
                    return false;
                }
            }

            for (var i = 0; i < m_ExcludeComponentTypes.Length; ++i)
            {
                var index = m_ExcludeComponentTypes[i];
                if (manager.HasComponent(entity, TypeManager.GetType(index)))
                {
                    return false;
                }
            }

            return true;
        }

        public void LoadBinding(Entity entity, IBindingContext context)
        {
            Binding.LoadBinding(entity, context);
        }

        public void UnloadBinding(Entity entity, IBindingContext context)
        {
            Binding.UnloadBinding(entity, context);
        }

        public void TransferToUnity(Entity entity, IBindingContext context)
        {
            Binding.TransferToUnityComponents(entity, context);
        }

        public void TransferFromUnity(Entity entity, IBindingContext context)
        {
            Binding.TransferFromUnityComponents(entity, context);
        }

        public IEnumerable<Type> RequiredComponentTypes => m_RequiredComponentTypes.Select(TypeManager.GetType);
        public IEnumerable<Type> ExcludeComponentTypes => m_ExcludeComponentTypes.Select(TypeManager.GetType);
    }
}
