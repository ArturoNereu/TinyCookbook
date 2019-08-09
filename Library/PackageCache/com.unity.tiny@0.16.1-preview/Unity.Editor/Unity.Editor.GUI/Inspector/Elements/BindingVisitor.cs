using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.Entities;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Unity.Editor
{
    internal class BindingVisitor<TData> : PropertyVisitor
        where TData : struct
    {
        private static readonly StringBuilder m_Path = new StringBuilder(64, 256);

        private readonly Dictionary<string, List<VisualElement>> m_Map;
        private readonly List<BindableElement> m_BindableElements;
        private readonly DataElement<TData> m_Root;
        public Entity Entity { get; set; }

        public BindingVisitor(DataElement<TData> root)
        {
            m_Root = root;
            m_Map = new Dictionary<string, List<VisualElement>>();
            m_BindableElements = new List<BindableElement>();
            root.Query<BindableElement>().Where(s => !string.IsNullOrEmpty(s.bindingPath)).ToList(m_BindableElements);
            foreach (var bindable in m_BindableElements)
            {
                if (!m_Map.TryGetValue(bindable.bindingPath, out var list))
                {
                    m_Map[bindable.bindingPath] = list = new List<VisualElement>();
                }

                list.Add(bindable);

                // compute "whole" path using parents
                var fullpath = ListPool<string>.Get();

                try
                {
                    fullpath.Add(bindable.bindingPath);
                    var parent = bindable.parent;
                    if (null != parent)
                    {
                        while (null != parent && parent != root)
                        {
                            if (parent is IBindable bParent)
                            {
                                if (!string.IsNullOrEmpty(bParent.bindingPath))
                                {
                                    fullpath.Add(bParent.bindingPath);
                                }
                            }

                            parent = parent.parent;
                        }

                        if (fullpath.Count > 1)
                        {
                            var fullPath = string.Join(".", ((IEnumerable<string>) fullpath).Reverse());
                            if (!m_Map.TryGetValue(fullPath, out var fullPathList))
                            {
                                fullPathList = new List<VisualElement>();
                                m_Map[fullPath] = fullPathList;
                            }

                            fullPathList.Add(bindable);
                        }
                    }
                }
                finally
                {
                    ListPool<string>.Release(fullpath);
                }
            }
        }

        private void Add(string str)
        {
            if (m_Path.Length > 0)
            {
                m_Path.Append('.');
            }

            m_Path.Append(str);
        }

        private void Remove(string str)
        {
            var length = m_Path.Length;
            if (length == str.Length)
            {
                m_Path.Clear();
            }
            else
            {
                m_Path.Remove(length - str.Length - 1, str.Length + 1);
            }
        }

        private bool Compare(string str, int startIndex)
        {
            if (startIndex + str.Length >= Length)
            {
                return false;
            }

            var index = startIndex;
            for (var i = 0; i < str.Length; ++i, ++index)
            {
                if (m_Path[index] != str[i])
                {
                    return false;
                }
            }

            return true;
        }

        private int Length => m_Path.Length;
        private int m_Offset;

        public void ResetOffsets()
        {
            m_Offset = 0;
        }

        protected override VisitStatus Visit<TProperty, TContainer, TValue>(TProperty property, ref TContainer container, ref TValue value,
            ref ChangeTracker changeTracker)
        {
            var propName = property.GetName();
            Add(propName);
            var path = m_Path.ToString();
            // Try mapping to the "full path"
            if (m_Map.TryGetValue(path, out var fullPathElements))
            {
                foreach (var element in fullPathElements)
                {
                    // TODO: optimise this
                    var method = typeof(BindingVisitor<>).MakeGenericType(typeof(TData)).GetMethod(nameof(GetUpdater), BindingFlags.NonPublic | BindingFlags.Instance)
                        .MakeGenericMethod(typeof(TValue));
                    method.Invoke(this, new object[] {element});
                }
            }

            m_Offset += Unsafe.SizeOf<TValue>();
            Remove(propName);
            return VisitStatus.Handled;
        }

        protected override VisitStatus BeginContainer<TProperty, TContainer, TValue>(TProperty property, ref TContainer container,
            ref TValue value, ref ChangeTracker changeTracker)
        {
            var propName = property.GetName();
            Add(propName);
            var path = m_Path.ToString();
            // Try mapping to the "full path"
            if (m_Map.TryGetValue(path, out var fullPathElements))
            {
                foreach (var element in fullPathElements)
                {
                    // TODO: optimise this
                    var method = typeof(BindingVisitor<>).MakeGenericType(typeof(TData)).GetMethod(nameof(GetUpdater), BindingFlags.NonPublic | BindingFlags.Instance)
                        .MakeGenericMethod(typeof(TValue));
                    method.Invoke(this, new object[] {element});
                }
            }

            PropertyContainer.Visit(ref value, this);
            Remove(propName);
            return VisitStatus.Override;
        }

        private void GetUpdater<TValue>(VisualElement element)
            where TValue : struct
        {
            if (TranslatorFactory<TValue>.TryGetUpdater(element, out var updater))
            {
                updater.SetUpdaters(element, m_Root, m_Offset, m_Root is StructDataElement<TData, TValue> buffer ? buffer.Index : -1);
            }
        }
    }
}
