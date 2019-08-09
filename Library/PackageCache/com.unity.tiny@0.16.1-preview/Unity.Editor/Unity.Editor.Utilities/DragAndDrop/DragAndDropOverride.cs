using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.Editor
{
    internal static class DragAndDropOverride
    {
        [InitializeOnLoadMethod]
        private static void RegisterAuthoringContext()
        {
            Application.BeginAuthoringProject += _ =>
            {
                Bridge.DragAndDropService.RegisterProjectBrowserHandler(OnProjectBrowserDrop);
                Bridge.DragAndDropService.RegisterSceneViewHandler(OnSceneViewDrop);
            };

            Application.EndAuthoringProject += _ => Bridge.DragAndDropService.UnregisterAllHandlers();
        }

        private static DragAndDropVisualMode OnSceneViewDrop(UnityEngine.Object obj, Vector3 worldPosition, Vector2 screenPosition, Transform parent, bool perform)
        {
            if (perform && DragAndDrop.objectReferences.OfType<GameObject>().Any(go =>

                    PrefabUtility.GetPrefabAssetType(go) != PrefabAssetType.NotAPrefab
                ))
            {
                EditorUtility.DisplayDialog("Unsupported operation", "Prefabs are not yet supported in the DOTS editor", "Ok");
                return DragAndDropVisualMode.Copy;
            }

            return DragAndDropVisualMode.Rejected;
        }

        private static DragAndDropVisualMode OnProjectBrowserDrop(int dragInstanceId, string dropUponPath, bool perform)
        {
            if (perform && DragAndDrop.objectReferences.OfType<GameObject>().Any())
            {
                EditorUtility.DisplayDialog("Unsupported operation", "Prefabs are not yet supported in the DOTS editor", "Ok");
                return DragAndDropVisualMode.Copy;    
            }

            return DragAndDropVisualMode.Rejected;
        }
    }
}