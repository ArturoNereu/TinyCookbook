using System;
using System.Reflection;
using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Editor
{
    internal class AddComponentSearcherAdapter : SearcherAdapter
    {
        public override bool HasDetailsPanel => false;
        
        public AddComponentSearcherAdapter(string title) : base(title)
        {
        }

        public override VisualElement MakeItem()
        {
            var element = base.MakeItem();
            element.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (!(evt.target is VisualElement e))
                {
                    return;
                }
                
                if (e.GetFirstAncestorOfType<TemplateContainer>() is var container && container.userData is TypeSearcherItem)
                {
                    if (evt.clickCount != 1)
                    {
                        return;
                    }
                    
                    // Simulate a double-click when single-clicking on an item to immediately select it. 
                    using (var newEvt = MouseDownEvent.GetPooled(new Event(evt.imguiEvent)
                    {
                        mousePosition = evt.mousePosition,
                        clickCount = 2
                    }))
                    {
                        newEvt.target = container;
                        container.SendEvent(newEvt);
                    }
                }
                else
                {
                    // Force to expand or collapse by clicking anywhere on the item, and not only the expander.
                    var searcher = GetSearcherControl(e);
                    searcher?.GetType()?
                        .GetMethod("ExpandOrCollapse", BindingFlags.NonPublic | BindingFlags.Instance)?
                        .Invoke(searcher, new object[] {evt});
                }
            });   
            return element;
        }

        public override VisualElement Bind(VisualElement element, SearcherItem item, ItemExpanderState expanderState, string query)
        {
            var expander =  base.Bind(element, item, expanderState, string.IsNullOrEmpty(query) ? query : query.Replace("^", ""));

            using (var pooledList = ListPool<VisualElement>.GetDisposable())
            {
                var list = pooledList.List;
                list.Add(element.Q<VisualElement>("labelsContainer"));
                element.Query<Label>().OfType<VisualElement>().ToList(list);

                switch (expanderState)
                {
                    case ItemExpanderState.Hidden:
                        break;
                    case ItemExpanderState.Collapsed:
                        foreach (var e in list)
                        {
                            e.AddToClassList("Collapsed");
                            e.RemoveFromClassList("Expanded");
                        }
                        break;
                    case ItemExpanderState.Expanded:
                        foreach (var e in list)
                        {
                            e.RemoveFromClassList("Collapsed");
                            e.AddToClassList("Expanded");
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(expanderState), expanderState, null);
                }
            }

            return expander;
        }

        // This method is needed because the SearcherControl class is private.
        private static VisualElement GetSearcherControl(VisualElement element)
        {
            while (null != element)
            {
                var type = element.GetType();
                if (type.FullName == "UnityEditor.Searcher.SearcherControl")
                {
                    return element;
                }

                element = element.parent;
            }

            return null;
        }
    }
}
