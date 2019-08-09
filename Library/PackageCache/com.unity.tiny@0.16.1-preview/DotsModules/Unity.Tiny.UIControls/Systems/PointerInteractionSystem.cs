using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Input;
using Unity.Tiny.Core2D;
using Unity.Tiny.UILayout;

namespace Unity.Tiny.UIControls
{
    /// <summary>
    ///  Updates PointerInteraction component based on the current state and position
    ///  of the pointer.
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(UIControlsSystem))]
    public class PointerInteractionSystem : ComponentSystem
    {
        private InputSystem input;

        protected override void OnStartRunning()
        {
            base.OnStartRunning();

            input = World.GetExistingSystem<InputSystem>();
        }

        protected override void OnUpdate()
        {
            var uiControls = new NativeList<Entity>(Allocator.Temp);
            GetInteractiveControlsSorted(ref uiControls);

            var pointers = PointerStateUtil.GetPointersState(input);

            ClearClickedState(uiControls);

            var pointersActivity = PointersActivity.Get();
            pointersActivity.ClearInvalidControls(EntityManager);

            for (int i = 0; i < pointers.Length; i++)
            {
                var pointer = pointers[i];

                float3 worldPoint = new float3(pointer.pos.x, pointer.pos.y, 0.0f);
                var pressedControl = pointersActivity.GetPressedControl(pointer.id);
                var hoverControl = pointersActivity.GetHoverControl(pointer.id);
                var controlUnderPointer = GetControlUnderPointer(uiControls, pointer.pos);

                if (pointer.down)
                {
                    pointersActivity.SetPressed(pointer.id, controlUnderPointer);
                    if (hoverControl != Entity.Null)
                        pointersActivity.ClearHover(pointer.id);
                    pointersActivity.SetHover(pointer.id, controlUnderPointer);
                }
                else if (pointer.up)
                {
                    if (controlUnderPointer != Entity.Null && controlUnderPointer == pressedControl &&
                        pointersActivity.GetPressCount(controlUnderPointer) == 1)
                    {
                        var interaction = EntityManager.GetComponentData<PointerInteraction>(controlUnderPointer);
                        interaction.clicked = true;
                        EntityManager.SetComponentData(controlUnderPointer, interaction);
                    }
                    pointersActivity.ClearPressed(pointer.id);
                    if (pointer.willGone)
                        pointersActivity.ClearHover(pointer.id);
                }
                else if (pointer.cancelled)
                {
                    pointersActivity.ClearPressed(pointer.id);
                    pointersActivity.ClearHover(pointer.id);
                }
                else
                {
                    if (hoverControl != controlUnderPointer)
                        pointersActivity.SetHover(pointer.id, controlUnderPointer);
                }
            }

            pointers.Dispose();
            UpdatePointerInteractionState(pointersActivity, uiControls);
            uiControls.Dispose();
        }

        private Rect GetControlBounds(Entity entity)
        {
            var rectSize = UILayoutService.GetRectTransformSizeOfEntity(this, entity);
            Rect bounds = new Rect(-rectSize.x / 2, -rectSize.y / 2, rectSize.x, rectSize.y);
            return bounds;
        }

        private bool IsPointInsideControlBounds(float2 worldPoint, Entity control)
        {
            var toWorldMatrix = TransformHelpers.ComputeWorldMatrix(this, control);
            var matrixInverted = math.inverse(toWorldMatrix);
            var bounds = GetControlBounds(control);
            var worldPointInverted = math.mul(matrixInverted, new float4(worldPoint.x, worldPoint.y, 0, 1));

            return bounds.Contains(worldPointInverted.xy);
        }

        private Entity GetControlUnderPointer(NativeList<Entity> uiControls, float2 pos)
        {
            for (int i = 0; i < uiControls.Length; i++)
            {
                var entity = uiControls[i];

                if (IsPointInsideControlBounds(pos, entity))
                    return entity;
            }

            return Entity.Null;
        }

        // Returns the entity with the Camera component that is referenced by the UICanvas.
        // If there is more entities with the UICanvas component, only the first one is used.
        private Entity GetUICamera()
        {
            Entity uiCamera = Entity.Null;
            Entities.ForEach((ref UICanvas canvas) =>
            {
                if (uiCamera != Entity.Null)
                    return;

                uiCamera = canvas.camera;
            });

            return uiCamera;
        }

        private void GetInteractiveControlsSorted(ref NativeList<Entity> uiControls)
        {
            Entity uiCamera = GetUICamera();

            if (uiCamera == Entity.Null || !EntityManager.HasComponent<DisplayListCamera>(uiCamera))
                return;

            var sortedEntities = EntityManager.GetBuffer<SortedEntity>(uiCamera);
            for (int i = sortedEntities.Length - 1; i >= 0; i--)
            {
                if (EntityManager.HasComponent<PointerInteraction>(sortedEntities[i].e) &&
                    !EntityManager.HasComponent<InactiveUIControl>(sortedEntities[i].e))
                    uiControls.Add(sortedEntities[i].e);
            }
        }

        private void ClearClickedState(NativeList<Entity> uiControls)
        {
            for (int i = 0; i < uiControls.Length; i++)
            {
                var control = uiControls[i];
                var interaction = EntityManager.GetComponentData<PointerInteraction>(control);
                interaction.clicked = false;
                EntityManager.SetComponentData(control, interaction);
            }
        }

        private void UpdatePointerInteractionState(PointersActivity pointersActivity, NativeList<Entity> uiControls)
        {
            for (int i = 0; i < uiControls.Length; i++)
            {
                var control = uiControls[i];
                var interaction = EntityManager.GetComponentData<PointerInteraction>(control);
                interaction.down = pointersActivity.IsPressed(control);
                interaction.over = pointersActivity.IsHover(control);
                EntityManager.SetComponentData(control, interaction);
            }
        }
    }
}
