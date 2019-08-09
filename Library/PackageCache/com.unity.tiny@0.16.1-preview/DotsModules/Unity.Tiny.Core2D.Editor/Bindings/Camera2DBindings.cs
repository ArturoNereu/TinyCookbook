using JetBrains.Annotations;
using Unity.Entities;
using Unity.Editor.Bindings;
using Unity.Tiny.Core2DTypes.Editor;

namespace Unity.Tiny.Core2D.Editor
{
    [UsedImplicitly]
    internal class Camera2DBindings : IEntityBinding,
        IComponentBinding<Camera2D>,
        IBindingDependency<ParentBindings>
    {
        public void LoadBinding(Entity entity, IBindingContext context)
        {
            context.AddMissingUnityComponent<UnityEngine.Camera>(entity);
        }

        public void UnloadBinding(Entity entity, IBindingContext context)
        {
            context.RemoveUnityComponent<UnityEngine.Camera>(entity);
        }

        public void TransferToUnityComponents(Entity entity, IBindingContext context)
        {
            var camera = context.GetUnityComponent<UnityEngine.Camera>(entity);
            var camera2D = context.GetComponentData<Camera2D>(entity);

            SetUnsupportedFields(camera);
            camera.clearFlags = camera2D.clearFlags.Convert();
            camera.backgroundColor = camera2D.backgroundColor.Convert();
            camera.orthographicSize = camera2D.halfVerticalSize;

            // TODO: map culling mask..
            //camera.cullingMask = tinyCamera.layerMask;
            camera.rect = camera2D.rect.Convert();
            camera.depth = camera2D.depth;

            if (context.HasComponent<Camera2DClippingPlanes>(entity))
            {
                var clippingPlanes = context.GetComponentData<Camera2DClippingPlanes>(entity);
                camera.nearClipPlane = clippingPlanes.near;
                camera.farClipPlane = clippingPlanes.far;
            }
            else
            {
                camera.nearClipPlane = -100000.0f;
                camera.farClipPlane = 100000.0f;
            }

            if (context.HasComponent<Camera2DAxisSort>(entity))
            {
                var axisSort = context.GetComponentData<Camera2DAxisSort>(entity);
                camera.transparencySortMode = UnityEngine.TransparencySortMode.CustomAxis;
                camera.transparencySortAxis = axisSort.axis;
            }
            else
            {
                camera.transparencySortMode = UnityEngine.TransparencySortMode.Default;
            }
        }

        public void TransferFromUnityComponents(Entity entity, IBindingContext context)
        {
            var camera = context.GetUnityComponent<UnityEngine.Camera>(entity);
            SetUnsupportedFields(camera);
            context.SetComponentData(entity, new Camera2D()
            {
                clearFlags = camera.clearFlags.Convert(),
                backgroundColor = camera.backgroundColor.Convert(),
                halfVerticalSize = camera.orthographicSize,
                rect = camera.rect.Convert(),
                depth = camera.depth
            });

            if (camera.transparencySortMode == UnityEngine.TransparencySortMode.Default)
            {
                context.RemoveComponent<Camera2DAxisSort>(entity);
            }
            else
            {
                context.SetComponentData(entity, new Camera2DAxisSort
                {
                    axis = camera.transparencySortAxis
                });
            }
        }

        private static void SetUnsupportedFields(UnityEngine.Camera camera)
        {
            camera.orthographic = true;
            camera.useOcclusionCulling = false;
            camera.allowHDR = false;
            camera.allowMSAA = false;
            camera.allowDynamicResolution = false;
        }
    }
}
