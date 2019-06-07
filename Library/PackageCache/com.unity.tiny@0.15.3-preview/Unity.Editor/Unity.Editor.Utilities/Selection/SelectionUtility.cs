using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.Editor
{
    internal static class SelectionUtility
    {
        public static Guid[] GetEntityGuidSelection()
        {
            var items = Selection.instanceIDs.Select(EditorUtility.InstanceIDToObject);
            return Filter(items).ToArray();
        }
        
        public static bool IsEntitySelectionEmpty()
        {
            var items = Selection.instanceIDs.Select(EditorUtility.InstanceIDToObject);
            return !Filter(items).Any();
        }

        private static IEnumerable<Guid> Filter(IEnumerable<Object> objects)
        {
            foreach (var obj in objects)
            {
                if (obj is GameObject go)
                {
                    if (null != go)
                    {
                        var view = go.GetComponent<EntityReference>();
                        if (null != view)
                        {
                            yield return view.Guid;
                        }
                    }
                }

                if (obj is Component component)
                {
                    if (null != component)
                    {
                        var view = component.GetComponent<EntityReference>();
                        if (null != view)
                        {
                            yield return view.Guid;
                        }
                    }
                }
            }
        }
    }
}
