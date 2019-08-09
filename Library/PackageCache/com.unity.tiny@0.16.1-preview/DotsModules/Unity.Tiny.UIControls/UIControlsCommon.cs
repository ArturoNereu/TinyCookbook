using Unity.Entities;
using Unity.Tiny.Core2D;

namespace Unity.Tiny.UIControls
{
    public static class UIControlsCommon
    {
        public static Color GetAndCaptureInitialColor(EntityManager mgr, EntityCommandBuffer ecb, Color currentColor, Entity entity)
        {
            if (mgr.HasComponent<InitialColor>(entity))
                return mgr.GetComponentData<InitialColor>(entity).color;
            else
            {
                ecb.AddComponent(entity, new InitialColor()
                {
                    color = currentColor
                });

                return currentColor;
            }
        }

        public static Entity GetSprite2DRendererEntity(EntityManager mgr, Entity entity, Entity entityAlt)
        {
            if (entity != Entity.Null && mgr.HasComponent<Sprite2DRenderer>(entity))
                return entity;

            if (entityAlt != Entity.Null && mgr.HasComponent<Sprite2DRenderer>(entityAlt))
                return entityAlt;

            return Entity.Null;
        }

        public static Color GetTransitionColor(ColorTintTransition transition, TransitionType type)
        {
            switch (type)
            {
                case TransitionType.Normal:
                    return transition.normal;
                case TransitionType.Hover:
                    return transition.hover;
                case TransitionType.Pressed:
                    return transition.pressed;
                case TransitionType.Disabled:
                    return transition.disabled;
                default:
                    break;
            }

            return new Color();
        }

        public static Entity GetTransitionSprite(SpriteTransition transition, TransitionType type)
        {
            switch (type)
            {
                case TransitionType.Normal:
                    return transition.normal;
                case TransitionType.Hover:
                    return transition.hover;
                case TransitionType.Pressed:
                    return transition.pressed;
                case TransitionType.Disabled:
                    return transition.disabled;
                default:
                    break;
            }

            return Entity.Null;
        }

        public static void ApplyTransition(EntityManager mgr, EntityCommandBuffer ecb, Entity target, Entity transition, TransitionType type)
        {
            if (!mgr.HasComponent<Sprite2DRenderer>(target))
                return;

            if (transition == Entity.Null)
                return;


            var renderer = mgr.GetComponentData<Sprite2DRenderer>(target);

            if (mgr.HasComponent<SpriteTransition>(transition))
            {
                var tr = mgr.GetComponentData<SpriteTransition>(transition);
                renderer.sprite = GetTransitionSprite(tr, type);
                ecb.SetComponent(target, renderer);
            }

            if (mgr.HasComponent<ColorTintTransition>(transition))
            {
                var tr = mgr.GetComponentData<ColorTintTransition>(transition);

                var initialColor = GetAndCaptureInitialColor(mgr, ecb, renderer.color, target);
                renderer.color = initialColor * GetTransitionColor(tr, type);
                ecb.SetComponent(target, renderer);
            }
        }

        public static TransitionType GetTransitionType(PointerInteraction interaction)
        {
            if (interaction.down && interaction.over)
            {
                return TransitionType.Pressed;
            }
            else if (interaction.over)
            {
                return TransitionType.Hover;
            }
            return TransitionType.Normal;
        }
    }
}
